﻿using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;

namespace LLC_MOD_Toolbox.Views
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : UserControl
    {
        [RelayCommand]
        public void Uninstall()
        {
            throw new System.NotImplementedException();
        }

        public Settings()
        {
            InitializeComponent();
        }
    }
}
