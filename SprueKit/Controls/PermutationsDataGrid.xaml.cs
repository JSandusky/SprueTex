using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for PermutationsDataGrid.xaml
    /// </summary>
    public partial class PermutationsDataGrid : UserControl
    {
        
        public PermutationsDataGrid()
        {
            InitializeComponent();

            DataContextChanged += PermutationsDataGrid_DataContextChanged;
            var documentManager = new IOCDependency<DocumentManager>();
            documentManager.Object.OnActiveDocumentChanged += Object_OnActiveDocumentChanged; ;
            Object_OnActiveDocumentChanged(documentManager.Object.ActiveDocument, null);
        }

        bool ignoreDataContextChange = false;
        private void PermutationsDataGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private void PermutationsChanged(object sender, EventArgs e)
        {
            ignoreDataContextChange = true;
            dataGrid.SetBinding(DataGrid.DataContextProperty, new Binding());
            dataGrid.SetBinding(DataGrid.DataContextProperty, binding_);
            ignoreDataContextChange = false;
        }

        Binding binding_;
        private void Object_OnActiveDocumentChanged(Document newDoc, Document oldDoc)
        {
            if (newDoc != null)
                dataGrid.SetBinding(DataGrid.DataContextProperty, binding_ = new Binding("Selection.MostRecentlySelected") { Source = newDoc, Delay = 50, Converter = new NeedToKnow(this) });
            else
                SetBinding(DataGrid.DataContextProperty, binding_ = new Binding("Selection.MostRecentlySelected") { Source = null });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                var permutable = dataGrid.DataContext as Data.IPermutable;
                var record = btn.Tag as Data.PermutationRecord;
                if (record != null && permutable != null)
                {
                    var targetProperty = permutable.GetType().GetProperty(record.Property);
                    if (targetProperty != null)
                        targetProperty.SetValue(permutable, record.Value.Value);
                }
            }
        }

        public class NeedToKnow : IValueConverter
        {
            object lastValue_;
            PermutationsDataGrid grid_;

            public NeedToKnow(PermutationsDataGrid grid)
            {
                grid_ = grid;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == lastValue_)
                {
                    return value;
                }
                else
                {
                    if (lastValue_ != null)
                    {
                        var permOld = lastValue_ as Data.IPermutable;
                        if (permOld != null)
                        {
                            permOld.PermutationsChanged -= grid_.PermutationsChanged;
                        }
                    }
                    if (value != null)
                    {
                        var permNew = value as Data.IPermutable;
                        if (permNew != null)
                            permNew.PermutationsChanged += grid_.PermutationsChanged;
                    }
                }
                lastValue_ = value;
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
