using System.Windows.Controls;
using System.Windows.Media;

namespace FlashMemX.UI
{
    public partial class CreateTemplateScreen : UserControl
    {
        public CreateTemplateScreen()
        {
            InitializeComponent();

            CardView front = new CardView();
            front.CardBackground.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");

            CardView back = new CardView();
            back.CardBackground.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");

            FrontCardView.Content = front;
            BackCardView.Content = back;
        }
    }
}
