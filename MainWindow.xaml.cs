using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using ProjectReinforced.Clients;
using ProjectReinforced.Recording;
using ProjectReinforced.Types;

namespace ProjectReinforced
{
    public enum Locale
    {
        English,
        Korean
    }
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isBegin = true;

        public static Locale Locale { get; set; } = Locale.English;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isBegin) //GetStarted 창 표시
            {
                this.IsEnabled = false;

                var getStartedWindow = new GetStarted();
                getStartedWindow.Show();
                getStartedWindow.Closed += (ss, ee) =>
                {
                    this.IsEnabled = true;
                };
            }

            await ClientManager.Initialize();
        }

        public static void SetLocale(Locale loc)
        {
            Locale = loc;
        }
    }
}
