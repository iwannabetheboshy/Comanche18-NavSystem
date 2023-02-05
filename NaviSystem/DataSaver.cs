using NaviSystem.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NaviSystem
{

    public class DataSaver
    {
        static readonly string path = @"C:\NavigationSystem";//C:\Users\konop\Documents\NavigationSystem"; 
        string subpath = @"Cast_";
        string currentCastDirectory;
        MainWindow mainWindow;
        public DataSaver(MainWindow window)
        {
            mainWindow = window;
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
        }
        void CreateCastFolder(string cast)
        {
            currentCastDirectory = path + @"\" + subpath + cast +"_"+ DateTime.Now.Date.ToShortDateString();

            DirectoryInfo dirInfo = new DirectoryInfo(currentCastDirectory);

            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
        }

        public void SaveDataToCast(object obj, string cast)
        {


            if (obj is PointData point1)
            {
                CreateCastFolder(cast);
                string path = currentCastDirectory + "/" + point1.GetName() + ".txt";
                DataToFile(path, point1.GetDataString());

                path = currentCastDirectory + "/LOG" + DateTime.Now.Date.Day + DateTime.Now.Date.Month + DateTime.Now.Date.Year + ".txt";
                DataToFile(path, "$PointData " + point1.GetDataString());
            }

            if (obj is ResultData resData)
            {
                CreateCastFolder(cast);
                string path;
                if (!resData.isWarring)
                {
                    path = currentCastDirectory + "/results" + DateTime.Now.Date.Day + DateTime.Now.Date.Month + DateTime.Now.Date.Year + ".txt";
                    DataToFile(path, resData.ToString());
                }
                else
                {
                    path = currentCastDirectory + "/warnings" + DateTime.Now.Date.Day + DateTime.Now.Date.Month + DateTime.Now.Date.Year + ".txt";
                    DataToFile(path, resData.ToString());
                }


                CreateCastFolder(cast);
                path = currentCastDirectory + "/LOG" + DateTime.Now.Date.Day + DateTime.Now.Date.Month + DateTime.Now.Date.Year + ".txt";
                DataToFile(path, "$ResultData " + resData.ToString());
                //Data Format:  "message vesselDepth winchCabelLenght dableLenghtSonarDyne depthDifferenceSonarDyne "+BeaconData.GetDataString()+BeaconData.GPSData.GetDataString();
            }

            if (obj is double depth)
            {
                CreateCastFolder(cast);
                string path = currentCastDirectory + "/" + "promer" + ".txt";
                //146.681000 47.114900 -3350.60 00:29:13UTC 05-06-18UTC
                string hour = DateTime.Now.Hour>9 ? DateTime.Now.Hour.ToString(): "0" + DateTime.Now.Hour.ToString();
                string min = DateTime.Now.Minute > 9 ? DateTime.Now.Minute.ToString() : "0" + DateTime.Now.Minute.ToString();
                string sec = DateTime.Now.Second > 9 ? DateTime.Now.Second.ToString() : "0" + DateTime.Now.Second.ToString();
                string day = DateTime.Now.Day > 9 ? DateTime.Now.Day.ToString() : "0" + DateTime.Now.Day.ToString();
                string month = DateTime.Now.Month > 9 ? DateTime.Now.Month.ToString() : "0" + DateTime.Now.Month.ToString();
                string year = DateTime.Now.Year.ToString().Substring(DateTime.Now.Year.ToString().Length-2,2);
                string data = String.Format("{0} {1} -{2} {3}:{4}:{5}UTC {6}-{7}-{8}UTC",
                    BeaconData.GPSData.Longitude.ToString().Replace(',','.'), BeaconData.GPSData.Latitude.ToString().Replace(',', '.'),
                    depth.ToString().Replace(',', '.'), hour, min, sec, day, month, year);
                DataToFile(path, data);

                path = currentCastDirectory + "/LOG" + DateTime.Now.Date.Day + DateTime.Now.Date.Month + DateTime.Now.Date.Year + ".txt";
                DataToFile(path, "$VDEPTH " + data);
            }



            if (obj is BeaconData beacon)
            {
                CreateCastFolder(cast);

                string path = currentCastDirectory + "/LOG" + DateTime.Now.Date.Day + DateTime.Now.Date.Month + DateTime.Now.Date.Year + ".txt";
                DataToFile(path, "$BeaconData " + beacon.GetDataString());

                if (mainWindow.isRecordingData)
                {
                    path = currentCastDirectory + "/" + beacon.Number.ToString() + ".txt";
                    DataToFile(path, beacon.GetDataString());
                }
            }
            if (obj is GPSData gps)
            {
                CreateCastFolder(cast);

                string path = currentCastDirectory + "/LOG" + DateTime.Now.Date.Day + DateTime.Now.Date.Month + DateTime.Now.Date.Year + ".txt";
                DataToFile(path, "$GPSData " + gps.GetDataString());

                if (mainWindow.isRecordingData)
                {
                    path = currentCastDirectory + "/" + gps.GetName() + ".txt";
                    DataToFile(path, gps.GetDataString());
                }
            }
        }

        void DataToFile(string fileName, string data)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(data);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
        
    }

    public class StatusList
    {
        ListBox list;
        MainWindow mainWindow;
        public StatusList(ListBox newlist, MainWindow mWindow)
        {
            list = newlist;
            
            mainWindow = mWindow;
            Status_List.temp = this;
        }

        public override string ToString()
        {
            ToList();
            return base.ToString();
        }

        public async void ToList()
        {
            var mes = Status_List.lastMessage;
            await mainWindow?.Dispatcher.InvokeAsync(() => {
                if (list.Items.Count > 100) list.Items.Clear();
                list.Items.Add(mes); list.ScrollIntoView(mes);
            });
        }
    } 


}

