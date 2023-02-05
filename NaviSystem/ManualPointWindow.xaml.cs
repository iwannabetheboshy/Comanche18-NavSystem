using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для ManualPointWindow.xaml
    /// </summary>
    public partial class ManualPointWindow : Window
    {
        MainWindow mailWindow;
        public ManualPointWindow(MainWindow wind)
        {
            mailWindow = wind;
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        bool isLatFirst = false;
        bool isLongFirst = false;
        bool isMesFirst = false;


        string keys = "D1D2D3D4D5D6D7D8D9D0";

        private void latTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!isLatFirst) latTextBox.Text = "";
            isLatFirst = true;
        }

        private void longTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!isLongFirst) longTextBox.Text = "";
            isLongFirst = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double Lat;
            double Long;
            try
            {
                Long = double.Parse(longTextBox.Text, System.Globalization.CultureInfo.InvariantCulture);
                Lat = double.Parse(latTextBox.Text, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка координат");
                return;
            }

            if (MesTextBox.Text == "Введите подпись") MesTextBox.Text = "";

            mailWindow.SetPointMarker(new Point(Long, Lat), MesTextBox.Text, true, false);
            this.Close();
        }

        private void longTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            string key = e.Key.ToString();
            key = (key == "D") ? " " : key;
            if (keys.Contains(key) || key == "Back" || key == "Delete" || key == "OemQuestion") { }
            else
                e.Handled = true;
        }


        private void MesTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!isMesFirst) MesTextBox.Text = "";
            isMesFirst = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void latTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string key = e.Key.ToString();
            key = (key == "D") ? " " : key;
            if (keys.Contains(key) || key == "Back" || key == "Delete" || key == "OemQuestion") { }
            else
                e.Handled = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "FileName"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "File type (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                StreamReader r_file = new StreamReader(dlg.FileName);
                string s;
                while ((s = r_file.ReadLine()) != null)
                {
                    var parts = s.Split(' ');


                    double Lat;
                    double Long;
                    try
                    {
                        parts[2] = parts[2].Replace(',', '.');
                        parts[3] = parts[3].Replace(',', '.');


                        Long = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                        Lat = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                        mailWindow.SetPointMarker(new Point(Long, Lat), "(Cтарый номер:"+parts[1]+")", true, false);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
            }

            this.Close();
        }
    }
}
