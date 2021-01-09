using System.Windows;
using System.Windows.Controls;
using FlashMemX.Core;

namespace FlashMemX.UI
{
    public partial class CreateSchemaScreen : UserControl
    {
        public CreateSchemaScreen()
        {
            InitializeComponent();
        }

        private void AddFieldButton_Click(object sender, RoutedEventArgs e)
        {
            SchemaField schemaField = new SchemaField();
            FieldStackPanel.Children.Add(schemaField);
            schemaField.BringIntoView();
        }

        private void SaveNewSchema_Click(object sender, RoutedEventArgs e)
        {
            var contentSchema = new ContentSchema();
            contentSchema.Name = SchemaName.Text;

            SchemaField schemaField;

            for(int i = 0; i < FieldStackPanel.Children.Count; ++i)
            {
                schemaField = (SchemaField)FieldStackPanel.Children[i];
                contentSchema.Fields.Add(
                    new ContentField()
                    {
                        ContentFieldType = (ContentFieldType)schemaField.FieldTypeComboBox.SelectedIndex,
                        Name = schemaField.FieldNameTextBox.Text
                    }
                );
            }

            ProgramManager.Database.InsertContentSchema(contentSchema);

            FieldStackPanel.Children.Clear();
            SchemaName.Clear();
        }
    }
}
