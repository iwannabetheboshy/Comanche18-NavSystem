using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NaviSystem.Data
{
    /// <summary>
    /// Contains main SerialPort parameters for Device
    /// </summary>
    public class MySerialData
    {
        public string DeviceName { get; set;}
        public string PortName { get; set; }
        public string PortNameOut { get; set; }
        public double BaudRate { get; set; }
        public double BaudRateOut { get; set; }
        public double DataBits { get; set; }
        public bool IsCorrect()
        {
            if(BaudRate>0 & PortName!="") return true;
            return false;
        }
    }

    public enum Type
    {
        ROV, DP, No
    }

    public static class DevicesTypes
    {
        public static string[] devicesTypes = { "DVL", "SonarDyne", "TrackLink", "GPS", "VesselDepth", "WinchCabel" };
    }

    public class BeaconData
    {
        public double X_offset { get; set; }
        public double Y_offset { get; set; }
        public double Depth { get; set; }
        public Type type = Type.No;
        public double Number { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double FLatitude { get; set; }
        public double FLongitude { get; set; }
        public static GPSData GPSData = new GPSData();
        public bool N { get; set; }
        public bool E { get; set; }

        public override string ToString()
        {
            return String.Format("#:{0}|D:{1}|Long:{3}|Lat:{2}", 
                Number, Math.Round(Depth, 1), Math.Round(Latitude,5), Math.Round(Longitude, 5));
        }
        public string GetName()
        {
            return Number.ToString();
        }
        public string GetDataString()
        {
            return String.Format("{0} {2} {1} {3} {4} {5} {6}", Math.Round(Number, 0), Latitude, Longitude, Math.Round(Depth, 1), DateTime.Now.ToString(), FLongitude, FLatitude).Replace(',','.');
        }
    }

    public class ResultData
    {
        string data = "";
        public bool isWarring = false;

        public ResultData(string mesage, List<BeaconData> beacons, bool isWar=false)
        {
            isWarring = isWar;
            data = mesage;

            foreach (var item in beacons)
            {
                data += " " + item.GetDataString();
            }
            data += " " + BeaconData.GPSData.GetDataString();
        }

        public override string ToString()
        {
            return data;
        }
    }



    public class GPSData
    {
        public double Latitude { get; set; }
        public double Heading { get; set; }
        public double Longitude { get; set; }
        public double Depth { get; set; }
        public bool? N { get; set; }
        public bool? E { get; set; }
        public double Velocity { get; set; }
        public double VelocityDirection { get; set; }

        public override string ToString()
        {
                return (String.Format("Vessel|Long:{0}|Lat:{1}|H:{2}",
                    Math.Round(Longitude, 5), Math.Round(Latitude, 5), Math.Round(Heading, 1))).ToString();
        }
        public string GetName()
        {
            return "Vessel";
        }
        public string GetDataString()
        {
            return (String.Format("Vessel {1} {0} {2} {3} {4} {5} {6}",
                   Latitude, Longitude,  Heading, (-Depth), VelocityDirection, Velocity, DateTime.Now.ToString())).Replace(',', '.');
        }
    }

    public class PointData
    {
        public int Number{ get; set; }
        public double Latitude { get; set; }
        public double Depth { get; set; }
        public double Longitude { get; set; }
        public string Message { get; set; }

        public string GetDataString()
        {
            return (String.Format("Point {0} {1} {2} {3} {4} {5}",
                  Number, Latitude, Longitude, Depth, Message.Replace(' ','_'), DateTime.Now.ToString())).Replace(',', '.');
        }

        public string GetName()
        {
            return "Points";
        }
    }

    public class DVLData
    {
        public double Heading { get; set; }
        public double X_Vel { get; set; }
        public double Y_Vel { get; set; }
        public double Z_Vel { get; set; }
        public double Pitch { get; set; }
        public double Roll { get; set; }
        public double X_Ref_Vel { get; set; }
        public double Y_Ref_Vel { get; set; }
        public double Z_Ref_Vel { get; set; }
        public double Depth { get; set; }
        public double Temperature { get; set; }
        public double[] MeasuredDistances { get; set; }
        public string GetDataString()
        {
            return (String.Format("DVL {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14}",
                   Heading, X_Vel, Y_Vel, Z_Vel, Pitch, Roll, X_Ref_Vel, Y_Ref_Vel, Z_Ref_Vel, Depth, Temperature, MeasuredDistances[0], MeasuredDistances[1], MeasuredDistances[2], MeasuredDistances[3])).Replace(',', '.');
        }
    }

    public static class Status_List // список событий системы
    {
        public static object temp;
        public static string lastMessage;
        public static List<string> list = new List<string>();
        public static void push(string status)
        {
            lastMessage = status;
            list.Add(status);
            temp.ToString();
            Console.WriteLine(status);

        }
    }
}
