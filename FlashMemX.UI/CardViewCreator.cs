using FlashMemX.Core;

namespace FlashMemX.UI
{
    internal class CardViewCreator
    {
        private readonly CustomCardViewBuilder customCardViewBuilder;

        public CardViewCreator()
        {
            customCardViewBuilder = new CustomCardViewBuilder();
        }

        public CardView Create(CardMetadata metadata)
        {
            return customCardViewBuilder.Create(metadata);
        }
    }
}
