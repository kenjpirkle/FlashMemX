using System.Collections.Generic;
using System.IO;
using FlashMemX.Core;

namespace FlashMemX.Core
{
    internal class BlobReader
    {
        private MemoryStream memoryStream;
        private BinaryReader binaryReader;

        ~BlobReader()
        {
            memoryStream?.Dispose();
            binaryReader?.Dispose();
        }

        private void Prepare(byte[] blob)
        {
            memoryStream = new MemoryStream(blob);
            binaryReader = new BinaryReader(memoryStream);
        }

        private void Dispose()
        {
            memoryStream.Dispose();
            binaryReader.Dispose();
        }

        public ContentSchema BlobToContentSchema(byte[] blob)
        {
            Prepare(blob);
            ContentSchema contentSchema = new ContentSchema();

            contentSchema.Name = binaryReader.ReadString();
            int numberFields = binaryReader.ReadInt32();
            for(int i = 0; i < numberFields; ++i)
            {
                contentSchema.Fields.Add(new ContentField()
                {
                    Name = binaryReader.ReadString(),
                    ContentFieldType = (ContentFieldType)binaryReader.ReadByte()
                });
            }

            Dispose();
            return contentSchema;
        }

        public IList<string> BlobToCustomContentFields(byte[] blob)
        {
            Prepare(blob);

            int numberFields = binaryReader.ReadInt32();
            List<string> fields = new List<string>(numberFields);
            for(int i = 0; i < numberFields; ++i)
                fields.Add(binaryReader.ReadString());

            Dispose();
            return fields;
        }

        public Template BlobToTemplate(byte[] blob)
        {
            Prepare(blob);
            Template template = new Template();

            int numFrontFields = binaryReader.ReadInt32();
            for(int i = 0; i < numFrontFields; ++i)
                template.Front.Add(binaryReader.ReadInt32());
            int numBackFields = binaryReader.ReadInt32();
            for(int i = 0; i < numBackFields; ++i)
                template.Back.Add(binaryReader.ReadInt32());

            Dispose();
            return template;
        }
    }
}
