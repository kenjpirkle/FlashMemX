using System.Windows;
using System.Windows.Controls;
using FlashMemX.Core;

namespace FlashMemX.UI
{
    public partial class SchemaField : UserControl
    {
        public SchemaField()
        {
            InitializeComponent();

            FieldTypeComboBox.Items.Add(ContentFieldType.Text);
            FieldTypeComboBox.Items.Add(ContentFieldType.Image);
            FieldTypeComboBox.Items.Add(ContentFieldType.Audio);
            FieldTypeComboBox.Items.Add(ContentFieldType.Video);

            DeleteButton.Visibility = Visibility.Hidden;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ((StackPanel)this.Parent).Children.Remove(this);
        }

        private void Card_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DeleteButton.Visibility = Visibility.Visible;
        }

        private void Card_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DeleteButton.Visibility = Visibility.Hidden;
        }
    }
}
