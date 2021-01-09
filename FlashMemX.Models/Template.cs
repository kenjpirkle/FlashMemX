using System.Collections.Generic;

namespace FlashMemX.Core
{
    public class Template
    {
        public Template()
        {
            Front = new List<int>();
            Back = new List<int>();
        }

        public long SchemaId { get; set; }

        public IList<int> Front { get; }
        public IList<int> Back { get; }
    }
}
