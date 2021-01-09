using System.Data.SQLite;
using FlashMemX.Core;

namespace FlashMemX.Core
{
    internal class DataRowReader
    {
        public CardMetadata ReadCardMetaData(SQLiteDataReader dataReader, long msDueBefore)
        {
            long id = dataReader.GetInt64(0);
            long deckId = dataReader.GetInt64(1);
            long schemaId = dataReader.GetInt64(2);
            long contentId = dataReader.GetInt64(3);
            long templateId = dataReader.GetInt64(4);
            int easiness = dataReader.GetInt32(5);
            long nextReview = dataReader.GetInt64(6);
            long actualReview = dataReader.GetInt64(7);
            long lastReview = dataReader.GetInt64(8);
            int stage = dataReader.GetInt32(9);
            float percentageOverdue = (float)(msDueBefore - lastReview) / (nextReview - lastReview);
            int reps = dataReader.GetInt32(10);
            int sReps = dataReader.GetInt32(11);
            int RsReps = dataReader.GetInt32(12);
            int averageEasiness = dataReader.GetInt32(13);

            string[] recentEasinesses = dataReader.GetString(14).Split(' ');
            int recentAverageEasiness = 0;

            for(int i = 0; i < 3; ++i)
                recentAverageEasiness += int.Parse(recentEasinesses[i]);

            recentAverageEasiness /= 3;

            float averageSuccessRate = ((float)(double)(sReps / reps) + (RsReps / (reps < 3 ? reps : 3))) / 2;

            return new CardMetadata()
            {
                Id = id,
                DeckId = deckId,
                ContentId = contentId,
                TemplateId = templateId,
                Easiness = easiness,
                NextReview = nextReview,
                ActualReview = actualReview,
                LastReview = lastReview,
                Stage = stage,
                PercentageOverdue = percentageOverdue,
                AverageEasiness = (averageEasiness + recentAverageEasiness) / 2,
                AverageSuccessRate = averageSuccessRate
            };
        }

        public CardMetadata ReadNewCardMetaData(SQLiteDataReader dataReader)
        {
            return new CardMetadata()
            {
                Id = dataReader.GetInt64(0),
                DeckId = dataReader.GetInt64(1),
                SchemaId = dataReader.GetInt64(2),
                ContentId = dataReader.GetInt64(3),
                TemplateId = dataReader.GetInt64(4)
            };
        }
    }
}
