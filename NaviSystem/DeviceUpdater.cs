using NaviSystem.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NaviSystem
{
    public partial class MainWindow
    {
        public event EventHandler<object> TrackLinkEvent;
        public event EventHandler<object> DVLEvent;
        public event EventHandler<object> SonarDyneEvent;
        public event EventHandler<object> GPSEvent;

        //<!-- Раскомментировать при подключении эхолота в качетве самостоятельного устройства --!>
        //public event EventHandler<object> VesselDepthEvent;

        public event EventHandler<object> WinchCabelEvent;



        private void UpdateTrackLink(object sender, object e)
        {
            deviceUpdater.UpdateTrackLink(e);
        }
        private void UpdateDVL(object sender, object e)
        {
            throw new NotImplementedException();
        }
        private void UpdateSonarDyne(object sender, object e)
        {
            deviceUpdater.UpdateSonarDyne(e);
        }
        private void UpdateGPS(object sender, object e)
        {
            deviceUpdater.UpdateGPS(e);
        }

        //<!-- Раскомментировать при подключении эхолота в качетве самостоятельного устройства --!>
        /*private void UpdateVesselDepth(object sender, object e)
        {
            deviceUpdater.UpdateVesselDepth(e);
        }*/

        private void UpdateWinchCabel(object sender, object e)
        {
            deviceUpdater.UpdateWinchCabel(e);
        }
    }


    public struct M_date
    {
        public double vert_distance;
        public double vertROVandDPdis;
        public double hor_distance;
        public double cable_lenght;
        public double rovLong;
        public double rovLat;
        public double rovDepth;
        public double dpDepth;
        public double pix_rad;
        public Point fRovPos;
        public bool isRov;
    }

    public class DeviceUpdater : INotifyPropertyChanged
    {
        MainWindow mainWindow;
        public static DataSaver dataSaver;
        private Point staticOffsets = new Point(0, -40);
        DispatcherTimer dispatcherTimer;


        public DeviceUpdater(MainWindow mW)
        {
            mainWindow = mW;
            dataSaver = new DataSaver(mW);

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(TimerCallback);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
            dispatcherTimer.Start();
        }

        private void TimerCallback(object state, System.EventArgs e)
        {
           MyDateTime = String.Format("{0}:{1}:{2}", DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, DateTime.Now.TimeOfDay.Seconds);
        }

        public Dictionary<string, Beacon> beacons = new Dictionary<string, Beacon>();

        public void UpdateDVL(object obj)
        {
            throw new NotImplementedException();
        }



        public async void UpdateGPS(object obj)
        {
            await mainWindow?.Dispatcher.InvokeAsync(() =>
            {
                if (obj is GPSData gps)
                {
                    BeaconData.GPSData.E = gps.E;
                    BeaconData.GPSData.Heading = gps.Heading;
                    BeaconData.GPSData.Latitude = gps.Latitude;
                    BeaconData.GPSData.Longitude = gps.Longitude;
                    BeaconData.GPSData.N = gps.N;
                    BeaconData.GPSData.Velocity = gps.Velocity;
                    BeaconData.GPSData.VelocityDirection = gps.VelocityDirection;

                    BeaconData.GPSData.Depth = gps.Depth;


                    if (vesselTree == null)
                    {
                        vesselTree = new MyTreeViewItem(BeaconData.GPSData, mainWindow);
                        mainWindow.treeView.Items.Add(vesselTree);
                        mainWindow.SVessel.Visibility = Visibility.Visible;
                    }
                    VesselHeading = Math.Round(BeaconData.GPSData.Heading, 1);
                    VesselDepth = Math.Round(BeaconData.GPSData.Depth, 1);
                    VesselVel = BeaconData.GPSData.Velocity;
                    VesselVelDir = BeaconData.GPSData.VelocityDirection;
                    vesselTree.Update();
                    dataSaver.SaveDataToCast(BeaconData.GPSData, mainWindow.CastNumber);
                }

            });
        }

        //<!-- Раскомментировать при подключении эхолота в качетве самостоятельного устройства --!>

        /*public async void UpdateVesselDepth(object obj)
        {
            await mainWindow?.Dispatcher.InvokeAsync(() =>
            {
                if (obj is double depth)
                {
                    VesselDepth = depth;
                    BeaconData.GPSData.Depth = depth;
                    dataSaver.SaveDataToCast(VesselDepth, mainWindow.CastNumber);
                }

            });
        }*/

        public void UpdateHeading(object obj)
        {
            throw new NotImplementedException();
        }

        int last_update = 0;
        public async void UpdateSonarDyne(object obj)
        {
            await mainWindow?.Dispatcher.InvokeAsync(() =>
            {
                if (obj is BeaconData beacon)
                {

                    var point = GetGeoFromOffsets(new Point(beacon.X_offset, beacon.Y_offset));

                    if (point.X != 0 & point.Y != 0)
                    {
                        if (!Beacons.ContainsKey(beacon.GetName()))
                        {
                            beacon.Latitude = point.Y;
                            beacon.Longitude = point.X;

                            var b = new Beacon(beacon, mainWindow);
                            b.newMesure += NewMesure;
                            if (vesselTree == null)
                            {
                                vesselTree = new MyTreeViewItem(BeaconData.GPSData, mainWindow);
                                mainWindow.treeView.Items.Add(vesselTree);
                                mainWindow.SVessel.Visibility = Visibility.Visible;
                            }

                            mainWindow.treeView.Items.Add(b.viewitem);
                            Beacons.Add(beacon.GetName(), b);
                            VesselHeading = Math.Round(BeaconData.GPSData.Heading, 1);
                            b.Update();
                            dataSaver.SaveDataToCast(beacon, mainWindow.CastNumber);
                        }
                        if (Beacons.ContainsKey(beacon.GetName()))
                        {
                            beacon.Latitude = point.Y;
                            beacon.Longitude = point.X;

                            if (vesselTree != null) vesselTree.Update();
                            var b = Beacons[beacon.GetName()];
                            b.my_beacon = beacon;
                            VesselHeading = Math.Round(BeaconData.GPSData.Heading, 1);
                            b.Update();
                            dataSaver.SaveDataToCast(beacon, mainWindow.CastNumber);

                            //var time = DateTime.Now.Minute * 60 + DateTime.Now.Second;
                            //if (Math.Abs(last_update - time) > 20)
                            //{
                            //    MessageBox.Show("SonarDyne не отвечает");
                            //}
                            //else last_update = time;

                        }
                    }
                }
                //if (obj is BeaconData beacon && !Beacons.ContainsKey(beacon.GetName()))
                //{
                    
                //    if (point.X != 0 & point.Y != 0)
                //    {

                //        beacon.Latitude = point.X;
                //        beacon.Longitude = point.Y;

                //        var b = new Beacon(beacon, mainWindow);
                //        b.newMesure += NewMesure;
                //        if (vesselTree == null)
                //        {
                //            vesselTree = new MyTreeViewItem(BeaconData.GPSData, mainWindow);
                //            mainWindow.treeView.Items.Add(vesselTree);
                //            mainWindow.SVessel.Visibility = Visibility.Visible;
                //        }

                //        mainWindow.treeView.Items.Add(b.viewitem);
                //        Beacons.Add(beacon.GetName(), b);
                //        VesselHeading = Math.Round(BeaconData.GPSData.Heading, 1);
                //        b.Update();
                //        dataSaver.SaveDataToCast(beacon, mainWindow.CastNumber);
                //    }
                //    else return;


                //}
                //else if (obj is BeaconData beacon1 && Beacons.ContainsKey(beacon1.GetName()))
                //{
                //    if (vesselTree != null) vesselTree.Update();
                //    var b = Beacons[beacon1.GetName()];
                //    b.my_beacon = beacon1;
                //    VesselHeading = Math.Round(BeaconData.GPSData.Heading, 1);
                //    b.Update();
                //    dataSaver.SaveDataToCast(beacon1, mainWindow.CastNumber);
                //}
            });
        }
        MyTreeViewItem vesselTree;



        public Dictionary<string, Beacon> Beacons {get => beacons; set => beacons = value; }

        public async void UpdateTrackLink(object obj)
        {
                 await mainWindow?.Dispatcher.InvokeAsync(() =>
                {
                    if (obj is BeaconData beacon && !Beacons.ContainsKey(beacon.GetName()))
                    {
                        var b = new Beacon(beacon, mainWindow);
                        b.newMesure += NewMesure;
                        if (vesselTree == null)
                        {
                            vesselTree = new MyTreeViewItem(BeaconData.GPSData, mainWindow);
                            mainWindow.treeView.Items.Add(vesselTree);
                            mainWindow.SVessel.Visibility = Visibility.Visible;
                        }

                        mainWindow.treeView.Items.Add(b.viewitem);
                        Beacons.Add(beacon.GetName(), b);
                        VesselHeading = Math.Round(BeaconData.GPSData.Heading, 1);
                        b.Update();
                        dataSaver.SaveDataToCast(beacon, mainWindow.CastNumber);

                    }
                    else if (obj is BeaconData beacon1 && Beacons.ContainsKey(beacon1.GetName()))
                    {
                        if (vesselTree != null)
                        {
                            vesselTree.Update();
                            dataSaver.SaveDataToCast(BeaconData.GPSData, mainWindow.CastNumber);
                        }
                        var b = Beacons[beacon1.GetName()];
                        b.my_beacon = beacon1;
                        VesselHeading = Math.Round(BeaconData.GPSData.Heading, 1);
                        b.Update();
                        dataSaver.SaveDataToCast(beacon1, mainWindow.CastNumber);
                    }
                });
        }

        internal async void UpdateWinchCabel(object obj)
        {
            await mainWindow?.Dispatcher.InvokeAsync(() =>
            {
                if (obj is double lenght)
                {
                    WinchCabelLenght = lenght;
                }

            });
             
        }


        private void NewMesure(object sender, M_date e)
        {
            RovLong = e.rovLong;
            RovLat = e.rovLat;
            RovDepth = e.rovDepth;
            DPDepth = e.dpDepth;
            RovFPos = e.fRovPos;

            if (!e.isRov)
            {
                CableLenght = e.cable_lenght;
                //DepthDifference = ((int)e.vert_distance).ToString();
                DepthDifference = ((int)e.vertROVandDPdis).ToString();

                if (Beacon.MaxCableLenght - 5 < e.cable_lenght)
                {
                    mainWindow.cableLenght.Background = Brushes.OrangeRed;
                    SetResultData("Taut Cable", VesselDepth.ToString(), WinchCabelLenght.ToString(), CableLenght.ToString(), DepthDifference.ToString());
                }
                else mainWindow.cableLenght.Background = new SolidColorBrush(Color.FromRgb(195, 195, 195));

                if (e.cable_lenght < 5)
                {
                    mainWindow.depthDifference.Background = Brushes.OrangeRed;

                    SetResultData("Between DP and ROV < 5m", VesselDepth.ToString(), WinchCabelLenght.ToString(), CableLenght.ToString(), DepthDifference.ToString()); 
                } 
                else mainWindow.depthDifference.Background = new SolidColorBrush(Color.FromRgb(195, 195, 195));

                if (e.vertROVandDPdis < 0)
                {
                    mainWindow.depthDifference.Background = Brushes.OrangeRed;
                    SetResultData("DP is under ROV", VesselDepth.ToString(), WinchCabelLenght.ToString(), CableLenght.ToString(), DepthDifference.ToString());
                }
                else mainWindow.depthDifference.Background = new SolidColorBrush(Color.FromRgb(195, 195, 195));
            }
        }


        

        public Point GetGeoFromOffsets(Point off) // пересчет смешения маяков относительно судна в географические координаты
        {
            if (BeaconData.GPSData.Latitude == 0 & BeaconData.GPSData.Longitude == 0 & BeaconData.GPSData.Heading == 0) return new Point(0, 0);

            Point offset = new Point(off.X + staticOffsets.X, off.Y + staticOffsets.Y);

            Point Ship_E_N = mainWindow.gis.GeoToUTM(new Point(BeaconData.GPSData.Latitude, BeaconData.GPSData.Longitude));
            double[] Beacon_E_N = new double[2];

            Beacon_E_N[1] = Ship_E_N.Y + (-offset.X * Math.Sin(BeaconData.GPSData.Heading * 3.142 / 180) +
                        offset.Y * Math.Cos(BeaconData.GPSData.Heading * 3.142 / 180));
            Beacon_E_N[0] = Ship_E_N.X + (offset.Y * Math.Sin(BeaconData.GPSData.Heading * 3.142 / 180) +
                offset.X * Math.Cos(BeaconData.GPSData.Heading * 3.142 / 180));

            Point long_lat = mainWindow.gis.UTMToGeo(new Point(Beacon_E_N[0], Beacon_E_N[1]), BeaconData.GPSData.Latitude > 0);
            return long_lat;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        void SetResultData(string message, string vesselDepth, string winchCabelLenght, string dableLenghtSonarDyne, string depthDifferenceSonarDyne)//List<string> param)
        {

            message = message.Replace(' ', '_');

            //Data Format:  "message vesselDepth winchCabelLenght dableLenghtSonarDyne depthDifferenceSonarDyne"

            message +=" "+ vesselDepth+ " " + winchCabelLenght + " " + dableLenghtSonarDyne + " " + depthDifferenceSonarDyne;
            var beaconDataList = new List<BeaconData>();
            foreach (var item in this.beacons)
            {
                beaconDataList.Add(item.Value.my_beacon);
            }


            DeviceUpdater.dataSaver.SaveDataToCast(new ResultData(message, beaconDataList, true), mainWindow.CastNumber);
        }

        public Point RovFPos = new Point();
        public Point VesselPos = new Point();

        double cableLenght = 0;
        public double CableLenght
        {
            get { return cableLenght; }
            set
            {
                cableLenght = Math.Round(value, 2);
                OnPropertyChanged("CableLenght");
            }
        }

        string depthDifference = "0";
        public string DepthDifference
        {
            get { return depthDifference; }
            set
            {
                depthDifference = value;
                OnPropertyChanged("DepthDifference");
            }
        }

        double vesselHeading = 0;
        public double VesselHeading
        {
            get { return vesselHeading; }
            set
            {
                if(value >= 360) vesselHeading = Math.Round(value - 360,0);
                else if(value <0 ) vesselHeading = Math.Round(value + 360,0);
                else vesselHeading = Math.Round(value,0);
                OnPropertyChanged("VesselHeading");
            }
        }

        double vesselVel = 0;
        public double VesselVel
        {
            get { return vesselVel; }
            set
            {
                vesselVel = Math.Round(value, 2);
                OnPropertyChanged("VesselVel");
            }
        }

        double vesselVelDir = 0;
        public double VesselVelDir
        {
            get { return vesselVelDir; }
            set
            {
                vesselVelDir = Math.Round(value, 2);
                VesselVelDir1 = vesselVelDir-90;
                OnPropertyChanged("VesselVelDir");
            }
        }
        double vesselVelDir1 = 0;
        public double VesselVelDir1
        {
            get { return vesselVelDir1; }
            set
            {
                vesselVelDir1 = value;
                OnPropertyChanged("VesselVelDir1");
            }
        }

        double vesselDepth = 0;
        public double VesselDepth
        {
            get { return vesselDepth; }
            set
            {
                vesselDepth = Math.Round(value, 2);
                if (Math.Abs(VesselDepth) < Math.Abs(winchCabelLenght) + 10)
                {
                    mainWindow.WinchCable.Background = Brushes.OrangeRed;
                    SetResultData("VesselDepth-WinchCabelLenght<10m", VesselDepth.ToString(), WinchCabelLenght.ToString(), CableLenght.ToString(), DepthDifference.ToString());
                }
                else mainWindow.WinchCable.Background = new SolidColorBrush(Color.FromRgb(195, 195, 195));
                OnPropertyChanged("VesselDepth");
            }
        }

        double winchCabelLenght = 0;
        public double WinchCabelLenght
        {
            get { return winchCabelLenght; }
            set
            {
                winchCabelLenght = Math.Round(value, 2);
                if (Math.Abs(VesselDepth) < Math.Abs(winchCabelLenght) + 10) mainWindow.WinchCable.Background = Brushes.OrangeRed;
                else mainWindow.WinchCable.Background = new SolidColorBrush(Color.FromRgb(195, 195, 195));
                OnPropertyChanged("WinchCabelLenght");
            }
        }

        double rovLong = 0;
        public double RovLong
        {
            get { return rovLong; }
            set
            {
                rovLong = Math.Round(value, 5);
                OnPropertyChanged("RovLong");
            }
        }
        double rovLat = 0;
        public double RovLat
        {
            get { return rovLat; }
            set
            {
                rovLat = Math.Round(value, 5);
                OnPropertyChanged("RovLat");
            }
        }

        double rovDepth = 0;
        public double RovDepth
        {
            get { return rovDepth; }
            set
            {
                rovDepth = Math.Round(value, 1);
                OnPropertyChanged("RovDepth");
            }
        }

        double dpDepth = 0;
        public double DPDepth
        {
            get { return dpDepth; }
            set
            {
                dpDepth = Math.Round(value, 1);
                OnPropertyChanged("DPDepth");
            }
        }

        string dateTime;

        public string MyDateTime
        {
            get { return dateTime; }
            set
            {
                dateTime = value;
                OnPropertyChanged("MyDateTime");
            }
        }
    }
        public class Beacon
            {
                public event EventHandler<M_date> newMesure;
                void OnNewNesure(M_date e)
                {
                    Volatile.Read(ref newMesure)?.Invoke(this, e);
                }

                MainWindow mainWindow;
                public BeaconData my_beacon;
                double number;

                static Dictionary<string, Label> Nolist = new Dictionary<string, Label>();
                static bool has_ROV = false;
                static bool has_DP = false;

                static double maxCableLenght = 100;
                public static double MaxCableLenght { set { if (value > 0) maxCableLenght = value; } get { return maxCableLenght; } }

                static Point ROV_p;
                static Point DP_p;
                static double ROV_d;
                static double DP_d;



                static M_date mesures;

                bool filtred = true;
                MyTreeViewItem hans;
                Brush colour;
                static Ellipse el = new Ellipse() { Height = 20, Width = 20, Stroke = Brushes.Black };
                string track_name;
                Visibility vis = Visibility.Visible;
                public MyTreeViewItem viewitem;
                Point label_pos = new Point();
                FrameworkElement label;
                NaviSystem.Data.Type T = NaviSystem.Data.Type.No;
                Data.Type set_type(Data.Type value)
                {
                    switch (value)
                    {
                        case Data.Type.ROV:
                            {
                                switch (T)
                                {
                                    case Data.Type.DP:
                                        {
                                            if (has_ROV) break;
                                            else
                                            {
                                                T = value;
                                                label.Visibility = Visibility.Hidden;
                                                label = mainWindow.ROV;
                                                label.Visibility = Visibility.Visible;
                                                has_ROV = true;

                                                has_DP = false;
                                            }
                                        }
                                        break;
                                    case Data.Type.No:
                                        {
                                            if (has_ROV) break;
                                            else
                                            {
                                                if (Nolist.ContainsKey(track_name))
                                                {
                                                    mainWindow.canvas.Children.Remove(Nolist[track_name]);
                                                    mainWindow.icons_for_zoom.Remove(Nolist[track_name]);
                                                    Nolist.Remove(track_name);
                                                }
                                                label = mainWindow.ROV;
                                                label.Visibility = Visibility.Visible;
                                                has_ROV = true;

                                                T = value;
                                            }
                                        }
                                        break;
                                }

                            }
                            break;
                        case Data.Type.DP:
                            switch (T)
                            {
                                case Data.Type.ROV:
                                    {
                                        if (has_DP) break;
                                        else
                                        {
                                            T = value;
                                            label.Visibility = Visibility.Hidden;
                                            label = mainWindow.DP;
                                            label.Visibility = Visibility.Visible;
                                            has_DP = true;

                                            has_ROV = false;
                                        }
                                    }
                                    break;
                                case Data.Type.No:
                                    {
                                        if (has_DP) break;
                                        else
                                        {
                                            if (Nolist.ContainsKey(track_name))
                                            {
                                                mainWindow.canvas.Children.Remove(Nolist[track_name]);
                                                mainWindow.icons_for_zoom.Remove(Nolist[track_name]);
                                                Nolist.Remove(track_name);
                                            }
                                            label = mainWindow.DP;
                                            label.Visibility = Visibility.Visible;
                                            has_DP = true;

                                            T = value;
                                        }
                                    }
                                    break;
                            }
                            break;
                        case Data.Type.No:
                            {
                                switch (T)
                                {
                                    case NaviSystem.Data.Type.ROV:
                                        {
                                            has_ROV = false;
                                            label.Visibility = Visibility.Hidden;

                                        }
                                        break;
                                    case NaviSystem.Data.Type.DP:
                                        {
                                            has_DP = false;

                                            label.Visibility = Visibility.Hidden;
                                        }
                                        break;
                                }
                                if (!Nolist.ContainsKey(track_name))
                                {
                                    Label No = new Label() { Content = number.ToString(), FontSize = 16, Foreground = mainWindow.tracks.get_stroke(track_name) };
                                    Nolist.Add(track_name, No);
                                    mainWindow.canvas.Children.Add(No);
                                    mainWindow.icons_for_zoom.Add(No);
                                    label = No;
                                }

                                T = value;
                            }
                            break;
                    }

                    return T;
                }
                void set_lab_margin(FrameworkElement lab, Point pos)
                {
                    try
                    {
                        if (lab != null)
                        {
                            if (T == Data.Type.No) lab.Margin = new Thickness(pos.X, pos.Y, 0, 0);
                            else lab.Margin = new Thickness(pos.X - (lab.Width / 2), pos.Y - (lab.Height / 2), 0, 0);
                            mainWindow.canvas.Children.Remove(lab);
                            mainWindow.canvas.Children.Add(lab);
                }
                    }
                    catch (Exception e)
                    {
                        Status_List.push("set_lab_margin (type: " + lab.GetType() + " ): " + e.ToString());
                    }

                }
                void set_vis(Visibility vis)
                {
                    if (T == Data.Type.DP)
                    {
                        label.Visibility = vis;
                        mainWindow.tracks.set_visibility(track_name, vis);
                        el.Visibility = vis;
                    }
                    else
                    {
                        label.Visibility = vis;
                        mainWindow.tracks.set_visibility(track_name, vis);
                    }
                }
                void mesure_update()
                {

                    if (has_ROV & has_DP)
                    {
                        mesures.isRov = false;
                        mesures.vert_distance = Math.Round(Math.Abs(ROV_d - DP_d), 1);
                        mesures.vertROVandDPdis = Math.Round((ROV_d - DP_d), 1);
                        mesures.hor_distance = mainWindow.mesh.l_mesure(ROV_p, DP_p);
                        double temp = Math.Round(Math.Sqrt(mesures.vert_distance * mesures.vert_distance + mesures.hor_distance * mesures.hor_distance), 2);

                        mesures.cable_lenght = temp > (maxCableLenght *2) ? mesures.cable_lenght : temp;

                        double work_area_rad = Math.Round(Math.Sqrt((-mesures.vert_distance * mesures.vert_distance + maxCableLenght * maxCableLenght) - 15), 2);
                        mesures.pix_rad = Math.Round(work_area_rad * mainWindow.mesh.get_m_to_pix_coef(), 0);

                        if (mesures.pix_rad > 20)
                        {
                            el.Width = mesures.pix_rad * 2;
                            el.Height = mesures.pix_rad * 2;
                        }
                        else mesures.pix_rad = 20;
                el.Visibility = Visibility.Visible;

                      

                        if(T == Data.Type.DP) set_lab_margin(el, label_pos);

                        OnNewNesure(mesures);
                    }
                    else if (has_ROV)
                    {
                        el.Visibility = Visibility.Hidden;
                        mesures.isRov = true;
                        OnNewNesure(mesures);
                    }
                    else el.Visibility = Visibility.Hidden;
        }
                public void Update()
                {
                    if (filtred) label_pos = mainWindow.tracks.AddFiltredGeoPoint(track_name, my_beacon.Longitude, my_beacon.Latitude);
                    else label_pos = mainWindow.tracks.AddGeoPoint(track_name, my_beacon.Longitude, my_beacon.Latitude);
            var GeoPoint = mainWindow.mesh.PixToGeo(label_pos.X, label_pos.Y);
            my_beacon.FLongitude = GeoPoint.X;
            my_beacon.FLatitude = GeoPoint.Y;

            switch (T)
                    {

                        case Data.Type.ROV:
                            {
                                ROV_p = label_pos;
                                ROV_d = my_beacon.Depth;
                                mesures.fRovPos = mainWindow.mesh.PixToGeo(label_pos.X, label_pos.Y);
                                mesures.rovLong = my_beacon.Longitude;
                                mesures.rovLat = my_beacon.Latitude;
                                mesures.rovDepth = my_beacon.Depth;
                                mainWindow.deviceUpdater.RovLong = my_beacon.Longitude;
                                mainWindow.deviceUpdater.RovLat = my_beacon.Latitude;
                                mainWindow.deviceUpdater.RovDepth = my_beacon.Depth;

                        //mainWindow.ROV.RenderTransform = new RotateTransform(my_beacon.Heading);
                    }
                            break;
                        case Data.Type.DP:
                            {
                                if (!mainWindow.icons_for_zoom.Contains(el))
                                {
                                    mainWindow.icons_for_zoom.Add(el);
                                    mainWindow.canvas.Children.Add(el);
                                }
                                DP_p = label_pos;
                                DP_d = my_beacon.Depth;
                                mesures.dpDepth = my_beacon.Depth;
                                mainWindow.deviceUpdater.DPDepth = my_beacon.Depth;

                        //if (has_ROV & has_DP) set_lab_margin(el, label_pos);
                    }
                            break;
                    }

                    mesure_update();

                    set_lab_margin(label, label_pos);
                    viewitem.target = my_beacon;
                    viewitem.Update();
                }
                public void delete()
                {
                    hans.Items.Remove(viewitem);
                    set_type(Data.Type.No);
                    if (Nolist.ContainsKey(track_name))
                    {
                        mainWindow.canvas.Children.Remove(Nolist[track_name]);
                        mainWindow.icons_for_zoom.Remove(Nolist[track_name]);
                        Nolist.Remove(track_name);
                    }

                }
                public Beacon(BeaconData beacon, MainWindow mWindow)
                {
                    mainWindow = mWindow;
                    my_beacon = beacon;
                    track_name = beacon.GetName(); number = beacon.Number; T = beacon.type;
                    if (!mainWindow.icons_for_zoom.Contains(mainWindow.DP)) mainWindow.icons_for_zoom.Add(mainWindow.DP);
                    if (!mainWindow.icons_for_zoom.Contains(mainWindow.ROV)) mainWindow.icons_for_zoom.Add(mainWindow.ROV);
                    set_type(T);
                    viewitem = new MyTreeViewItem(beacon, mWindow);
                    viewitem.typebox.SelectionChanged += Typebox_SelectionChanged;
                    viewitem.checkvis.Click += Checkvis_Click;
                }

                private void Checkvis_Click(object sender, RoutedEventArgs e)
                {
                    if ((bool)viewitem.checkvis.IsChecked)
                    {
                        set_vis(Visibility.Hidden);
                    }
                    else
                    {
                        set_vis(Visibility.Visible);
                    }
                }

                private void Typebox_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    if (((Label)viewitem.typebox.SelectedItem).Content.ToString() == "ROV")
                        switch (set_type(Data.Type.ROV))
                        {
                            case Data.Type.DP:
                                viewitem.typebox.SelectedIndex = 1;
                                break;
                            case Data.Type.No:
                                viewitem.typebox.SelectedIndex = 2;
                                break;
                        }

                    if (((Label)viewitem.typebox.SelectedItem).Content.ToString() == "No")
                        switch (set_type(Data.Type.No))
                        {
                            case Data.Type.ROV:
                                viewitem.typebox.SelectedIndex = 0;
                                break;
                            case Data.Type.DP:
                                viewitem.typebox.SelectedIndex = 1;
                                break;
                        }
                    if (((Label)viewitem.typebox.SelectedItem).Content.ToString() == "DP")
                        switch (set_type(Data.Type.DP))
                        {
                            case NaviSystem.Data.Type.ROV:
                                viewitem.typebox.SelectedIndex = 0;
                                break;
                            case NaviSystem.Data.Type.No:
                                viewitem.typebox.SelectedIndex = 2;
                                break;
                        }
                }

    }


        

        //HANSUpdater hans_up = new HANSUpdater();
        //class HANSUpdater
        //{
        //    public struct M_date
        //    {
        //        public double vert_distance;
        //        public double hor_distance;
        //        public double cable_lenght;
        //        public double pix_rad;
        //    }
        //    Dictionary<string, Beacon> beacons = new Dictionary<string, Beacon>();
        //    MyTreeViewItem hans_viewitem;

        //    class Beacon
        //    {
        //        public event EventHandler<M_date> newMesure;
        //        void OnNewNesure(M_date e)
        //        {
        //            Volatile.Read(ref newMesure)?.Invoke(this, e);
        //        }

        //        BeaconData my_beacon;
        //        double number;

        //        static Dictionary<string, Label> Nolist = new Dictionary<string, Label>();
        //        static bool has_ROV = false;
        //        static bool has_DP = false;

        //        static double maxCableLenght = 90;
        //        public static double MaxCableLenght { set { if (value > 0) maxCableLenght = value; } get { return maxCableLenght; } }

        //        static Point ROV_p;
        //        static Point DP_p;
        //        static double ROV_d;
        //        static double DP_d;


        //        static M_date mesures;

        //        bool filtred = true;
        //        MyTreeViewItem hans;
        //        Brush colour;
        //        Ellipse el = new Ellipse() { Height = 20, Width = 20, Stroke = Brushes.Black };
        //        string track_name;
        //        Visibility vis = Visibility.Visible;
        //        MyTreeViewItem viewitem;
        //        Point label_pos = new Point();
        //        FrameworkElement label;
        //        NaviSystem.Data.Type T = NaviSystem.Data.Type.No;
        //        Data.Type set_type(Data.Type value)
        //        {
        //            switch (value)
        //            {
        //                case Data.Type.ROV:
        //                    {
        //                        switch (T)
        //                        {
        //                            case Data.Type.DP:
        //                                {
        //                                    if (has_ROV) break;
        //                                    else
        //                                    {
        //                                        T = value;
        //                                        label.Visibility = Visibility.Hidden;
        //                                        label = Objects_Links.M_Window.ROV;
        //                                        label.Visibility = Visibility.Visible;
        //                                        has_ROV = true;

        //                                        has_DP = false;
        //                                    }
        //                                }
        //                                break;
        //                            case Data.Type.No:
        //                                {
        //                                    if (has_ROV) break;
        //                                    else
        //                                    {
        //                                        if (Nolist.ContainsKey(track_name))
        //                                        {
        //                                            Objects_Links.M_Window.canvas.Children.Remove(Nolist[track_name]);
        //                                            Objects_Links.M_Window.icons_for_zoom.Remove(Nolist[track_name]);
        //                                            Nolist.Remove(track_name);
        //                                        }
        //                                        label = Objects_Links.M_Window.ROV;
        //                                        label.Visibility = Visibility.Visible;
        //                                        has_ROV = true;

        //                                        T = value;
        //                                    }
        //                                }
        //                                break;
        //                        }

        //                    }
        //                    break;
        //                case Data.Type.DP:
        //                    switch (T)
        //                    {
        //                        case Data.Type.ROV:
        //                            {
        //                                if (has_DP) break;
        //                                else
        //                                {
        //                                    T = value;
        //                                    label.Visibility = Visibility.Hidden;
        //                                    label = Objects_Links.M_Window.DP;
        //                                    label.Visibility = Visibility.Visible;
        //                                    has_DP = true;

        //                                    has_ROV = false;
        //                                }
        //                            }
        //                            break;
        //                        case Data.Type.No:
        //                            {
        //                                if (has_DP) break;
        //                                else
        //                                {
        //                                    if (Nolist.ContainsKey(track_name))
        //                                    {
        //                                        Objects_Links.M_Window.canvas.Children.Remove(Nolist[track_name]);
        //                                        Objects_Links.M_Window.icons_for_zoom.Remove(Nolist[track_name]);
        //                                        Nolist.Remove(track_name);
        //                                    }
        //                                    label = Objects_Links.M_Window.DP;
        //                                    label.Visibility = Visibility.Visible;
        //                                    has_DP = true;

        //                                    T = value;
        //                                }
        //                            }
        //                            break;
        //                    }
        //                    break;
        //                case Data.Type.No:
        //                    {
        //                        switch (T)
        //                        {
        //                            case Device_Date.HANS.Type.ROV:
        //                                {
        //                                    has_ROV = false;
        //                                    label.Visibility = Visibility.Hidden;

        //                                }
        //                                break;
        //                            case Device_Date.HANS.Type.DP:
        //                                {
        //                                    has_DP = false;

        //                                    label.Visibility = Visibility.Hidden;
        //                                }
        //                                break;
        //                        }
        //                        if (!Nolist.ContainsKey(track_name))
        //                        {
        //                            Label No = new Label() { Content = number.ToString(), FontSize = 16, Foreground = Objects_Links.Tracks.get_stroke(track_name) };
        //                            Nolist.Add(track_name, No);
        //                            Objects_Links.M_Window.canvas.Children.Add(No);
        //                            Objects_Links.M_Window.icons_for_zoom.Add(No);
        //                            label = No;
        //                        }

        //                        T = value;
        //                    }
        //                    break;
        //            }

        //            return T;
        //        }
        //        void set_lab_margin(FrameworkElement lab, Point pos)
        //        {
        //            try
        //            {
        //                if (lab != null)
        //                {
        //                    if (T == Data.Type.No) lab.Margin = new Thickness(pos.X, pos.Y, 0, 0);
        //                    else lab.Margin = new Thickness(pos.X - (lab.Width / 2), pos.Y - (lab.Height / 2), 0, 0);
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Status_List.push("set_lab_margin (type: " + lab.GetType() + " ): " + e.ToString());
        //            }

        //        }
        //        void set_vis(Visibility vis)
        //        {
        //            if (T == Data.Type.DP)
        //            {
        //                label.Visibility = vis;
        //                Objects_Links.Tracks.set_visibility(track_name, vis);
        //                el.Visibility = vis;
        //            }
        //            else
        //            {
        //                label.Visibility = vis;
        //                Objects_Links.Tracks.set_visibility(track_name, vis);
        //            }
        //        }
        //        void mesure_update()
        //        {

        //            if (has_ROV & has_DP)
        //            {
        //                mesures.vert_distance = Math.Round(Math.Abs(ROV_d - DP_d), 1);
        //                mesures.hor_distance = Objects_Links.M_Window.mesh.l_mesure(ROV_p, DP_p);
        //                double temp = Math.Round(Math.Sqrt(mesures.vert_distance * mesures.vert_distance + mesures.hor_distance * mesures.hor_distance), 2);

        //                mesures.cable_lenght = temp > (maxCableLenght + 50) ? mesures.cable_lenght : temp;

        //                double work_area_rad = Math.Round(Math.Sqrt(-mesures.vert_distance * mesures.vert_distance + maxCableLenght * maxCableLenght), 2);
        //                mesures.pix_rad = Math.Round(work_area_rad * Objects_Links.M_Window.mesh.get_m_to_pix_coef(), 0);

        //                if (mesures.pix_rad < 20) mesures.pix_rad = 20;

        //                el.Width = mesures.pix_rad * 2;
        //                el.Height = mesures.pix_rad * 2;

        //                OnNewNesure(mesures);
        //            }


        //        }
        //        public void Update()
        //        {
        //            if (filtred) label_pos = Objects_Links.Tracks.AddFiltredGeoPoint(track_name, my_beacon.Position.Longitude, my_beacon.Position.Latitude);
        //            else label_pos = Objects_Links.Tracks.AddGeoPoint(track_name, my_beacon.Position.Longitude, my_beacon.Position.Latitude);
        //            switch (T)
        //            {
        //                case Data.Type.ROV:
        //                    {
        //                        ROV_p = label_pos;
        //                        ROV_d = my_beacon.Depth;
        //                        Objects_Links.M_Window.ROV.RenderTransform = new RotateTransform(my_beacon.Heading);
        //                    }
        //                    break;
        //                case Data.Type.DP:
        //                    {
        //                        if (!Objects_Links.M_Window.icons_for_zoom.Contains(el))
        //                        {
        //                            Objects_Links.M_Window.icons_for_zoom.Add(el);
        //                            Objects_Links.M_Window.canvas.Children.Add(el);
        //                        }
        //                        DP_p = label_pos;
        //                        DP_d = my_beacon.Depth;

        //                        if (has_ROV & has_DP) set_lab_margin(el, label_pos);
        //                    }
        //                    break;
        //            }

        //            mesure_update();

        //            set_lab_margin(label, label_pos);
        //            viewitem.update();
        //        }
        //        public void delete()
        //        {
        //            hans.Items.Remove(viewitem);
        //            set_type(Data.Type.No);
        //            if (Nolist.ContainsKey(track_name))
        //            {
        //                Objects_Links.M_Window.canvas.Children.Remove(Nolist[track_name]);
        //                Objects_Links.M_Window.icons_for_zoom.Remove(Nolist[track_name]);
        //                Nolist.Remove(track_name);
        //            }

        //        }
        //        public Beacon(Beacon beacon, MyTreeViewItem h)
        //        {
        //            my_beacon = beacon;
        //            viewitem = new MyTreeViewItem(beacon);
        //            track_name = beacon.get_name(); number = beacon.Number; T = beacon.type;
        //            if (!Objects_Links.M_Window.icons_for_zoom.Contains(Objects_Links.M_Window.DP)) Objects_Links.M_Window.icons_for_zoom.Add(Objects_Links.M_Window.DP);
        //            if (!Objects_Links.M_Window.icons_for_zoom.Contains(Objects_Links.M_Window.ROV)) Objects_Links.M_Window.icons_for_zoom.Add(Objects_Links.M_Window.ROV);
        //            (hans = h).Items.Add(viewitem);
        //            set_type(T);
        //            viewitem.typebox.SelectionChanged += Typebox_SelectionChanged;
        //            viewitem.checkvis.Click += Checkvis_Click;
        //        }

        //        private void Checkvis_Click(object sender, RoutedEventArgs e)
        //        {
        //            if ((bool)viewitem.checkvis.IsChecked)
        //            {
        //                set_vis(Visibility.Hidden);
        //            }
        //            else
        //            {
        //                set_vis(Visibility.Visible);
        //            }
        //        }

        //        private void Typebox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //        {
        //            if (((Label)viewitem.typebox.SelectedItem).Content.ToString() == "ROV")
        //                switch (set_type(Data.Type.ROV))
        //                {
        //                    case Data.Type.DP:
        //                        viewitem.typebox.SelectedIndex = 1;
        //                        break;
        //                    case Data.Type.No:
        //                        viewitem.typebox.SelectedIndex = 2;
        //                        break;
        //                }

        //            if (((Label)viewitem.typebox.SelectedItem).Content.ToString() == "No")
        //                switch (set_type(Data.Type.No))
        //                {
        //                    case Data.Type.ROV:
        //                        viewitem.typebox.SelectedIndex = 0;
        //                        break;
        //                    case Data.Type.DP:
        //                        viewitem.typebox.SelectedIndex = 1;
        //                        break;
        //                }
        //            if (((Label)viewitem.typebox.SelectedItem).Content.ToString() == "DP")
        //                switch (set_type(Data.Type.DP))
        //                {
        //                    case Device_Date.HANS.Type.ROV:
        //                        viewitem.typebox.SelectedIndex = 0;
        //                        break;
        //                    case Device_Date.HANS.Type.No:
        //                        viewitem.typebox.SelectedIndex = 2;
        //                        break;
        //                }
        //        }
        //    }

        //    public void Update(BeaconData beacon)
        //    {
        //        if (hans_viewitem == null)
        //        {
        //            hans_viewitem = new MyTreeViewItem(Navigation_Date.HANS);
        //            Objects_Links.M_Window.treeView.Items.Add(hans_viewitem);
        //        }
        //        string b_name = beacon.get_name();
        //        if (beacons.ContainsKey(b_name)) beacons[b_name].Update();
        //        else
        //        {

        //            var b = new Beacon(beacon, hans_viewitem);
        //            b.newMesure += B_newMesure;
        //            beacons.Add(b_name, b);
        //        }
        //    }

        //    private void B_newMesure(object sender, M_date e)
        //    {
        //        Objects_Links.M_Window.CabletextBlock.Text = "Натяжение кабеля " +
        //                        Environment.NewLine + e.cable_lenght.ToString(); /* + Environment.NewLine +
        //                            "Горизонтальнo: " + e.hor_distance.ToString()+ Environment.NewLine +
        //                            "Радиус: " + e.pix_rad.ToString();*/

        //        Objects_Links.M_Window.DepthtextBlock.Text = "Разница глубин " +
        //            Environment.NewLine + ((int)e.vert_distance).ToString();
        //        if (Beacon.MaxCableLenght - 5 < e.cable_lenght) Objects_Links.M_Window.CabletextBlock.Background = Brushes.OrangeRed;
        //        else Objects_Links.M_Window.CabletextBlock.Background = new SolidColorBrush(Color.FromRgb(241, 241, 241));
        //    }
        //}



        //double maxCableLenght = 80;
        //int count = 0;
        //Ellipse DPel = new Ellipse() { Width = 20, Height = 20, Stroke = Brushes.Black };
        //List<AC_COM> list = new List<AC_COM>();
        //public bool set_correctly { private set; get; }
        //Dictionary<string, Label> Nolist = new Dictionary<string, Label>();

        //void treeview_update(object target, string name)
        //{
        //    int state = 0;
        //    foreach (var item in Objects_Links.M_Window.treeView.Items)
        //    {
        //        if (item is MyTreeViewItem && ((MyTreeViewItem)item).myname == name)
        //        {
        //            ((MyTreeViewItem)item).update();
        //            state = 1;
        //        }
        //    }
        //    if (state == 0)
        //    {
        //        if (target is Device_Date.Ship) Objects_Links.M_Window.treeView.Items.Add(new MyTreeViewItem(Navigation_Date.Ship));
        //        if (target is Device_Date.DVL) Objects_Links.M_Window.treeView.Items.Add(new MyTreeViewItem(Navigation_Date.DVL));
        //        if (target is Device_Date.HANS) Objects_Links.M_Window.treeView.Items.Add(new MyTreeViewItem(Navigation_Date.HANS));

        //    }
        //}
        //public void Ship_update(bool filtred = true)
        //{

        //    if (Navigation_Date.Ship != null)
        //    {
        //        if (Objects_Links.Tracks != null)
        //        {
        //            Point p = new Point();
        //            if (filtred) p = Objects_Links.Tracks.AddFiltredGeoPoint(Navigation_Date.Ship.get_name(), Navigation_Date.Ship.Position.Longitude, Navigation_Date.Ship.Position.Latitude);
        //            else p = Objects_Links.Tracks.AddGeoPoint(Navigation_Date.Ship.get_name(), Navigation_Date.Ship.Position.Longitude, Navigation_Date.Ship.Position.Latitude);
        //            if (!Objects_Links.M_Window.icons_for_zoom.Contains(Objects_Links.M_Window.SVessel)) Objects_Links.M_Window.icons_for_zoom.Add(Objects_Links.M_Window.SVessel);
        //            Objects_Links.M_Window.SVessel.Margin = new Thickness(p.X - (Objects_Links.M_Window.SVessel.Width / 2), p.Y - (Objects_Links.M_Window.SVessel.Height / 2), 0, 0);
        //            Objects_Links.M_Window.SVessel.RenderTransform = new RotateTransform(Navigation_Date.Ship.Heading - 90);
        //            Status_List.push("Ship_update" + Navigation_Date.Ship.Position.Longitude.ToString() + "  " + Navigation_Date.Ship.Position.Latitude.ToString());
        //            treeview_update(Navigation_Date.Ship, Navigation_Date.Ship.get_name());
        //            if (Objects_Links.M_Window.SVessel.Tag == null)
        //            {
        //                Objects_Links.M_Window.SVessel.Visibility = Visibility.Visible;
        //                Objects_Links.M_Window.SVessel.Tag = 1;
        //            }
        //        }
        //    }
        //}

        //public void set_COM_list()
        //{
        //    if (Objects_Links.S_Date.set_correctly)
        //    {
        //        list = COM_Factory.factory(Objects_Links.S_Date.get_COM_list());
        //        set_correctly = true;
        //        foreach (var item in list)
        //        {
        //            if (item is TrackLink) item.NewDate += TrackLinkMsg;
        //        }
        //    }
        //}

        //private async void TrackLinkMsg(object sender, object e)
        //{
        //    try
        //    {
        //        await Objects_Links.M_Window?.Dispatcher.InvokeAsync(() =>
        //        {
        //            //HANS_update(true);
        //            hans_up.Update((Device_Date.HANS.Beacon)e);
        //            Objects_Links.M_Window.update_HANS();
        //            Ship_update(true);
        //        });
        //    }
        //    catch (Exception e1) { Status_List.push("DeviceUpdater.TrackLinkMsg " + e1.Message + " " + DateTimeOffset.Now.ToString()); }

        //}

        //public void start_all_devices()
        //{
        //    if (set_correctly)
        //    {
        //        foreach (var item in list)
        //        {
        //            Objects_Links.T_Manager.start(item);
        //        }
        //    }
        //}
        //public void start_device_by_name(string name)
        //{
        //    if (set_correctly)
        //    {
        //        foreach (var item in list)
        //        {
        //            if (item.devicename == name) Objects_Links.T_Manager.start(item);
        //        }
        //    }
        //}
        //public void stop_device_by_name(string name)
        //{
        //    if (set_correctly)
        //    {
        //        foreach (var item in list)
        //        {
        //            if (item.devicename == name) Objects_Links.T_Manager.cansel(item);
        //        }
        //    }
        //}
        //public void stopall_devices()
        //{
        //    if (set_correctly)
        //    {
        //        foreach (var item in list)
        //        {
        //            Objects_Links.T_Manager.cansel(item);
        //        }
        //    }
        //}



    
}
    

