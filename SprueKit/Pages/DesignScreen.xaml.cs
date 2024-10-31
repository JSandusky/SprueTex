using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using FirstFloor.ModernUI;

using SprueKit.Data;
using System.Collections.ObjectModel;

namespace SprueKit.Pages
{
    /// <summary>
    /// Screen where the user will spend the majority of their time
    /// </summary>
    [IOCInitialized]
    public partial class DesignScreen : UserControl
    {

        static DesignScreen inst_;

        public static DesignScreen inst() {
            if (inst_ == null)
                inst_ = new DesignScreen();
            return inst_;
        }

        IOCDependency<DocumentManager> documentManager = new IOCDependency<DocumentManager>();
        public DocumentManager DocumentManager { get { return documentManager.Object; } }

        public DesignScreen()
        {
            inst_ = this;
            Loaded += Window_Loaded;
            InitializeComponent();

            documentManager.Object.OnActiveDocumentChanged += DocumentChanged;

            ideGrid.ColumnDefinitions[0].Width = new GridLength(220, GridUnitType.Pixel);
            ideGrid.ColumnDefinitions[4].Width = new GridLength(220, GridUnitType.Pixel);
        }

        void DocumentChanged(Document newDoc, Document oldDoc)
        {
            mainContent.Children.Clear();
            if (newDoc != null)
            {
                docCommands.SetBinding(SprueKit.Controls.CommandInfoRepeater.ItemsSourceProperty, new Binding("DocumentCommands")
                {
                    Source = newDoc
                });
                leftFrame.Content = newDoc.Controls.LeftPanelControl;
                mainContent.Children.Add(newDoc.Controls.ContentControl);
                undoRedoHistory.DataContext = newDoc;
            }
            else
            {
                undoRedoHistory.DataContext = null;
                leftFrame.Content = null;
            }
            
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == splitRightVertical) // Info Tabs
            {
                if (ideGrid.ColumnDefinitions[4].Width.Value == 0)
                    ideGrid.ColumnDefinitions[4].Width = new GridLength(ActualWidth * 0.2f, GridUnitType.Pixel);
                else
                    ideGrid.ColumnDefinitions[4].Width = new GridLength(0, GridUnitType.Pixel);
            }
            else if (sender == splitLeftVertical) //Files
            {
                if (ideGrid.ColumnDefinitions[0].Width.Value == 0)
                    ideGrid.ColumnDefinitions[0].Width = new GridLength(ActualWidth * 0.2f, GridUnitType.Pixel);
                else
                    ideGrid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Pixel);
            }
            else if (sender == splitLog) //Log/Errors
            {
                if (ideGrid.RowDefinitions[2].Height.Value == 26)
                    ideGrid.RowDefinitions[2].Height = new GridLength(ActualHeight * 0.2f, GridUnitType.Pixel);
                else
                    ideGrid.RowDefinitions[2].Height = new GridLength(26, GridUnitType.Pixel);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DocumentChanged(documentManager.Object.ActiveDocument, null);
            propertyEditor.Object_OnActiveDocumentChanged(documentManager.Object.ActiveDocument, null);

            //winFormHost.Child = new System.Windows.Forms.MaskedTextBox("00/00/0000");
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            if (documentManager.Object.ActiveDocument != null && documentManager.Object.ActiveDocument.IsDirty)
                documentManager.Object.ActiveDocument.Save(true);
        }

        private void OnExport(object sender, RoutedEventArgs e)
        {
            if (documentManager.Object.ActiveDocument != null)
                documentManager.Object.ActiveDocument.Export();
        }

        private void OnExpandView(object sender, RoutedEventArgs e)
        {
            ideGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            ideGrid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Pixel);
            ideGrid.RowDefinitions[2].Height = new GridLength(26, GridUnitType.Pixel);
            ideGrid.ColumnDefinitions[4].Width = new GridLength(0, GridUnitType.Pixel);
        }

        private void OnExpandSideView(object sender, RoutedEventArgs e)
        {
            ideGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            ideGrid.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Pixel);
            ideGrid.RowDefinitions[2].Height = new GridLength(26, GridUnitType.Pixel);
            ideGrid.ColumnDefinitions[4].Width = new GridLength(0, GridUnitType.Pixel);
        }

        private void TimelinePlay(object sender, RoutedEventArgs e)
        {
            ((SprueKit.Graphics.Controls.Timeline)timelineScroller.ScrollableContent).IsPlaying = true;
        }

        private void TimelinePause(object sender, RoutedEventArgs e)
        {
            ((SprueKit.Graphics.Controls.Timeline)timelineScroller.ScrollableContent).IsPlaying = false;
        }
    }
}
