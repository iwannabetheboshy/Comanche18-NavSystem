using NaviSystem.Date;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NaviSystem.DeviceCommunication
{
    public class Factory
    {
        /// <summary>
        /// Existing divices list
        /// </summary>
        public static string[] device_types = { "DVL", "Test", "SonarDyne", "TrackLink" };

        static List<ISerialPortCom> list = new List<ISerialPortCom>();
        /// <summary>
        /// Getting of ISerialPortCom list for DeviceSerialDate list
        /// </summary>
        /// <param name="com_list">List of main SerialPort parameters for Devices</param>
        /// <returns></returns>
        public static List<ISerialPortCom> GetISerialPortComList(List<DeviceSerialDate> com_list)
        {
            list.Clear();
            if (com_list == null) return null;
            foreach (DeviceSerialDate item in com_list)
            {
                switch (item.DeviceName)
                {
                    //case "DVL":
                    //    list.Add(new DVL(item));
                    //    break;
                    //case "Test":
                    //    list.Add(new Test(item));
                    //    break;
                    //case "SonarDyne":
                    //    list.Add(new SonarDyne(item));
                    //    break;
                    //case "TrackLink":
                    //    var temp = new TrackLink(item);
                    //    list.Add(temp);
                    //    EventTest.TL = temp;
                    //    break;
                    default:
                        break;
                }
            }
            if (list.Count == 0) return null;
            return list;
        }
    }
}
