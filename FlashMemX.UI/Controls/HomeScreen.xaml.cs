using System.Windows;
using System.Windows.Controls;

namespace FlashMemX.UI
{
    public partial class HomeScreen : UserControl
    {
        public HomeScreen()
        {
            InitializeComponent();
        }

        private void StudyButton_Click(object sender, RoutedEventArgs e)
        {
            ProgramManager.StartStudy();
        }

        private void CreateNewContentType_Click(object sender, RoutedEventArgs e)
        {
            ProgramManager.StartCreateSchema();
        }

        private void CreateNewTemplate_Click(object sender, RoutedEventArgs e)
        {
            ProgramManager.StartCreateTemplate();
        }
    }
}
