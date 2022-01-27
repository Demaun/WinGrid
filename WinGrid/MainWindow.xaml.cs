using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace WinGridApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static WinGrid WinGrid;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinGrid");
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            Environment.CurrentDirectory = dir;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            WinGrid = new WinGrid();
            Close();
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            App.Log($"Exception: {e.Exception}");
            Environment.Exit(1);
        }
    }
}
