using System.IO;

namespace FlashMemX.Core
{
    internal class BlobCreator
    {
        private MemoryStream memoryStream;
        private BinaryWriter binaryWriter;

        ~BlobCreator()
        {
            memoryStream?.Dispose();
            binaryWriter?.Dispose();
        }

        public byte[] Create(object data)
        {
            if (data is CustomContent content)
                return CustomContentToBlob(content);
            else if (data is ContentSchema schema)
                return ContentSchemaToBlob(schema);
            else if (data is Template template)
                return TemplateToBlob(template);
            else
                return new byte[0];
        }

        private void Prepare()
        {
            memoryStream = new MemoryStream();
            binaryWriter = new BinaryWriter(memoryStream);
        }

        private void Dispose()
        {
            memoryStream.Dispose();
            binaryWriter.Dispose();
        }

        private byte[] ContentSchemaToBlob(ContentSchema contentSchema)
        {
            Prepare();

            binaryWriter.Write(contentSchema.Name);
            binaryWriter.Write(contentSchema.Fields.Count);
            ContentField contentField;
            for (int i = 0; i < contentSchema.Fields.Count; ++i)
            {
                contentField = contentSchema.Fields[i];
                binaryWriter.Write(contentField.Name);
                binaryWriter.Write((byte)contentField.ContentFieldType);
            }

            byte[] bytes = memoryStream.ToArray();
            Dispose();
            return bytes;
        }

        private byte[] CustomContentToBlob(CustomContent customContent)
        {
            Prepare();

            binaryWriter.Write(customContent.Fields.Count);

            for (int i = 0; i < customContent.Fields.Count; ++i)
                binaryWriter.Write(customContent.Fields[i]);

            byte[] bytes = memoryStream.ToArray();
            Dispose();
            return bytes;
        }

        private byte[] TemplateToBlob(Template template)
        {
            Prepare();

            binaryWriter.Write(template.Front.Count);
            for (int i = 0; i < template.Front.Count; ++i)
                binaryWriter.Write(template.Front[i]);
            binaryWriter.Write(template.Back.Count);
            for (int i = 0; i < template.Back.Count; ++i)
                binaryWriter.Write(template.Back[i]);

            byte[] bytes = memoryStream.ToArray();
            Dispose();
            return bytes;
        }
    }
}
