using System.Collections.Generic;

namespace FlashMemX.Core
{
    public class CustomContent : Content
    {
        public CustomContent()
        {
            Fields = new List<string>();
        }

        public IList<string> Fields { get; set; }
    }
}
