using System.Collections.Generic;
using FlashMemX.Core;

namespace FlashMemX.UI
{
    public class Cache
    {
        private readonly Dictionary<long, Template> templates;
        private readonly Dictionary<long, ContentSchema> contentSchemas;
        private readonly Dictionary<long, Content> content;

        public Cache(IList<CardMetadata> metadata, Database database)
        {
            templates = database.GetTemplates(metadata);
            contentSchemas = database.GetContentSchemas(metadata);
            content = database.GetContents(metadata);
        }

        public Template GetTemplate(long templateId)
        {
            return templates[templateId];
        }

        public ContentSchema GetContentSchema(long schemaId)
        {
            return contentSchemas[schemaId];
        }

        public Content GetContent(long contentId)
        {
            return content[contentId];
        }
    }
}
