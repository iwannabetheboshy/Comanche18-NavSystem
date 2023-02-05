using NaviSystem.Data;
using System;
using System.Collections.Generic;
//using ViewModel;

namespace NaviSystem.DeviceControl
{
    public interface IDeviceControl
    {
        List<string> GetSerialPorts();
        List<string> StartDevices(List<MySerialData> s_list);
        List<string> CancelDevices(List<MySerialData> s_list);
        List<MySerialData> GetActiveList();
        bool IsActive(MySerialData s_date);
    }

    public class DeviceControl : IDeviceControl
    {
        string[] device_types = DevicesTypes.devicesTypes;
        List<MySerialData> activeDevicesList = new List<MySerialData>();
        //IViewModel viewModel;
        TaskManager taskManager = new TaskManager();

        public event EventHandler<object> TrackLinkEvent;
        public event EventHandler<object> DVLEvent;
        public event EventHandler<object> SonarDyneEvent;
        public event EventHandler<object> GPSEvent;
       // public event EventHandler<object> VesselDepthEvent;
        public event EventHandler<object> WinchCabelEvent;

        



        public DeviceControl(EventHandler<object> TrackLinkE, EventHandler<object> DVLE, EventHandler<object> SonarDyneE,
            EventHandler<object> GPSE,/* EventHandler<object> VesselDepthE, */ EventHandler<object> WinchCabelE)
        {
            TrackLinkEvent += TrackLinkE;
            DVLEvent += DVLE;
            SonarDyneEvent += SonarDyneE;
            GPSEvent += GPSE;
           // VesselDepthEvent += VesselDepthE;
            WinchCabelEvent += WinchCabelE;
        }


        static List<ACDevice> devicesList = new List<ACDevice>();
        /// <summary>
        /// Getting of IDevice list for MySerialData list
        /// </summary>
        /// <param name="com_list">List of main SerialPort parameters for Devices</param>
        /// <returns></returns>
        List<ACDevice> GetISerialPortComList(List<MySerialData> com_list)
        {
            devicesList.Clear();
            if (com_list == null) return null;
            foreach (MySerialData item in com_list)
            {
                switch (item.DeviceName)
                {
                    case "DVL":
                        {
                            if (!item.IsCorrect())
                            {
                                Status_List.push("MySerialData is not correct: " + item.DeviceName);
                                break;
                            }

                            DVL dvl = new DVL(item);
                            dvl.NewDate += DVLEvent;
                            devicesList.Add(dvl);
                        }
                        break;
                    case "SonarDyne":
                        {
                            if (!item.IsCorrect())
                            {
                                Status_List.push("MySerialData is not correct: " + item.DeviceName);
                                break;
                            }
                            SonarDyne sd = new SonarDyne(item);
                            sd.NewDate += SonarDyneEvent;
                            devicesList.Add(sd);
                        }
                        break;
                    case "TrackLink":
                        {
                            if (!item.IsCorrect())
                            {
                                Status_List.push("MySerialData is not correct: " + item.DeviceName);
                                break;
                            }
                            TrackLink tl = new TrackLink(item);
                            tl.NewDate += TrackLinkEvent;
                            devicesList.Add(tl);
                        }
                        break;
                    case "GPS":
                        {
                            if (!item.IsCorrect())
                            {
                                Status_List.push("MySerialData is not correct: " + item.DeviceName);
                                break;
                            }
                            GPS gps = new GPS(item);
                            gps.NewDate += GPSEvent;
                            devicesList.Add(gps);
                        }
                        break;

                    //<!-- Раскомментировать при подключении эхолота в качетве самостоятельного устройства --!>

                    /*case "VesselDepth":
                        {
                            if (!item.IsCorrect())
                            {
                                Status_List.push("MySerialData is not correct: " + item.DeviceName);
                                break;
                            }
                            VesselDepth rh = new VesselDepth(item);
                            rh.NewDate += VesselDepthEvent;
                            devicesList.Add(rh);
                        }
                        break;*/

                    case "WinchCabel":
                        {
                            if (!item.IsCorrect())
                            {
                                Status_List.push("MySerialData is not correct: " + item.DeviceName);
                                break;
                            }
                            WinchCabel rh = new WinchCabel(item);
                            rh.NewDate += WinchCabelEvent;
                            devicesList.Add(rh);
                        }
                        break;
                    default:
                        break;
                }
            }
            if (devicesList.Count == 0) return null;
            return devicesList;
        }

        public List<string> CancelDevices(List<MySerialData> s_list)
        {
            List<string> result = new List<string>();
            List<ACDevice> temp = new List<ACDevice>();
            foreach (ACDevice currentDevice in devicesList)
            {
                foreach (MySerialData forCancel in s_list)
                {
                    if(forCancel.DeviceName == currentDevice.GetName())
                    {
                        taskManager.Cansel(currentDevice);
                        temp.Add(currentDevice);
                        result.Add(currentDevice.GetName());
                    }
                }
            }
            foreach (var item in temp)
            {
                devicesList.Remove(item);
            }
            return result;
        }

        public List<string> GetSerialPorts()
        {
            return new List<string>(System.IO.Ports.SerialPort.GetPortNames());
        }

        public bool IsActive(MySerialData s_date)
        {
            foreach (var item in devicesList)
            {
                if(item.GetName()== s_date.DeviceName)
                {
                    var actionLIst = item.GetActions();
                    if(actionLIst!=null) return taskManager.IsActive(actionLIst[0].name); 
                }
            }
            return false;
        }

        public List<string> StartDevices(List<MySerialData> s_list)
        {
            activeDevicesList = s_list;
            List<string> result = new List<string>();
            var list = GetISerialPortComList(s_list);
            if (list!=null) foreach (var item in list)
            {
                if (taskManager.Start(item)) result.Add(item.GetName());
            }
            return result;
        }

        public List<MySerialData> GetActiveList()
        {
            return activeDevicesList;
        }
    }

    public interface IDevice
    {
        List<ActionInfo> GetActions();
        string GetName();
    }

    /// <summary>
    /// Contains Device's single Action() information 
    /// </summary>
    public class ActionInfo
    {
        /// <summary>
        /// Constructing Action() info
        /// </summary>
        /// <param name="n_name">Action information name "read" or "date_processing"</param>
        /// <param name="n_function">Desired Action()</param>
        /// <param name="n_clean">Will run when n_function's Thread stops</param>
        /// <param name="n_sleep_time">Sleep time between invocations of n_function in loop)</param>
        public ActionInfo(string n_name, Action n_function, Action n_clean = null, int n_sleep_time = 30)
        {
            name = n_name;
            function = n_function;
            clean = n_clean;
            sleep_time = n_sleep_time;
        }
        public int sleep_time { get; set; }
        public string name { get; set; }
        public Action function { get; set; }
        public Action clean { get; set; } // действие перед остановкой задачи
        public override string ToString()
        {
            return String.Format("Name:{0}", name);
        }
    }
}
