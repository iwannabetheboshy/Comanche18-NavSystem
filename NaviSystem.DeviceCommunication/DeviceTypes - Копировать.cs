using NaviSystem.Date;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace NaviSystem.DeviceCommunication
{
    public abstract class BaseDevice
    {
        /// <summary>
        /// Определение члна события
        /// </summary>
        public event EventHandler<object> NewDate;
        /// <summary>
        /// Определение метода, ответственного за уведомление зарегестрированных объектов о событии
        /// </summary>
        /// <param name="e">Инфомация, передаваемая получателям события</param>
        protected virtual void OnNewDate(object e)
        {
            //сохранение ссылки на делегата во временой переменной (во избежание изменения делегата другим потоком)
            Volatile.Read(ref NewDate)?.Invoke(this, e);
        }
        protected List<DiviceActionInfo> actionInfoList  = new List<DiviceActionInfo>();
        protected SerialPort mySerialPort = new SerialPort();
        protected string devicename { get; set; }
        /// <summary>
        /// Queue of date from read() for date_process()
        /// </summary>
        protected Queue<object> queue = new Queue<object>();

        protected bool openPort()
        {
            if (mySerialPort.IsOpen)
            {
                Status_List.push("SerialPort " + mySerialPort.PortName + " IsOpen " + DateTimeOffset.Now.ToString());
                return true;
            }
            else
            {
                try
                {
                    mySerialPort.Open();
                    Status_List.push("SerialPort " + mySerialPort.PortName + " IsOpen " + DateTimeOffset.Now.ToString());
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool closePort()
        {

            if (mySerialPort.IsOpen)
            {
                mySerialPort.Close();
                Status_List.push("SerialPort " + mySerialPort.PortName + " IsClosed " + DateTimeOffset.Now.ToString());
                return true;
            }
            else
            {
                return false;
            }

        }
        public abstract void read();
        public abstract void dateProcess();
        public abstract void initActionInfoList();
    }

    //public class TrackLink : BaseDevice, ISerialPortCom
    //{
    //    public void clean()
    //    {
    //        closePort();
    //    }
    //    public TrackLink(DeviceSerialDate com)
    //    {
    //        devicename = com.DeviceName;
    //        //mySerialPort.PortName = com.PortName;
    //        //mySerialPort.BaudRate = (int)com.BaudRate;
    //        //mySerialPort.Parity = Parity.None;
    //        //mySerialPort.StopBits = StopBits.One;
    //        //mySerialPort.DataBits = (int)com.DataBits;
    //        //mySerialPort.Handshake = Handshake.None;
    //        //mySerialPort.RtsEnable = true;
    //        //mySerialPort.ReadTimeout = 300;
    //        list_f_info = new List<F_info>();
    //        list_f_info.Add(new F_info(devicename + " read", read, clean));
    //        list_f_info.Add(new F_info(devicename + " date_process", date_process));
    //    }
    //    public override void read()
    //    {
    //        string buffer = (string)IntegrationTest.TrackLinkTest.testread();
    //        //if (!open_port()) return;

    //        //string buffer;
    //        //for (;;)
    //        //{
    //        //    try
    //        //    {
    //        //        buffer = mySerialPort.ReadLine();
    //        //        if (buffer.Length > 5) break;
    //        //    }
    //        //    catch (Exception)
    //        //    {
    //        //        return;
    //        //    }
    //        //}
    //        queue.Enqueue(buffer);
    //    }

    //    delegate void For_Date_Proc(Device_Date.HANS.Beacon beacon);
    //    public override void date_process()
    //    {
    //        //LQF "9,05/04/17,14:54:31,  4250.5598,-14718.5044,  4250.5538,-14718.5110,153.7, 198.4\r"
    //        string date = "";
    //        //string[] b = new string[5];
    //        lock (queue)
    //        {
    //            if (queue.Count > 0)
    //            {
    //                date = (string)queue.Dequeue();
    //            }
    //            else return;
    //        }


    //        if (date == null) return;
    //        try
    //        {
    //            string[] sep = date.Split(',');

    //            double b = double.Parse(sep[0], System.Globalization.CultureInfo.InvariantCulture);
    //            double s_lat = double.Parse(sep[3], System.Globalization.CultureInfo.InvariantCulture) / 100;
    //            double s_long = double.Parse(sep[4], System.Globalization.CultureInfo.InvariantCulture) / 100;
    //            double b_lat = double.Parse(sep[5], System.Globalization.CultureInfo.InvariantCulture) / 100;
    //            double b_long = double.Parse(sep[6], System.Globalization.CultureInfo.InvariantCulture) / 100;
    //            double s_h = double.Parse(sep[7], System.Globalization.CultureInfo.InvariantCulture);
    //            double b_d = double.Parse(sep[8], System.Globalization.CultureInfo.InvariantCulture);

    //            Navigation_Date.Ship.Heading = s_h;
    //            Navigation_Date.Ship.Position = new Geo_Pos(s_lat, s_long, null);


    //            For_Date_Proc Date_Proc = Beac =>
    //            {
    //                Beac.Number = b;
    //                Beac.Depth = b_d;
    //                //Beac.X_offset = x;
    //                //Beac.Y_offset = y;
    //                Beac.Position = new Geo_Pos(b_lat, b_long, null);
    //                //Action update = async () => // синхронно запускаем обновление интенрфейса исключительно async
    //                //{
    //                //    //await Objects_Links.M_Window?.Dispatcher.InvokeAsync(Objects_Links.M_Window.update_HANS);
    //                //    await Objects_Links.M_Window?.Dispatcher.InvokeAsync(() => {
    //                //        Objects_Links.M_Window.device_updater.HANS_update(true);
    //                //        Objects_Links.M_Window.update_HANS();
    //                //        Objects_Links.M_Window.device_updater.Ship_update(true);
    //                //    }
    //                //      Action update =  () => // синхронно запускаем обновление интенрфейса исключительно async
    //                //      {
    //                //          //await Objects_Links.M_Window?.Dispatcher.InvokeAsync(Objects_Links.M_Window.update_HANS);
    //                //          Objects_Links.M_Window?.Dispatcher.InvokeAsync(() => {
    //                //              Objects_Links.M_Window.device_updater.HANS_update(true);
    //                //              Objects_Links.M_Window.update_HANS();
    //                //              Objects_Links.M_Window.device_updater.Ship_update(true);
    //                //          }

    //                // );
    //                //};
    //                //update();
    //                OnNewDate(Beac);
    //            };

    //            if (Navigation_Date.HANS.beacons.ContainsKey(b))
    //            {
    //                Date_Proc(Navigation_Date.HANS.beacons[b]);
    //                Status_List.push("Beacon number " + b + " Updated" + DateTimeOffset.Now.ToString());
    //                return;
    //            }
    //            else
    //            {
    //                Device_Date.HANS.Beacon beac = new Device_Date.HANS.Beacon();
    //                Date_Proc(beac);
    //                Navigation_Date.HANS.beacons.Add(b, beac);
    //                Status_List.push("Beacon number " + b + " Created and Updated" + DateTimeOffset.Now.ToString());
    //                return;
    //            }

    //        }
    //        catch (Exception e)
    //        {
    //            Status_List.push("Beacon date processing FALSE " + e.Message + " " + DateTimeOffset.Now.ToString());
    //            return;
    //        }
    //    }

    //    public override void dateProcess()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void initActionInfoList()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public SerialPort GetSerialPort()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool StartActions()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool StopActions()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public string GetName()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool IsInAction()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}


}
