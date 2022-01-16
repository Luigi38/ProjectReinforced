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

namespace ProjectReinforced
{
    /// <summary>
    /// GetStarted.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GetStarted : Window
    {
        public GetStarted()
        {
            InitializeComponent();

            this.Loaded += GetStarted_Loaded;
        }

        private void GetStarted_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < tabControl.Items.Count; i++)
            {
                var tab = tabControl.Items[i] as TabItem;
                tab.Visibility = Visibility.Hidden;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow.SetLocale(Locale.Korean);
        }

        private void Main_StartButton_Click(object sender, RoutedEventArgs e)
        {
            (tabControl.Items[1] as TabItem).IsSelected = true;
        }

        private void Games_NextButton_Click(object sender, RoutedEventArgs e)
        {
            (tabControl.Items[2] as TabItem).IsSelected = true;
        }

        private void Theme_NextButton_Click(object sender, RoutedEventArgs e)
        {
            (tabControl.Items[3] as TabItem).IsSelected = true;
        }

        private void Video_OKButton_Click(object sender, RoutedEventArgs e)
        {
            (tabControl.Items[4] as TabItem).IsSelected = true;
        }

        private void Video_PrevButton_Copy_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
