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

namespace NaviSystem
{
    /// <summary>
    /// Логика взаимодействия для RecordingWindow.xaml
    /// </summary>
    public partial class RecordingWindow : Window
    {
        MainWindow wind;
        public RecordingWindow(MainWindow window)
        {
            InitializeComponent();
            wind = window;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text != "")
            {
                wind.CastNumber = textBox.Text;
            }
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
