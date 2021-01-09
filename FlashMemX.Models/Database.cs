using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using NodaTime;
using System.Data;
using System;

namespace FlashMemX.Core
{
    public class Database
    {
        private readonly string path;
        private readonly string connectionString;
        private readonly SQLiteConnection connection;
        private readonly SQLiteCommand command;
        private SQLiteTransaction transaction;
        private SQLiteDataReader dataReader;
        private readonly BlobCreator blobCreator;
        private readonly BlobReader blobReader;
        private readonly DataRowReader dataRowReader;

        public Database()
        {
            this.path =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "flashmemx.db");

            connectionString = $"Data Source ={path};Version=3;";
            connection = new SQLiteConnection(connectionString);
            command = new SQLiteCommand(connection);
            blobCreator = new BlobCreator();
            blobReader = new BlobReader();
            dataRowReader = new DataRowReader();

            if(!File.Exists(path))
                CreateDatabase();
        }

        ~Database()
        {
            command?.Dispose();
            transaction?.Dispose();
            dataReader?.Dispose();
            connection?.Dispose();
        }

        public IList<CardMetadata> GetCardMetadataDueBefore(long timeDueInMs)
        {
            var metaData = new List<CardMetadata>();

            Prepare(
                "SELECT " +
                    "ID, DECKID, SCHEMAID, CONTENTID, TEMPLATEID, EASINESS, NEXTREV," +
                    "ACTUALREV, LASTREV, STAGE, REPS, SREPS, RSREPS, AVGEASINESS," +
                    "RECENTAVGEASINESS FROM card_metadata " +
                        "WHERE STAGE > 0 AND NEXTREV < @1;"
            );

            SetParameters(timeDueInMs);

            dataReader = command.ExecuteReader();
            while(dataReader.Read())
                metaData.Add(dataRowReader.ReadCardMetaData(dataReader, timeDueInMs));

            CommitRead();

            return metaData;
        }

        public IList<CardMetadata> GetNewCardMetadata()
        {
            var metaData = new List<CardMetadata>();

            Prepare(
                "SELECT " +
                    "ID, DECKID, SCHEMAID, CONTENTID, TEMPLATEID FROM card_metadata " +
                        "WHERE STAGE = 0;"
            );

            dataReader = command.ExecuteReader();
            while(dataReader.Read())
                metaData.Add(dataRowReader.ReadNewCardMetaData(dataReader));

            CommitRead();

            return metaData;
        }

        public IList<CardMetadata> GetNewCardMetadata(CardOrderer cardOrderer)
        {
            Prepare(
                "SELECT " +
                    "ID, DECKID, SCHEMAID, CONTENTID, TEMPLATEID FROM card_metadata " +
                        "WHERE STAGE = 0;"
            );

            dataReader = command.ExecuteReader();
            while(dataReader.Read())
                cardOrderer.Add(dataRowReader.ReadNewCardMetaData(dataReader));

            CommitRead();

            return cardOrderer.ToList();
        }

        public Dictionary<long, Content> GetContents(IList<CardMetadata> metadata)
        {
            var contents = new Dictionary<long, Content>();

            Prepare(
                "SELECT SCHEMAID, CONTENT FROM content " +
                    "WHERE ID = @1;"
            );

            for(int i = 0; i < metadata.Count; ++i)
            {
                if(!contents.ContainsKey(metadata[i].ContentId))
                {
                    SetParameters(metadata[i].ContentId);

                    dataReader = command.ExecuteReader(CommandBehavior.SingleRow);
                    dataReader.Read();

                    contents.Add(
                        metadata[i].ContentId,
                        new CustomContent()
                        {
                            Id = metadata[i].ContentId,
                            SchemaId = dataReader.GetInt64(0),
                            Fields = blobReader.BlobToCustomContentFields(dataReader.GetBytes(1))
                        }
                    );
                }
            }

            CommitRead();
            return contents;
        }

        public Dictionary<long, ContentSchema> GetContentSchemas(IList<CardMetadata> metadata)
        {
            var contentSchemas = new Dictionary<long, ContentSchema>();

            Prepare(
                "SELECT SCHEMA FROM content_schema " +
                    "WHERE ID = @1;"
            );

            for(int i = 0; i < metadata.Count; ++i)
            {
                if(!contentSchemas.ContainsKey(metadata[i].SchemaId))
                {
                    SetParameters(metadata[i].SchemaId);

                    dataReader = command.ExecuteReader(CommandBehavior.SingleRow);
                    dataReader.Read();

                    ContentSchema schema = blobReader.BlobToContentSchema(dataReader.GetBytes(0));
                    contentSchemas.Add(metadata[i].SchemaId, schema);
                }
            }

            CommitRead();
            return contentSchemas;
        }

        public Dictionary<long, Template> GetTemplates(IList<CardMetadata> metadata)
        {
            var templates = new Dictionary<long, Template>();

            Prepare(
                "SELECT SCHEMAID, TEMPLATE FROM templates " +
                    "WHERE ID = @1;"
            );

            for(int i = 0; i < metadata.Count; ++i)
            {
                if(!templates.ContainsKey(metadata[i].TemplateId))
                {
                    SetParameters(metadata[i].TemplateId);
                    dataReader = command.ExecuteReader(CommandBehavior.SingleRow);
                    dataReader.Read();

                templates.Add(metadata[i].TemplateId, blobReader.BlobToTemplate(dataReader.GetBytes(1)));
                }
            }

            CommitRead();
            return templates;
        }

        public ContentSchema GetContentSchema(long id)
        {
            Prepare(
                "SELECT SCHEMA FROM content_schema WHERE ID = @1;"
            );

            SetParameters(id);
            dataReader = command.ExecuteReader(CommandBehavior.SingleRow);
            dataReader.Read();

            ContentSchema contentSchema = blobReader.BlobToContentSchema(dataReader.GetBytes(0));

            CommitRead();
            return contentSchema;
        }

        public Deck GetDeck(long id)
        {
            Prepare(
                "SELECT NAME, DESC, NEWCARDSPERDAY FROM decks " +
                    "WHERE ID = @1;"
            );

            SetParameters(id);
            dataReader = command.ExecuteReader(CommandBehavior.SingleRow);
            dataReader.Read();

            Deck deck = new Deck()
            {
                Id = id,
                Name = dataReader.GetString(0),
                Description = dataReader.GetString(1),
                NumNewCardsPerDay = dataReader.GetInt32(2)
            };

            CommitRead();
            return deck;
        }

        public void InsertDeck(Deck deck)
        {
            Prepare(
                "INSERT INTO decks(ID, NAME, DESC, NEWCARDSPERDAY)" +
                    "VALUES(@1, @2, @3, @4);"
            );

            long id = GetUniqueIdFromTable("decks");

            SetParameters(
                id,
                deck.Name,
                deck.Description,
                deck.NumNewCardsPerDay
            );

            CommitWrite();
        }

        public IList<long> InsertCustomContent(IList<Content> content)
        {
            var contentIds = new List<long>(content.Count);

            Prepare(
                "INSERT INTO content(ID, SCHEMAID, CONTENT)" +
                    "VALUES(@1, @2, @3);"
            );

            for(int i = 0; i < content.Count; ++i)
            {
                long id = GetUniqueIdFromTable("content");
                contentIds.Add(id);

                SetParameters(
                    id,
                    content[i].SchemaId,
                    blobCreator.Create(content[i])
                );

                command.ExecuteNonQuery();
            }

            Commit();
            return contentIds;
        }

        public void InsertCardMetadata(IList<CardMetadata> metaData)
        {
            Prepare(
                "INSERT INTO card_metadata(" +
                    "ID, DECKID, SCHEMAID, CONTENTID, TEMPLATEID, EASINESS," +
                    "NEXTREV, ACTUALREV, LASTREV, STAGE, REPS, SREPS," +
                    "RSREPS, AVGEASINESS, RECENTAVGEASINESS)" +
                        "VALUES(@1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11" +
                               "@12, @13, @14, @15);"
            );

            for(int i = 0; i < metaData.Count; ++i)
            {
                SetParameters(
                    GetUniqueIdFromTable("card_metadata"),
                    metaData[i].DeckId,
                    metaData[i].SchemaId,
                    metaData[i].ContentId,
                    metaData[i].TemplateId,
                    metaData[i].Easiness,
                    metaData[i].NextReview,
                    metaData[i].ActualReview,
                    metaData[i].LastReview,
                    metaData[i].Stage,
                    (int?)0,
                    (int?)0,
                    (int?)0,
                    (int?)0,
                    string.Empty
                );

                command.ExecuteNonQuery();
            }

            Commit();
        }

        public long InsertTemplate(Template template)
        {
            Prepare(
                "INSERT INTO templates(ID, SCHEMAID, TEMPLATE)" +
                    "VALUES(@1, @2, @3);"
            );

            long id = GetUniqueIdFromTable("templates");

            SetParameters(
                id,
                template.SchemaId,
                blobCreator.Create(template)
            );

            CommitWrite();
            return id;
        }

        public long InsertContentSchema(ContentSchema contentSchema)
        {
            Prepare(
                "INSERT INTO content_schema(ID, SCHEMA)" +
                    "VALUES(@1, @2);"
            );

            long id = GetUniqueIdFromTable("content_schema");

            SetParameters(
                id,
                blobCreator.Create(contentSchema)
            );

            CommitWrite();
            return id;
        }

        private void CreateDatabase()
        {
            const string commandString =

            #region Pragma values

                "PRAGMA temp_store = 2;" +

            #endregion

            #region stats table

                    "CREATE TABLE stats(" +
                        "TARGRET INT NOT NULL," +
                        "AVGEASE INT NOT NULL," +
                        "AVGEASECNT INT NOT NULL," +
                        "IVL1 INT NOT NULL," +
                        "IVL2 INT NOT NULL," +
                        "IVL3 INT NOT NULL," +
                        "IVL4 INT NOT NULL," +
                        "IVL5 INT NOT NULL," +
                        "IVL1CNT INT NOT NULL," +
                        "IVL2CNT INT NOT NULL," +
                        "IVL3CNT INT NOT NULL," +
                        "IVL4CNT INT NOT NULL," +
                        "IVL5CNT INT NOT NULL," +
                        "AVGRELEASE INT NOT NULL," +
                        "AVGRELEASESOFT INT NOT NULL," +
                        "AVGRELEASECNT INT NOT NULL," +
                        "AVGRELEASESOFTCNT INT NOT NULL," +
                        "NEWCARDSPERDECK INT NOT NULL);" +
                    "INSERT INTO stats(" +
                        "TARGRET,AVGEASE,AVGEASECNT,IVL1,IVL2," +
                        "IVL3,IVL4,IVL5,IVL1CNT,IVL2CNT,IVL3CNT," +
                        "IVL4CNT,IVL5CNT,AVGRELEASE,AVGRELEASESOFT," +
                        "AVGRELEASECNT,AVGRELEASESOFTCNT,NEWCARDSPERDECK)" +
                            "VALUES(9500,2500,1,60000,600000,86400000," +
                                   "172800000,518400000,1,1,1,1,1,500,750," +
                                   "1,1,50);" +

            #endregion

            #region rev_log table

                "CREATE TABLE rev_log(" +
                    "ID INT PRIMARY KEY NOT NULL," +
                    "CARDID INT NOT NULL," +
                    "QUALITY INT NOT NULL," +
                    "EASINESS INT NOT NULL," +
                    "NEXTREV INT NOT NULL," +
                    "ACTUALREV INT NOT NULL," +
                    "LASTREV INT NOT NULL," +
                    "STAGE INT NOT NULL," +
                    "TIME INT NOT NULL);" +

            #endregion

            #region decks table

                "CREATE TABLE decks(" +
                    "ID INT PRIMARY KEY NOT NULL," +
                    "NAME TEXT NOT NULL," +
                    "DESC TEXT NOT NULL," +
                    "NEWCARDSPERDAY INT NOT NULL);" +

            #endregion

            #region card_metadata table

                "CREATE TABLE card_metadata(" +
                    "ID INT PRIMARY KEY NOT NULL," +
                    "DECKID INT NOT NULL," +
                    "SCHEMAID INT NOT NULL," +
                    "CONTENTID INT NOT NULL," +
                    "TEMPLATEID INT NOT NULL," +
                    "EASINESS INT NOT NULL," +
                    "NEXTREV INT NOT NULL," +
                    "ACTUALREV INT NOT NULL," +
                    "LASTREV INT NOT NULL," +
                    "STAGE INT NOT NULL," +
                    "REPS INT NOT NULL," +
                    "SREPS INT NOT NULL," +
                    "RSREPS INT NOT NULL," +
                    "AVGEASINESS INT NOT NULL," +
                    "RECENTAVGEASINESS TEXT NOT NULL);" +
                "CREATE INDEX due_cards ON card_metadata(NEXTREV) WHERE STAGE < 4;" +
                "CREATE INDEX due_cards_decks ON card_metadata(DECKID, NEXTREV) WHERE STAGE < 4;" +
                "CREATE INDEX new_cards ON card_metadata(STAGE) WHERE STAGE = 4;" +
                "CREATE INDEX new_cards_decks ON card_metadata(DECKID) WHERE STAGE = 4;" +

            #endregion

            #region content_schema table

                "CREATE TABLE content_schema(" +
                    "ID INT PRIMARY KEY NOT NULL," +
                    "SCHEMA BLOB NOT NULL);" +

            #endregion

            #region templates table

                "CREATE TABLE templates(" +
                    "ID INT PRIMARY KEY NOT NULL," +
                    "SCHEMAID INT NOT NULL," +
                    "TEMPLATE BLOB NOT NULL);" +

            #endregion

            #region content table

                "CREATE TABLE content(" +
                    "ID INT PRIMARY KEY NOT NULL," +
                    "SCHEMAID INT NOT NULL," +
                    "CONTENT BLOB NOT NULL);";

            #endregion

            Prepare(commandString);

            CommitWrite();
        }

        private void Prepare(string commandText)
        {
            connection.Open();
            transaction = connection.BeginTransaction();
            command.CommandText = commandText;
        }

        private void SetParameters(params object[] values)
        {
            int i;
            for(i = 0; i < command.Parameters.Count; ++i)
                command.Parameters[i].Value = values[i];

            while(i < values.Length)
            {
                command.Parameters.Add(new SQLiteParameter($"@{i + 1}", values[i]));
                ++i;
            }
        }

        private void Commit()
        {
            transaction.Commit();
            connection.Close();
        }

        private void CommitWrite()
        {
            command.ExecuteNonQuery();
            transaction.Commit();
            connection.Close();
        }

        private void CommitRead()
        {
            dataReader.Close();
            transaction.Commit();
            connection.Close();
        }

        private long GetUniqueIdFromTable(string tableName)
        {
            long millisecondsId = SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds();

            var tempCommand = new SQLiteCommand(
                $"SELECT EXISTS(SELECT * FROM {tableName} WHERE ID = @ID)",
                connection);

            tempCommand.Parameters.AddWithValue("@ID", millisecondsId);

            long numRows = (long)tempCommand.ExecuteScalar();

            while(numRows != 0)
            {
                ++millisecondsId;
                tempCommand.Parameters[0].Value = millisecondsId;

                numRows = (long)tempCommand.ExecuteScalar();
            }

            return millisecondsId;
        }
    }
}
