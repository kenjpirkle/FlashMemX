using System.Collections.Generic;

namespace FlashMemX.Core
{
    public class ContentSchema
    {
        public ContentSchema()
        {
            Fields = new List<ContentField>();
        }

        public string Name { get; set; }
        public List<ContentField> Fields { get; set; }
    }
}
