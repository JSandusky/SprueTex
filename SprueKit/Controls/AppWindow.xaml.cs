﻿using System;
using System.Collections.Generic;
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
    /// Interaction logic for AppWindow.xaml
    /// </summary>
    public partial class AppWindow : UserControl
    {
        public AppWindow()
        {
            InitializeComponent();

            MinWidth = 240;

            tabs.FilterSource += Tabs_FilterSource;
        }

        private void Tabs_FilterSource(object sender, FirstFloor.ModernUI.Windows.Controls.SourceEventArgs e)
        {
            if (e.Source.OriginalString.Equals("cmd://exit"))
            {
                Application.Current.MainWindow.Close();
                e.Canceled = true;
            }
        }
    }
}
