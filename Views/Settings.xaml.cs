﻿using System.Windows.Controls;
using Microsoft.Win32;

namespace LLC_MOD_Toolbox.Views
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _ = new OpenFileDialog();
        }
    }
}
