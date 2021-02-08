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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        private static Locale _locale = Locale.English;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
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
        }

        public static void SetLocale(Locale loc)
        {
            _locale = loc;
        }
    }
}
