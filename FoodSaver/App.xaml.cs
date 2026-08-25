using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FoodSaver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public MainViewModel SharedMainViewModel { get; } = new MainViewModel();

        private Window window;
        public void ShowWindow(Window newWindow)
        {
            window?.Hide();
            window = newWindow;
            newWindow.Show();
        }
    }
}
