using System;
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
using System.Windows.Shapes;

namespace WinGridApp
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        private WinGrid _winGrid;
        private ConfigurationManager _config;
        public ConfigurationWindow(WinGrid winGrid, ConfigurationManager config)
        {
            _winGrid = winGrid;
            _config = config;
            InitializeComponent();
            DataContext = config;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            _winGrid.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            _config.Serialize();
            Close();
        }
    }
}
