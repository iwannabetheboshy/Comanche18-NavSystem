using NaviSystem.Data;
using NaviSystem.DeviceControl;
using NaviSystem.GIS;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NaviSystem
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public IGIS gis = new GIS.GIS();
        public Dictionary<double, BeaconData> beacons = new Dictionary<double, BeaconData>();
        public DVLData dvl = new DVLData();
        public double rovHead = new double();
        public Tracks tracks;
        public DevicesSettings devicesSettings = new DevicesSettings();
        public DeviceUpdater deviceUpdater;
        IDeviceControl deviceControl;
        DevicesSettingsMenu devicesSettingsMenu;
        private string castNumber = " ";
        StatusList statusList;



        //T_manager tsk = new T_manager();
        //DVL dvl;
        //Set_Date set_date = new Set_Date();
        public Mesh mesh;

        //public DeviceUpdater device_updater = new DeviceUpdater();

        public MainWindow()
        {

            InitializeComponent();
            

            statusList = new StatusList(statusListBox, this);


            TrackLinkEvent += UpdateTrackLink;
            DVLEvent += UpdateDVL;
            SonarDyneEvent += UpdateSonarDyne;
            GPSEvent += UpdateGPS;
            //VesselDepthEvent += UpdateVesselDepth;
            WinchCabelEvent += UpdateWinchCabel;

            //<!-- Раскомментировать при подключении эхолота в качетве самостоятельного устройства --!>

            deviceControl = new DeviceControl.DeviceControl(TrackLinkEvent, DVLEvent, SonarDyneEvent, GPSEvent, /*VesselDepthEvent,*/ WinchCabelEvent);
            devicesSettingsMenu = new DevicesSettingsMenu(setListBox);
            mesh = new Mesh(this, canvas, ScrollViewer1, grid_h_lab, grid_v_lab, label1, label2);
            deviceUpdater = new DeviceUpdater(this);
            DataContext = deviceUpdater;
            tracks = new Tracks(mesh, canvas, slider);

            line.Stroke = Brushes.Red;
            canvas.Children.Add(line);

            grid_v_lab.Children.Add(can);

            WindowState = System.Windows.WindowState.Maximized;

            mesh.mesh();
        }



        //private void dispatcherTimer_Tick(object sender, EventArgs e)
        //{
        //    label.Content = count;
        //    count = 0;
        //}

        int count = 0;

        Polyline line = new Polyline();
        int x = 0;
        int y = 0;

        int ab = 0;





        private void button_Click(object sender, RoutedEventArgs e)
        {
            tracks.RemoveAllFiltred();
            tracks.RemoveAll();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            mesh.empty_map();
            mesh.ToVessel();
            zoom(1.1);
        }

        private void button3k_Click(object sender, RoutedEventArgs e)
        {

            //device_updater.stopall_devices();

        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Desired_map"; // Default file name
            dlg.DefaultExt = ".jpg"; // Default file extension
            dlg.Filter = "Map type (.jpg)|*.jpg"; // Filter files by extension

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                Map.Source = new BitmapImage(new Uri(dlg.FileName));
                Map.Tag = null;

                //mesh_grind(Map.Source.ToString());
                mesh.mesh();
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {



        }
        public List<FrameworkElement> icons_for_zoom = new List<FrameworkElement>();
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            zoom(1.1);

        }

        private void button4_Copy_Click(object sender, RoutedEventArgs e)
        {
            zoom(0.9);
            //mesh.zoom(0.9);
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            mesh.move_to_center();

        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            mesh.ToMapCenter();
            //mesh.update_labels();
        }
        int i = 1;
        Grid can = new Grid()
        {
            HorizontalAlignment = HorizontalAlignment.Right,
        };

        public bool isRecordingData = false;
        public string CastNumber
        {
            get => castNumber;
            set
            {
                castNumber = value;
                isRecordingData = true;
                dataSevingGrid.Background = Brushes.LawnGreen;

                if (value == "-1")
                {
                    isRecordingData = false;
                    dataSevingGrid.Background = new SolidColorBrush(Color.FromRgb(195, 195, 195));
                }
            }
        }

        private void ScrollViewer1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //zoom((ScrollViewer1.ActualHeight/canvas.ActualHeight));
            grid_v_lab.Width = ScrollViewer1.ActualHeight;
            mesh.mesh();
            zoom(0.9); 
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (button8.IsChecked == true) mesh.mes_state = true;
            mesh.add_mes_lines(e.GetPosition(canvas));
        }

        private void canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (button8.IsChecked == true)
            {
                if (mesh.mes_lines.Count > 0)
                {
                    if (!mesh.mes_state)
                    {
                        foreach (Line item in mesh.mes_lines)
                        {
                            canvas.Children.Remove(item);
                        }
                        foreach (Ellipse item in mesh.mes_elipse)
                        {
                            canvas.Children.Remove(item);
                        }
                        mesh.mes_lines.Clear();
                        mesh.mes_elipse.Clear();
                        if (canvas.Children.Contains(mesh.mes_lab)) canvas.Children.Remove(mesh.mes_lab);

                        mesh.distance = 0;
                    }
                    else
                    {
                        mesh.add_mes_lines(new Point(mesh.mes_lines.Last().X1, mesh.mes_lines.Last().Y1));
                        mesh.show_m_track(new Point(mesh.mes_lines.Last().X1, mesh.mes_lines.Last().Y1));
                        mesh.mes_state = false;
                    }


                }


            }
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            var list = devicesSettings.GetCOMList();
            deviceControl.StartDevices(list);
            var temp = MessageBox.Show("Начать запись данных?", "", MessageBoxButton.OKCancel);
            if(temp == MessageBoxResult.OK)
            {
                StartDataRecording(null, null);
            }
            // device_updater.start_all_devices();


        }


        private void button8_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void button8_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var devices = deviceControl.GetActiveList();
            deviceControl.CancelDevices(devices);

        }

        private void button7_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //device_updater.stopall_devices();
        }
        public class AAA
        {
            public string name = "AAAA";
            public override string ToString()
            {
                return "AAAA";
            }
        }
        private void button9_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button16_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void updateButton(object sender, RoutedEventArgs e)
        {
            devicesSettingsMenu.UpdateList();
        }

        private void saveButton(object sender, RoutedEventArgs e)
        {
            devicesSettingsMenu.Save();
        }

        private void deleteButton(object sender, RoutedEventArgs e)
        {
            devicesSettingsMenu.Delete();
        }

        private void addButton(object sender, RoutedEventArgs e)
        {
            devicesSettingsMenu.Add();
        }

        private void StopButton(object sender, RoutedEventArgs e)
        {
            deviceControl.CancelDevices(devicesSettingsMenu.GetDevicesList());
        }

        private void StopAllButton(object sender, RoutedEventArgs e)
        {
            var devices = deviceControl.GetActiveList();
            deviceControl.CancelDevices(devices);
            //treeView.Items.Clear();
        }

        private void MoveToVessel(object sender, RoutedEventArgs e)
        {
            mesh.ToVessel();
            zoom(1.1);
        }


        private void StartDataRecording(object sender, RoutedEventArgs e)
        {
            RecordingWindow recWind = new RecordingWindow(this);
            recWind.Show();
        }

        private void Map_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((bool)mousePosVis.IsChecked)
            {
                var pointWindow = new PointMessageWindow(this, e.GetPosition(canvas), false, false);
                pointWindow.Show();
            }
        }

        private void ScrollViewer1_MouseMove(object sender, MouseEventArgs e)
        {
            mouse_scroll(e);
            mesh.mouse_move(e);
            mesh.show_m_track(e.GetPosition(canvas));
            mesh.ShowMousePos(e);
        }

        private void mousePosVis_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)mousePosVis.IsChecked) mousePos.Visibility = Visibility.Visible;
        }

        private void mousePosVis_Unchecked(object sender, RoutedEventArgs e)
        {
            mousePos.Visibility = Visibility.Hidden;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var listToDel = new List<MyPointsListBoxGrind>();
            foreach (MyPointsListBoxGrind item in pointsListBox.Items)
            {
                if ((bool)item.checkBox.IsChecked) listToDel.Add(item);
            }
            foreach (var item in listToDel)
            {
                item.Remove();
            }
        }

        public void SetPointMarker(Point pixPoint, string message, bool isGEO, bool isROV, bool isVessel = false)
        {
            Point geoPoint;
            if (isGEO)
            {
                geoPoint = pixPoint;
                pixPoint = mesh.GeoToPix(geoPoint.X, geoPoint.Y);
            }
            else geoPoint = mesh.PixToGeo(pixPoint.X, pixPoint.Y);

            int number = ++MyPointsListBoxGrind.lastNumber;
            PointData point = new PointData()
            {
                Latitude = geoPoint.X,
                Longitude = geoPoint.Y,
                Message = message,
                Number = number
            };
            if (isROV) point.Depth = deviceUpdater.RovDepth;

            if (isVessel) point.Depth = deviceUpdater.VesselDepth;

            DeviceUpdater.dataSaver.SaveDataToCast(point, this.CastNumber);
            var temp = new MyPointsListBoxGrind(this, pixPoint, String.Format("{0} {1}", Math.Round(geoPoint.X, 6), Math.Round(geoPoint.Y, 6)), number, message);
            pointsListBox.Items.Add(temp);
        }        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ManualPointWindow mpWindow = new ManualPointWindow(this);
            mpWindow.Show();
        }



        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var listToDel = new List<MyPointsListBoxGrind>();
            foreach (MyPointsListBoxGrind item in pointsListBox.Items)
            {
                listToDel.Add(item);
            }
            foreach (var item in listToDel)
            {
                item.Remove();
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (deviceUpdater.RovFPos.X == 0 && deviceUpdater.RovFPos.Y == 0) return;
            var pointWindow = new PointMessageWindow(this, deviceUpdater.RovFPos, true, true);
            pointWindow.Show();
        }

        private void button11_Click(object sender, RoutedEventArgs e)
        {
            this.CastNumber = "-1";
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            statusListBox.Items.Clear();
        }

        bool warTextBoxWasFocus = false;
        private void WarTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!warTextBoxWasFocus) WarTextBox.Text = "";
            warTextBoxWasFocus = true;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            WarList.Items.Add(DateTime.Now.ToString()+" " + WarTextBox.Text);
            var beaconDataList = new List<BeaconData>();
            foreach (var item in deviceUpdater.beacons)
            {
                beaconDataList.Add(item.Value.my_beacon);
            }
            DeviceUpdater.dataSaver.SaveDataToCast(new ResultData(WarTextBox.Text.Replace(' ','_'), beaconDataList), this.CastNumber);
            WarTextBox.Text = "";
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (BeaconData.GPSData.Longitude == 0 && BeaconData.GPSData.Latitude == 0) return;
            var pointWindow = new PointMessageWindow(this, new Point(BeaconData.GPSData.Longitude, BeaconData.GPSData.Latitude), true, true, "Vessel");
            pointWindow.Show();
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Desired_map"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Map type (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                //try
                //{
                    string fName = dlg.FileName;
                    using (StreamWriter sw = new StreamWriter(fName.Remove(fName.IndexOf(".txt"))+"F.txt", true, System.Text.Encoding.Default))
                    {
                        StreamWriter sw1 = new StreamWriter(fName.Remove(fName.IndexOf(".txt")) + "FSurf.txt", true, System.Text.Encoding.Default);
                        
                        using (StreamReader r_file = new StreamReader(fName))
                        {
                            string sLine;
                            var points = new List<Point>();
                            var fPoints = new List<Point>();
                            while ((sLine = r_file.ReadLine()) != null)
                            {
                                var data = sLine.Split(' ');

                                points.Add(new Point(Double.Parse(data[1].Replace('.',',')), Double.Parse(data[2].Replace('.', ','))));

                                string sOutData = "";

                                if (fPoints.Count > 0)
                                {
                                    Point pixPoint = mesh.GeoToPix(points.Last().X, points.Last().Y);
                                    Point p = mesh.GeoToPix(fPoints.Last().X, fPoints.Last().Y);

                                    double X = slider.Value * pixPoint.X + (1 - slider.Value) * p.X;
                                    double Y = slider.Value * pixPoint.Y + (1 - slider.Value) * p.Y;
                                    var resPoint = mesh.PixToGeo(X, Y);
                                    fPoints.Add(resPoint);

                                    

                                    foreach (var item in data)
                                    {
                                        sOutData += item + " ";
                                    }
                                    sOutData += (resPoint.X.ToString() + " " + resPoint.Y.ToString()).Replace(',', '.');

                                    sw.WriteLine(sOutData);
                                sw1.WriteLine((resPoint.X.ToString() + " " + resPoint.Y.ToString()).Replace(',', '.') + " " + data[3] + " " + data[4] + " " + data[5]);
                            }
                                else
                                {
                                    fPoints.Add(new Point(points.Last().X, points.Last().Y));

                                    foreach (var item in data)
                                    {
                                        sOutData += item + " ";
                                    }
                                    sOutData += data[1] + " " + data[2];

                                    sw.WriteLine(sOutData);
                                sw1.WriteLine(data[1] + " " + data[2] + " " + data[3] + " " + data[4] + " " + data[5]);
                            }
                            };


                        }

                    }

                //}
               // catch (Exception)
                //{

                 //   throw;
                //}
            }
        }

        int trackscount = 0;
        private void InsertPointButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Desired_track"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Map type (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                //try
                //{
                string fName = dlg.FileName;

                using (StreamReader r_file = new StreamReader(fName))
                {
                    string sLine;
                    bool isFirst = true;

                    while ((sLine = r_file.ReadLine()) != null)
                    {
                        var data = sLine.Split(' ');

                        if (data.Count() == 6)
                        {
                            var point = new Point(Double.Parse(data[1].Replace('.', ',')), Double.Parse(data[2].Replace('.', ',')));
                            this.tracks.AddGeoPoint((trackscount).ToString(), point.X, point.Y);

                        }

                        if (data.Count() == 5)
                        {
                            var point = new Point(Double.Parse(data[0].Replace('.', ',')), Double.Parse(data[1].Replace('.', ',')));
                            this.tracks.AddGeoPoint((trackscount).ToString(), point.X, point.Y);
                        }

                        if (data.Count() == 8)
                        {
                            var point1 = new Point(Double.Parse(data[6].Replace('.', ',')), Double.Parse(data[7].Replace('.', ',')));
                            this.tracks.AddGeoPoint((trackscount).ToString(), point1.X, point1.Y);
                        }

                        if (isFirst)
                        {
                            this.tracks.change_stroke((trackscount).ToString(), Brushes.Black);
                            isFirst = false;
                        }
                    }
                };
                trackscount++;

            }


        }

        private void TransformFromMinutesToWhole(object sender, RoutedEventArgs e)
        {

        }
        //private void Button_Click_2(object sender, RoutedEventArgs e)
        //{
        //    ManualPointWindow mpWindow = new ManualPointWindow(this);
        //    mpWindow.Show();
        //}

    }
}
