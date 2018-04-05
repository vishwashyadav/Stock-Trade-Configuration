using StockTradeConfiguration.Models.APIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Stock_Trade_Configuration.Views
{
    /// <summary>
    /// Interaction logic for APIInfoEditor.xaml
    /// </summary>
    public partial class APIInfoEditor : Window
    {
        public APIInfoEditor()
        {
            InitializeComponent();
        }

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var grid = sender as Grid;
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { });

            var objInstance = grid.DataContext;
            if (objInstance == null)
                return;

            var properties = objInstance.GetType().GetProperties(System.Reflection.BindingFlags.Instance | BindingFlags.Public);
            int rowNumber = 0;
            
            foreach (var property in properties)
            {
                var attrib = property.CustomAttributes.FirstOrDefault(s => s.AttributeType == typeof(APIRequiredPropertyAttribute));
                if(attrib!=null)
                {
                    var apiAttribute = property.GetCustomAttribute<APIRequiredPropertyAttribute>();
                    grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                    grid.Children.Add(GetDisplayElement(property, apiAttribute, rowNumber, 0));
                    grid.Children.Add(GetEditorControl(property, apiAttribute, rowNumber++, 1));
                }
            }

            
        }

        FrameworkElement GetDisplayElement(PropertyInfo prop, APIRequiredPropertyAttribute attrib, int row, int column)
        {
            string name = !string.IsNullOrEmpty(attrib.Name) ? attrib.Name : prop.Name;
            TextBlock textBlock = new TextBlock()
            {
                Text = name
            };
            textBlock.Margin = new Thickness(5);
            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, column);
            return textBlock;
        }

        FrameworkElement GetEditorControl(PropertyInfo prop, APIRequiredPropertyAttribute attrib, int row, int column)
        {
            Binding textBinding = new Binding(prop.Name);
            textBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            TextBox textBox = new TextBox();
            textBox.SetBinding(TextBox.TextProperty, textBinding);
            textBox.Margin = new Thickness(5);
            Grid.SetRow(textBox, row);
            Grid.SetColumn(textBox, column);

            return textBox;
        }
    }
}
