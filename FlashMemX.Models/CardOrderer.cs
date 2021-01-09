using System.Collections.Generic;

namespace FlashMemX.Core
{
    public abstract class CardOrderer
    {
        public abstract void Add(CardMetadata metadata);
        public abstract List<CardMetadata> ToList();
    }
}