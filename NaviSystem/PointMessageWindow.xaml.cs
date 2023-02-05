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
    /// Логика взаимодействия для PointMessageWindow.xaml
    /// </summary>
    public partial class PointMessageWindow : Window
    {
        MainWindow window;
        Point pix;
        bool isGEO = false;
        bool isROV = false;
        string mess;
        public PointMessageWindow(MainWindow wind, Point pixPoint, bool isGeo, bool _isROV, string mes="")
        {
            window = wind;
            isGEO = isGeo;
            pix = pixPoint;
            isROV = _isROV;
            mess = mes;
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (mess == "")
            {
                window.SetPointMarker(pix, mess + " " + textBox.Text, isGEO, isROV);
            }
            else
            {
                window.SetPointMarker(pix, mess + " " + textBox.Text, isGEO, isROV, true);
            }
            
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
