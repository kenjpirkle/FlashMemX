using System.Windows.Controls;
using System.Windows.Media;
using FlashMemX.Core;

namespace FlashMemX.UI
{
    internal class CustomCardViewBuilder
    {
        public CardView Create(CardMetadata metadata)
        {
            var cardView = new CardView();
            var template = ProgramManager.Cache.GetTemplate(metadata.TemplateId);
            var content = (CustomContent)ProgramManager.Cache.GetContent(metadata.ContentId);

            cardView.CardBackground.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#4286f4");

            for(int i = 0; i < template.Front.Count; ++i)
                cardView.ContentStackPanel.Children.Add(new TextBlock() { Text = content.Fields[template.Front[i]],
                                                                          HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                                                                          Margin = new System.Windows.Thickness(10) });

            return cardView;
        }
    }
}