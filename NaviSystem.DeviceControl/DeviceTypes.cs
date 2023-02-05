using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using NaviSystem.Data;

namespace NaviSystem.DeviceControl
{
    public abstract class ACDevice
    {
        #region Event
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
        #endregion

        protected Queue<object> queue = new Queue<object>();
        protected SerialPort sPort = new SerialPort();
        protected string name = "";
        public List<ActionInfo> actionList = new List<ActionInfo>();



        public List<ActionInfo> GetActions()
        {
            if (actionList.Count > 0) return actionList;
            else return null;
        }

        public string GetName()
        {
            if (name != "") return name;
            else return null;
        }

        protected bool OpenPort()
        {
            if (sPort.IsOpen)
            {
                //Status_List.push("SerialPort " + sPort.PortName + " IsOpen " + DateTimeOffset.Now.ToString());
                return true;
            }
            else
            {
                try
                {
                    sPort.Open();
                    Status_List.push("SerialPort " + sPort.PortName + " IsOpen " + DateTimeOffset.Now.ToString());
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool ClosePort()
        {

            if (sPort.IsOpen)
            {
                sPort.Close();
                Status_List.push("SerialPort " + sPort.PortName + " IsClosed " + DateTimeOffset.Now.ToString());
                return true;
            }
            else
            {
                return false;
            }

        }

        protected abstract void SetActionList();
    }

    #region Devices 
    public class TrackLink : ACDevice
    {
        bool isTest = true;
        protected BeaconData DataProcessing(object data)
        {
            //LQF "9,05/04/17,14:54:31,  4250.5598,-14718.5044,  4250.5538,-14718.5110,153.7, 198.4\r"
            //string[] b = new string[5];

            string date = data.ToString();
            if (date == null) return null;

            string[] sep = date.Split(',');

            double b = double.Parse(sep[0], System.Globalization.CultureInfo.InvariantCulture);
            double s_lat = double.Parse(sep[3], System.Globalization.CultureInfo.InvariantCulture) / 100;
            double s_long = double.Parse(sep[4], System.Globalization.CultureInfo.InvariantCulture) / 100;
            double b_lat = double.Parse(sep[5], System.Globalization.CultureInfo.InvariantCulture) / 100;
            double b_long = double.Parse(sep[6], System.Globalization.CultureInfo.InvariantCulture) / 100;
            double s_h = double.Parse(sep[7], System.Globalization.CultureInfo.InvariantCulture);
            double b_d = double.Parse(sep[8], System.Globalization.CultureInfo.InvariantCulture);
            bool e = s_long < 0;
            bool n = s_lat > 0;

            s_long = e ? s_long * (-1) : s_long;
            b_long = e ? b_long * (-1) : b_long;
            s_lat = !n ? s_lat * (-1) : s_lat;
            b_lat = !n ? b_lat * (-1) : b_lat;

            BeaconData NewData = new BeaconData()
            {
                Depth = b_d,
                Number = b,
                Longitude = b_long,
                Latitude = b_lat,
                E = e,
                N = n
            };

            BeaconData.GPSData = new GPSData()
            {
                Latitude = s_lat,
                Longitude = s_long,
                Heading = s_h,
                E = e,
                N = n
            };

            return NewData;
        }

        public TrackLink(Data.MySerialData s_date)
        {
            name = "TrackLink";
            if (!isTest)
            {
                sPort.PortName = s_date.PortName;
                sPort.BaudRate = (int)s_date.BaudRate;
                sPort.Parity = Parity.None;
                sPort.StopBits = StopBits.One;
                sPort.DataBits = (int)s_date.DataBits;
                sPort.Handshake = Handshake.None;
                sPort.RtsEnable = true;
                sPort.ReadTimeout = 300;
            }


            SetActionList();
        }

        void Clean()
        {
            ClosePort();
        }
        public void Read()
        {
            string buffer = (string)IntegrationTest.TrackLinkTest.testread();
            //if (!openPort()) return;

            //string buffer;
            //for (;;)
            //{
            //    try
            //    {
            //        buffer = mySerialPort.ReadLine();
            //        if (buffer.Length > 5) break;
            //    }
            //    catch (Exception)
            //    {
            //        return;
            //    }
            //}
            queue.Enqueue(buffer);
        }

        public void DataUpdate()
        {
            //LQF "9,05/04/17,14:54:31,  4250.5598,-14718.5044,  4250.5538,-14718.5110,153.7, 198.4\r"
            string date = "";
            //string[] b = new string[5];
            lock (queue)
            {
                if (queue.Count > 0)
                {
                    date = (string)queue.Dequeue();
                }
                else return;
            }
            if (date == null) return;
            BeaconData beacon;

            //try
            //{
                beacon = DataProcessing(date);
                if(beacon != null) OnNewDate(beacon);
            //}
            //catch (Exception e)
            //{
            //    throw new Exception(String.Format("TrackLink DataProcessing({0}) Fail message:{1}", date, e.Message));
            //}
            
        }

        protected override void SetActionList()
        {
            actionList.Add(new ActionInfo("TrackLinkDataUpdate", DataUpdate));
            actionList.Add(new ActionInfo("TrackLinkRead", Read, Clean));
        }
    }

    public class DVL : ACDevice
    {
        public DVL(Data.MySerialData s_date)
        {
            name = "DVL";

            sPort.PortName = s_date.PortName;
            sPort.BaudRate = 9600;
            sPort.Parity = Parity.None;
            sPort.StopBits = StopBits.One;
            sPort.DataBits = 8;
            sPort.Handshake = Handshake.None;
            sPort.RtsEnable = true;
            sPort.ReadTimeout = 500;

            SetActionList();
        }

        void Clean()
        {
            ClosePort();
        }

        private int status; //0-все хорошо, 1 - исключение открытия порта, 2 - исключение чтения
        public void Read()
        {
            if (!OpenPort()) return;

            byte[] buf = new byte[88];
            int count = 88;
            for (int i = 0; i < count; i++)
            {

                try
                {
                    buf[i] = (byte)sPort.ReadByte();
                }
                catch (Exception e)
                {
                    Status_List.push("DVL Reading Exception " + e.Message);

                    break; // !!!!!!!!!!!!!!
                }

                if (i > 0 && buf[i - 1] == 125)
                {
                    if (buf[i] == 1)
                    {
                        buf[0] = buf[i - 1];
                        buf[1] = buf[i];
                        i = 1;
                        count = 88;
                    }
                    else if (buf[i] == 0)
                    {
                        buf[0] = buf[i - 1];
                        buf[1] = buf[i];
                        i = 1;
                        count = 47;
                    }
                }
            }
            if (buf.Length > 46) queue.Enqueue(buf);
            else return;
        }

        public DVLData DataProcessing(byte[] data)
        {
            byte[] dataTemp = new byte[90];

            bool state = false;
            short crc = new short();
            int crc1 = new int();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 125)
                {

                    if (data[i + 1] == 1 && data.Length - i >= 88)
                    {
                        for (int i1 = 0; i1 < 88; i1++)
                        {
                            dataTemp[i1] = data[i + i1];
                            if (i1 != 86 && i1 != 87)
                            {
                                crc1 += dataTemp[i1];
                                crc = (short)crc1;
                            }

                        }
                        if (crc == BitConverter.ToInt16(dataTemp, 86))
                        {
                            state = true;
                        }
                        break;
                    }
                    else if (data[i + 1] == 0 && data.Length - i >= 47)
                    {
                        for (int i2 = 0; i2 < 47; i2++)
                        {
                            dataTemp[i2] = data[i + i2];
                            if (i2 != 45 && i2 != 46)
                            {
                                crc1 += dataTemp[i2];
                                crc = (short)crc1;
                            }

                        }
                        if (crc == BitConverter.ToInt16(dataTemp, 45))
                        {
                            state = true;
                        }
                        break;
                    }
                }
                state = false;
            }
            if (state == false)
            {
                Status_List.push("DVL DataProcessing() Error");
                return null; //устройство не читается
            } 
            else  // устройство прочитано (формируем данные)
            {
             

               return new DVLData()
               {
                    Heading = BitConverter.ToInt16(dataTemp, 52) * 0.01,
                    X_Vel = BitConverter.ToInt16(dataTemp, 5),
                    Y_Vel = BitConverter.ToInt16(dataTemp, 7),
                    Z_Vel = BitConverter.ToInt16(dataTemp, 9),
                    Pitch = BitConverter.ToInt16(dataTemp, 48) * 0.01,
                    Roll = BitConverter.ToInt16(dataTemp, 50) * 0.01,
                    X_Ref_Vel = BitConverter.ToInt16(dataTemp, 22),
                    Y_Ref_Vel = BitConverter.ToInt16(dataTemp, 24),
                    Z_Ref_Vel = BitConverter.ToInt16(dataTemp, 26),
                    Depth = BitConverter.ToInt16(dataTemp, 46),
                    Temperature = BitConverter.ToInt16(dataTemp, 43) * 0.01
                    //public double[,] Measured_Points { get; set; } Обязательно добавить
               };
            }
        }

        public void DataUpdate()
        {
            byte[] data = new byte[100];

            lock (queue)
            {
                if (queue.Count > 0)
                {
                    data = (byte[])queue.Dequeue();
                }
                else return;
            }
            if (data == null) return;
            DVLData dvlData;

            try
            {
                dvlData = DataProcessing(data);
                if(dvlData!=null) Status_List.push(dvlData.Pitch.ToString());
                // if (dvlData != null) OnNewDate(dvlData);
            }
            catch (Exception e)
            {
                Status_List.push(String.Format("DVL DataUpdate() Fail message:{0}", e.Message));
            }

        }

        protected override void SetActionList()
        {
            actionList.Add(new ActionInfo("DVLDataUpdate", DataUpdate));
            actionList.Add(new ActionInfo("DVLRead", Read, Clean));
        }
    }

    public class SonarDyne : ACDevice
    {
        public SonarDyne(Data.MySerialData s_date)
        {
            name = "SonarDyne";

            sPort.PortName = s_date.PortName;
            sPort.BaudRate = (int)s_date.BaudRate;
            sPort.Parity = Parity.None;
            sPort.StopBits = StopBits.One;
            sPort.DataBits = 8;
            sPort.Handshake = Handshake.None;
            sPort.RtsEnable = true;
            sPort.ReadTimeout = 300;

            SetActionList();
        }

        void Clean()
        {
            ClosePort();
        }
        public void Read()
        {
            //string buffer = (string)IntegrationTest.TrackLinkTest.testread();
            if (!OpenPort()) return;

            string buffer;
            for (; ;)
            {
                try
                {
                buffer = sPort.ReadLine();
                Status_List.push("SonarDyne date: " + buffer);


                if (buffer.Length > 5) break;
            }
                catch (Exception)
                {
                    return;
                }
            }
            queue.Enqueue(buffer);
        }

        protected BeaconData DataProcessing(string data)
        {
            if (data == null) return null;
            try
            {
                int B_ind = data.IndexOf("B", 8);
                int X_ind = data.IndexOf("X", 8);
                int Y_ind = data.IndexOf("Y", 8);
                int D_ind = data.IndexOf("D", 8);
                int P_ind = data.IndexOf("P", 8);
                string beacon = data.Substring(B_ind + 2, X_ind - 1 - B_ind - 2);
                string X_m = data.Substring(X_ind + 2, Y_ind - 1 - X_ind - 2);
                string Y_m = data.Substring(Y_ind + 2, D_ind - 1 - Y_ind - 2);
                string D_m = data.Substring(D_ind + 2, P_ind - 1 - D_ind - 2);
                double Beacon_number = double.Parse(beacon, System.Globalization.CultureInfo.InvariantCulture);
                double x = double.Parse(X_m, System.Globalization.CultureInfo.InvariantCulture);
                double y = double.Parse(Y_m, System.Globalization.CultureInfo.InvariantCulture);
                double d = double.Parse(D_m, System.Globalization.CultureInfo.InvariantCulture);

                BeaconData NewData = new BeaconData()
                {
                    Depth = d,
                    Number = Beacon_number,
                    X_offset = x,
                    Y_offset = y
                };

                return NewData;

            }
            catch (Exception e)
            {
                Status_List.push("SonarDyne date processing Fail " + e.Message);
                return null;
            }
        }

        public void DataUpdate()
        {
//SonarDyne test data
            string data = "";

            lock (queue)
            {
                if (queue.Count > 0)
                {
                    data = (string)queue.Dequeue();
                }
                else return;
            }

            if (data == null) return;
            BeaconData beacon;

            try
            {
                beacon = DataProcessing(data);
                
                if (beacon != null)
                {
                    Status_List.push("SonarDyne date processing OK");
                    OnNewDate(beacon);
                }
                else Status_List.push("SonarDyne date processing NULL");
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("SonarDyne DataProcessing({0}) Fail message:{1}", data, e.Message));
            }

        }

        protected override void SetActionList()
        {
            actionList.Add(new ActionInfo("SonarDyneDataUpdate", DataUpdate, Clean, 200));
            actionList.Add(new ActionInfo("SonarDyneRead", Read, Clean, 200));
        }
    }

    public class GPS : ACDevice
    {
        SerialPort headPort = new SerialPort();
        bool hasHedPort = false;
        public GPS(Data.MySerialData s_date)
        {
            name = "GPS";

            sPort.PortName = s_date.PortName;
            sPort.BaudRate = (int)s_date.BaudRate;
            sPort.Parity = Parity.None;
            sPort.StopBits = StopBits.One;
            sPort.DataBits = 8;
            sPort.Handshake = Handshake.None; 
            sPort.RtsEnable = true;
            sPort.ReadTimeout = 200;

            SetActionList();

            if(s_date.PortNameOut.Length > 2 & s_date.BaudRateOut > 0)
            {
                headPort.PortName = s_date.PortNameOut;
                headPort.BaudRate = (int)s_date.BaudRateOut;
                headPort.Parity = Parity.None;
                headPort.StopBits = StopBits.One;
                headPort.DataBits = 8;
                headPort.Handshake = Handshake.None;
                headPort.RtsEnable = true;
                headPort.ReadTimeout = 300;
                hasHedPort = true;
            }
        }

        void Clean()
        {
            ClosePort();
            CloseHeadPort();
        }
        public void Read()
        {
            //<!-- Образец пакета (2016 - 2019гг.) --!>
            //string buffer = "L131.914817l43.105100H333.0*25P3233.37D11.4V57.3d\r";

            //<!-- Образец пакета (2019г. - н.в.) --!>
            /*string buffer = "$GPRMC,021642,A,4546.116,N,13807.579,E,10.3,281.5,130819,5.2,W*2E\n\r
                               $HEHDT,272.0,T*28\n\r
                               $SDDBT,111,f,666,M,T*2A\n\r";*/



            if (!OpenPort()) return;

            string buffer;
            //string buffer = "$GPRMC,021642,A,4546.116,N,13807.579,E,10.3,281.5,130819,5.2,W*2E\r\n$HEHDT,272.0,T*28\r\n$SDDPT,111,666,T*2A\r\n";

            for (; ; )
            {
                try
                {
                    //buffer = sPort.ReadExisting();
                    if ((buffer = sPort.ReadExisting()) != string.Empty)
                    {                        
                        Status_List.push(buffer);
                        break;
                    }
                    //break;
                    //if (buffer.Length > 3) break;
                    //buffer = sPort.ReadTo(",F*");
                    //break;                  
                }
                catch (Exception)
                {
                    return;
                }
            }
            queue.Enqueue(buffer);
        }

        protected GPSData DataProcessing(string data)
        {
            if (data == null) return null;
            try
            {
                //$GPRMC
                int GPRMC_ind = data.IndexOf("$GPRMC,", 0);
                int A_ind = data.IndexOf("A,", GPRMC_ind);
                int N_ind = data.IndexOf(",N,", GPRMC_ind);
                int E_ind = data.IndexOf(",E,", GPRMC_ind);
                int V_ind = data.IndexOf(",", E_ind + 3);
                int H_ind = data.IndexOf(",", V_ind + 1);

                string latitude = data.Substring(A_ind + 2, N_ind - A_ind - 2);
                string longitude = data.Substring(N_ind + 3, N_ind - A_ind - 1);
                string vel = data.Substring(E_ind + 3, V_ind - 1 - E_ind - 2);
                string dir = data.Substring(V_ind + 1, H_ind - 1 - V_ind - 2);

                //$HEHDT
                int HEHDT_ind = data.IndexOf("$HEHDT,", 0);
                int Hyro1_ind = data.IndexOf(",", HEHDT_ind);
                int Hyro2_ind = data.IndexOf(",", Hyro1_ind + 1);
                int Star_ind = data.IndexOf("*", Hyro1_ind);

                //string heading = "272.0";
                //string star = "0.28";

                string heading = data.Substring(Hyro1_ind + 1, Hyro2_ind - 1 - Hyro1_ind);
                string star = data.Substring(Star_ind + 1, Star_ind - Hyro2_ind);

                //$SDDBT
                //int SDDBT_ind = data.IndexOf("$SDDPT", 0);
                //int F_ind = data.IndexOf(",", SDDBT_ind);
                //int F2_ind = data.IndexOf(",", F_ind + 1);
                //int M_ind = data.IndexOf(",M,", SDDBT_ind);
                string depth;

                if ((data.IndexOf("$SDDPT,,", 0) == -1) && (data.IndexOf("$SDDPT,", 0) != -1))
                {
                    int SDDBT_ind = data.IndexOf("$SDDPT", 0);
                    int F_ind = data.IndexOf(",", SDDBT_ind);
                    int F2_ind = data.IndexOf(",", F_ind + 1);
                    depth = data.Substring(F_ind + 1, F2_ind - F_ind - 1);
                }
                else { depth = "0"; }

                //string depth = data.Substring(F_ind + 1, F_ind - SDDBT_ind - 3);

                //Parsing
                double Lat = double.Parse(latitude, System.Globalization.CultureInfo.InvariantCulture);
                double Long = double.Parse(longitude, System.Globalization.CultureInfo.InvariantCulture);

                //Transorm from minutes to #.##
                Long = (Long * Math.Pow(10, 3)) / Math.Pow(10, 5);
                double degrees = (int) Long;
                double TransformMinutes = 5 * (Long - degrees) / 3;
                Long = degrees + TransformMinutes;

                Lat = (Lat * Math.Pow(10, 3)) / Math.Pow(10, 5);
                degrees = (int)Lat;
                TransformMinutes = 5 * (Lat - degrees) / 3;
                Lat = degrees + TransformMinutes;

                double Head = double.Parse(heading, System.Globalization.CultureInfo.InvariantCulture);
                double Vel = double.Parse(vel, System.Globalization.CultureInfo.InvariantCulture);
                double Dir = double.Parse(dir, System.Globalization.CultureInfo.InvariantCulture);

                double Dep = double.Parse(depth, System.Globalization.CultureInfo.InvariantCulture);
                OnNewDate(depth);

                GPSData NewData = new GPSData()
                {
                    Longitude = Long,
                    Latitude = Lat,
                    Heading = Head,
                    Depth = Dep,
                    Velocity = Vel,
                    VelocityDirection = Dir
                };

                //'\n' + '\r' 0D 0A 
              if (hasHedPort) SendToPort(String.Format("$HEHDT,{0},T*{1}", heading, star)); //БУДЬ ВНИМАТЕЛЕН! (добавил концевик)

                Status_List.push(data);

                return NewData;
            }
            catch (Exception e)
            {
                Status_List.push("GPS date processing Fail " + e.Message);
                return null;
            }
        }

        public void DataUpdate()
        {
            //SonarDyne test data
            string data = "";

            lock (queue)
            {
                if (queue.Count > 0)
                {
                    data = (string)queue.Dequeue();
                }
                else return;
            }

            if (data == null) return;
            GPSData gpsData;

            try
            {
                gpsData = DataProcessing(data);
                if (gpsData != null) OnNewDate(gpsData);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("GPS DataUpdate({0}) Fail message:{1}", data, e.Message));
            }

        }

        public bool OpenHeadPort()
        {
            if (headPort.IsOpen)
            {
                //Status_List.push("SerialPort (Head) " + sPort.PortName + " IsOpen " + DateTimeOffset.Now.ToString());
                return true;
            }
            else
            {
                try
                {
                    headPort.Open();
                    Status_List.push("SerialPort (Head) " + headPort.PortName + " IsOpen " + DateTimeOffset.Now.ToString());
                    return true;
                }
                catch (Exception e)
                {
                    Status_List.push("SerialPort (Head) open " + headPort.PortName + e.Message);
                    return false;
                }
        
            }
        }
        public bool CloseHeadPort()
        {

            if (headPort.IsOpen)
            {
                headPort.Close();
                Status_List.push("SerialPort (Head) " + headPort.PortName + " IsClosed " + DateTimeOffset.Now.ToString());
                return true;
            }
            else
            {
                return false;
            }
        }
        public void SendToPort(string msg)
        {
            if (!OpenHeadPort()) return;
            else
            {
                try
                {
                    headPort.Write(msg+Environment.NewLine);
                    Status_List.push("SerialPort (Head) OK:"+ msg);
                }
                catch (Exception e)
                {
                    Status_List.push("SerialPort (Head) " + headPort.PortName +" sending error " + e.Message);
                    throw;
                }
            }
        }

        protected override void SetActionList()
        {
            actionList.Add(new ActionInfo("GPSDataUpdate", DataUpdate, Clean, 200));
            actionList.Add(new ActionInfo("GPSRead", Read, Clean, 200));
        }
    }

    //<!-- Раскомментировать при подключении эхолота в качетве самостоятельного устройства --!>

    /*public class VesselDepth : ACDevice
    {
        public VesselDepth(Data.MySerialData s_date)
        {
            name = "VesselDepth";

            sPort.PortName = s_date.PortName;
            sPort.BaudRate = 4800;
            sPort.Parity = Parity.None;
            sPort.StopBits = StopBits.One;
            sPort.DataBits = 8;
            sPort.Handshake = Handshake.None;
            sPort.RtsEnable = true;
            sPort.ReadTimeout = 500;

            SetActionList();
        }

        void Clean()
        {
            ClosePort();
        }

        public void Read()
        {
            //string buffer = "$SDDPH$SDDPT,5.66,7878";
            if (!OpenPort()) return;

            string buffer;

            try
            {
                //Status_List.push("VesselDepth try Read");
                buffer = sPort.ReadLine();
                Status_List.push("VesselDepthRead: " + buffer);
                //Status_List.push(buffer);
            }
            catch (Exception)
            {
                return;
            }
            try
            {
                //int A_ind = buffer.IndexOf("$SDDPT", 0);
                //int A_ind = buffer.IndexOf("$SDDPT,", 0);
                //int O_ind = buffer.LastIndexOf(",", StringComparison.Ordinal);
                //int A_ind1 = buffer.IndexOf(",", 0);
                //string h = buffer.Substring(A_ind1);
                //int y = buffer.Length;
                //string strDepth = buffer.Substring(A_ind1 + 1, O_ind-1);

                //Status_List.push("VesselDepthRead: " + strDepth);
                //double depth = double.Parse(strDepth, System.Globalization.CultureInfo.InvariantCulture);///100; //Важно!
                //OnNewDate(depth);


                int A_ind = buffer.IndexOf("$SDDPT,", 0);
                int O_ind = buffer.IndexOf("*", 0);
                //int length = buffer.Length;
                //string strDepth_1 = buffer.Substring(A_ind + 7, length - 7 - A_ind - 1);
                string strDepth_1 = buffer.Substring(A_ind, O_ind - A_ind);
                string strDepth_2 = strDepth_1.Remove(6, 1);
                int A_ind_1 = strDepth_2.IndexOf("T", 0);
                int O_ind_1 = strDepth_2.IndexOf(",", 0);
                string strDepth = strDepth_2.Substring(A_ind_1 + 1, O_ind_1 - 1 - A_ind_1);

                Status_List.push("VesselDepthRead: " + strDepth);

                double depth = double.Parse(strDepth, System.Globalization.CultureInfo.InvariantCulture);// / 100;
                OnNewDate(depth);

            }
            catch (Exception e)
            {
                Status_List.push("VesselDepthRead error" + e.Message);
            }


        }
        protected override void SetActionList()
        {
            actionList.Add(new ActionInfo("VesselDepthRead", Read, Clean, 200));
        }
    }*/

    public class WinchCabel : ACDevice
    {
        public WinchCabel(Data.MySerialData s_date)
        {
            name = "WinchCabel";

            sPort.PortName = s_date.PortName;
            sPort.BaudRate = 9600;
            sPort.Parity = Parity.None;
            sPort.StopBits = StopBits.One;
            sPort.DataBits = 8;
            sPort.Handshake = Handshake.None;
            sPort.RtsEnable = true;
            sPort.ReadTimeout = 300;
            

            SetActionList();
        }

        void Clean()
        {
            ClosePort();
        }

        public void Read()
        {
            //string buffer = "L135.869983l43.482600H056.9*25P3233.37D11.4V57.3d\r"
            if (!OpenPort()) return;

            string buffer;

            try
            {
                buffer = sPort.ReadLine();
                //Status_List.push(buffer);
            }
            catch (Exception)
            {
                return;
            }

            try
            {

                double lenght = double.Parse(buffer, System.Globalization.CultureInfo.InvariantCulture);
                OnNewDate(lenght);
            }
            catch (Exception e)
            {
                Status_List.push("VinchCabelRead error" + e.Message);
            }


        }
        protected override void SetActionList()
        {
            actionList.Add(new ActionInfo("VinchCabelRead", Read, Clean, 300));
        }
    }
    #endregion

}
