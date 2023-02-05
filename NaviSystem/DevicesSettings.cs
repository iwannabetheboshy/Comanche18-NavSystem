using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NaviSystem.Data;
//using ViewModel;

namespace NaviSystem
{

    public class DevicesSettings // Данные настроек
    {
        public bool setCorrectly;
        public int deviceNumber { get; private set; }
        List<MySerialData> COM_List = new List<MySerialData>(); // список портов устройств
        System.IO.FileStream setFile;// = new System.IO.FileStream(Environment.CurrentDirectory.ToString() + "\\Settings.txt", System.IO.FileMode.OpenOrCreate);
        public DevicesSettings()
        {
            InitSetDate();
        }
        public void InitSetDate() // чтение настроек из файла
        {

            StreamReader r_file = new StreamReader((Environment.CurrentDirectory.ToString().Replace(@"bin\Debug", "Content")) + "\\Settings.txt");
            COM_List.Clear();
            try
            {
                List<string> file_lines = new List<string>();
                deviceNumber = 0;
                string s;
                while ((s = r_file.ReadLine()) != null) { file_lines.Add(s); };
                for (int i = 0; i < file_lines.Count; i += 4)
                {
                    MySerialData COM = new MySerialData();
                    COM.DeviceName = file_lines[i].Substring(9);
                    if (COM.DeviceName.Length < 3)
                    {
                        break;
                    }
                    COM.PortName = file_lines[i + 1].Substring(9);
                    COM.BaudRate = double.Parse(file_lines[i + 2].Substring(9), System.Globalization.CultureInfo.InvariantCulture);
                    COM.DataBits = double.Parse(file_lines[i + 3].Substring(9), System.Globalization.CultureInfo.InvariantCulture);

                    if (COM.DeviceName == "GPS")
                    {
                        COM.PortNameOut = file_lines[i + 4].Substring(12);
                        COM.BaudRateOut = double.Parse(file_lines[i + 5].Substring(12), System.Globalization.CultureInfo.InvariantCulture);
                        i += 2;
                    }

                    /* Проверка правильности подключния устройств (при эксплуатации разкоментировать)
                    if (existing_ports.Contains(COM.PortName)) COM_List.Add(COM);
                    else Status_List.push(COM.PortName + " Does Not Exist " + DateTimeOffset.Now.ToString()); */
                    COM_List.Add(COM); // при эксплуатации закоментировать
                    deviceNumber++;
                }
                setCorrectly = true;
            }
            catch (Exception e)
            {
                Status_List.push("Read Settings File Error " + e.Message + DateTimeOffset.Now.ToString());
                setCorrectly = false;
            }
            finally { r_file.Close(); }
        }
        public void ManualSet(List<MySerialData> arr) // ручное формирование списка портов устройств (пользователем в Set_window)
        {
            //if (!init_set_file("2")) { set_correctly = false; return; };
            StreamWriter w_file = new StreamWriter((Environment.CurrentDirectory.ToString().Replace(@"bin\Debug", "Content")) + "\\Settings.txt");
            COM_List.Clear();
            try
            {
                int count = 1;
                deviceNumber = 0;
                foreach (var item in arr)
                {
                    COM_List.Add(item);
                    w_file.WriteLine(count.ToString() + "-Device:" + item.DeviceName);
                    w_file.WriteLine("PortName:" + item.PortName);
                    w_file.WriteLine("BaudRate:" + item.BaudRate.ToString());
                    w_file.WriteLine("DataBits:" + item.DataBits.ToString());

                    if (item.DeviceName == "GPS")
                    {
                        w_file.WriteLine("PortNameOut:" + item.PortNameOut.ToString());
                        w_file.WriteLine("BaudRateOut:" + item.BaudRateOut.ToString());
                    }

                    count++;
                    deviceNumber++;
                }
                setCorrectly = true;
            }
            catch (IOException exc)
            {
                Status_List.push("Write Settings File Error " + exc.Message + DateTimeOffset.Now.ToString());
                setCorrectly = false;
            }
            finally { w_file.Close(); }
        }
        public List<MySerialData> GetCOMList() // возвращает список портов устройств, если он сформирован
        {
            InitSetDate();
            if (setCorrectly) return COM_List;
            else return null;
        }
    }

    public class DevicesSettingsMenu
    {
        ListBox listBox;
        DevicesSettings devicesSettings=new DevicesSettings();

        public DevicesSettingsMenu(ListBox lBox)
        {
            listBox = lBox;
            UpdateList();
        }

        public void UpdateList()
        {
            listBox.Items.Clear();
            devicesSettings.InitSetDate();
            List<MySerialData> lis = devicesSettings.GetCOMList();
            foreach (MySerialData item in lis)
            {
                listBox.Items.Add(new MyGrid(item));
            }
        }

        public void Delete()
        {
            List<MyGrid> obj = new List<MyGrid>();
            foreach (MyGrid item in listBox.Items)
            {
                if ((bool)item.check.IsChecked) obj.Add(item);
            }
            foreach (MyGrid item in obj)
            {
                listBox.Items.Remove(item);
            }
        }

        public List<MySerialData> GetDevicesList()
        {
            List<MySerialData> list = new List<MySerialData>();
            foreach (MyGrid item in listBox.Items)
            {
                if ((bool)item.check.IsChecked)
                {
                    MySerialData com = new MySerialData()
                    {
                        DeviceName = item.combo_Device.SelectedItem.ToString(),
                        BaudRate = double.Parse(item.combo_BaudRate.SelectedItem.ToString(), System.Globalization.CultureInfo.InvariantCulture),
                        PortName = item.combo_PortName.SelectedItem?.ToString(),
                        //DataBits = double.Parse(item.combo_DataBits.SelectedItem.ToString(), System.Globalization.CultureInfo.InvariantCulture)
                    };
                    list.Add(com);
                }
            }
            return list;
        }

        public void Save()
        {
            List<MySerialData> list = new List<MySerialData>();
            foreach (MyGrid item in listBox.Items)
            {
                if ((bool)item.check.IsChecked)
                {
                    if (item.combo_Device.SelectedItem.ToString()== "GPS")
                    {
                        MySerialData com = new MySerialData()
                        {
                            DeviceName = item.combo_Device.SelectedItem.ToString(),
                            BaudRate = double.Parse(item.combo_BaudRate.SelectedItem.ToString(), System.Globalization.CultureInfo.InvariantCulture),
                            PortName = item.combo_PortName.SelectedItem?.ToString(),
                            DataBits = 8,
                            PortNameOut = item.combo_PortNameOut.SelectedItem?.ToString(),
                            BaudRateOut = double.Parse(item.combo_BaudRateOut.SelectedItem.ToString(), System.Globalization.CultureInfo.InvariantCulture)
                        };
                        list.Add(com);
                    }
                    else
                    {
                        MySerialData com = new MySerialData()
                        {
                            DeviceName = item.combo_Device.SelectedItem.ToString(),
                            BaudRate = double.Parse(item.combo_BaudRate.SelectedItem.ToString(), System.Globalization.CultureInfo.InvariantCulture),
                            PortName = item.combo_PortName.SelectedItem?.ToString(),
                            DataBits = 8
                        };
                        list.Add(com);
                    }
                   
                    
                }
            }
            devicesSettings.ManualSet(list);
        }

        public void Add()
        {
            listBox.Items.Add(new MyGrid());
        }


        public class MyGrid : Grid
        {
            public CheckBox check = new CheckBox();
            public ComboBox combo_Device = new ComboBox();
            public ComboBox combo_PortName = new ComboBox();
            public ComboBox combo_BaudRate = new ComboBox();
            public ComboBox combo_DataBits = new ComboBox();

            public ComboBox combo_PortNameOut = new ComboBox();
            public ComboBox combo_BaudRateOut = new ComboBox();

            public Label labelOut = new Label() { Content = "Heading to"};
            bool isGPS = false;

            public MyGrid()
            {
                this.Children.Add(SetCheck());
                this.Children.Add(SetDivice());
                this.Children.Add(SetPortName());
                this.Children.Add(SetBaudRate());
                //this.Children.Add(SetDataBits());
                this.HorizontalAlignment = HorizontalAlignment.Left;
                this.Height = 25;
                //this.Width = 188;
                
               
                this.Background = Brushes.DarkGray;
                //Children.Add
                combo_Device.SelectionChanged += Combo_Device_SelectionChanged;
            }

            private void Combo_Device_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                string type = combo_Device.Items.GetItemAt(combo_Device.SelectedIndex).ToString();
                if (type == "GPS")
                {
                    ChangeView();
                    isGPS = true;
                }
                else
                {
                    if (isGPS)
                    {
                        this.Height = 25;
                        this.Children.Remove(combo_PortNameOut);
                        this.Children.Remove(combo_BaudRateOut);
                        this.Children.Remove(labelOut);
                        isGPS = false;
                    }
                }

            }

            public MyGrid(MySerialData com)
            {
                this.Children.Add(SetCheck(true));
                //this.Margin = new Thickness(0, 0, 5, 0);
                this.Children.Add(SetDivice(com.DeviceName));
                this.Children.Add(SetPortName(com.PortName));
                this.Children.Add(SetBaudRate(com.BaudRate));
                //this.Children.Add(SetDataBits(com.DataBits));
                this.HorizontalAlignment = HorizontalAlignment.Left;
                this.Height = 25;
                //this.Width = 188;
                this.Background = Brushes.DarkGray;
                if (com.DeviceName == "GPS")
                {
                    ChangeView(com);
                    isGPS = true;
                }
                //Children.Add
                combo_Device.SelectionChanged += Combo_Device_SelectionChanged;
            }

            public void ChangeView(MySerialData com=null)
            {
                this.Height = 50;
                labelOut.HorizontalAlignment = HorizontalAlignment.Center;
                labelOut.VerticalAlignment = VerticalAlignment.Top;
                labelOut.Margin = new Thickness(2, 22, 2, 0);
                Grid.SetColumn(labelOut, 0);
                this.Children.Add(labelOut);
                if (com == null)
                {
                    this.Children.Add(SetPortNameOut());
                    this.Children.Add(SetBaudRateOut());
                }
                else
                {
                    this.Children.Add(SetPortNameOut(com.PortNameOut));
                    this.Children.Add(SetBaudRateOut(com.BaudRateOut));
                }
            }

            //CheckBox SetCheck()
            //{
            //    //check.Height = 25;
            //    //check.Width = 20;
            //    check.HorizontalAlignment = HorizontalAlignment.Left;
            //    check.Margin = new Thickness(168, 4, 0, 0);
            //    return check;
            //}

            CheckBox SetCheck(bool state=false)
            {
                //check.Height = 25;
                //check.Width = 20;
                check.HorizontalAlignment = HorizontalAlignment.Center;
                check.VerticalAlignment = VerticalAlignment.Top;
                check.Margin = new Thickness(2, 4, 2, 0);
                check.IsChecked = state;

                this.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                Grid.SetColumn(check, 3);

                return check;
            }

            //public ComboBox SetDivice()
            //{
            //    combo_Device.SelectedIndex = 0;
            //    combo_Device.FontSize = 10;
            //    foreach (string item in DevicesTypes.devicesTypes)
            //    {
            //        combo_Device.Items.Add(item);
            //    }
            //    combo_Device.Height = 25;
            //    combo_Device.Width = 65;
            //    combo_Device.HorizontalAlignment = HorizontalAlignment.Left;
            //    combo_Device.Margin = new Thickness(5, 0, 0, 0);
            //    return combo_Device;
            //}

            public ComboBox SetDivice(string type="")
            {
                
                combo_Device.SelectedIndex = 0;
                combo_Device.FontSize = 10;
                foreach (string item in DevicesTypes.devicesTypes)
                {
                    combo_Device.Items.Add(item);
                }
                if (type!="" && combo_Device.Items.Contains(type))
                {
                    combo_Device.Items.IndexOf(type);
                    combo_Device.SelectedIndex = combo_Device.Items.IndexOf(type);
                }
                combo_Device.Height = 25;
                //combo_Device.Width = ;
                combo_Device.HorizontalAlignment = HorizontalAlignment.Center;
                combo_Device.VerticalAlignment = VerticalAlignment.Top;
                combo_Device.Margin = new Thickness(2, 0, 2, 0);

                this.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                Grid.SetColumn(combo_Device, 0);

                return combo_Device;
            }

            //ComboBox SetPortName()
            //{
            //    combo_PortName.Height = 25;
            //    combo_PortName.Width = 50;
            //    combo_PortName.FontSize = 10;
            //    combo_PortName.HorizontalAlignment = HorizontalAlignment.Left;
            //    combo_PortName.Margin = new Thickness(75, 0, 0, 0);
            //    string[] st = System.IO.Ports.SerialPort.GetPortNames();
            //    foreach (string item in st)
            //    {
            //        combo_PortName.Items.Add(item);
            //    }
            //    return combo_PortName;
            //}

            ComboBox SetPortName(string name="")
            {
                combo_PortName.Height = 25;
                //combo_PortName.Width = 30;
                combo_PortName.FontSize = 10;
                combo_PortName.HorizontalAlignment = HorizontalAlignment.Center;
                combo_PortName.VerticalAlignment = VerticalAlignment.Top;
                //combo_PortName.Margin = new Thickness(75, 0, 0, 0);
                combo_PortName.Margin = new Thickness(2, 0, 2, 0);
                string[] st = System.IO.Ports.SerialPort.GetPortNames();
                foreach (string item in st)
                {
                    combo_PortName.Items.Add(item);
                }
                if (name!="" && combo_PortName.Items.Contains(name))
                {
                    combo_PortName.Items.IndexOf(name);
                    combo_PortName.SelectedIndex = combo_PortName.Items.IndexOf(name);
                }

                this.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                Grid.SetColumn(combo_PortName, 1);

                return combo_PortName;
            }

            ComboBox SetPortNameOut(string name = "")
            {
                combo_PortNameOut.Height = 25;
                //combo_PortName.Width = 30;
                combo_PortNameOut.FontSize = 10;
                combo_PortNameOut.HorizontalAlignment = HorizontalAlignment.Center;
                combo_PortNameOut.VerticalAlignment = VerticalAlignment.Top;
                //combo_PortName.Margin = new Thickness(75, 0, 0, 0);
                combo_PortNameOut.Margin = new Thickness(2, 22, 2, 0);
                string[] st = System.IO.Ports.SerialPort.GetPortNames();
                foreach (string item in st)
                {
                    combo_PortNameOut.Items.Add(item);
                }
                if (name != "" && combo_PortNameOut.Items.Contains(name))
                {
                    combo_PortNameOut.Items.IndexOf(name);
                    combo_PortNameOut.SelectedIndex = combo_PortNameOut.Items.IndexOf(name);
                }

                Grid.SetColumn(combo_PortNameOut, 1);

                return combo_PortNameOut;
            }

            //ComboBox SetBaudRate()
            //{
            //    combo_BaudRate.SelectedIndex = 0;
            //    combo_BaudRate.Items.Add("9600");
            //    combo_BaudRate.Items.Add("4800");
            //    combo_BaudRate.Height = 25;
            //    combo_BaudRate.Width = 45;
            //    combo_BaudRate.FontSize = 10;
            //    combo_BaudRate.HorizontalAlignment = HorizontalAlignment.Left;
            //    combo_BaudRate.Margin = new Thickness(110, 0, 0, 0);
            //    return combo_BaudRate;
            //}

            ComboBox SetBaudRate(double br=0)
            {
                if(br==0) combo_BaudRate.Items.Add("9600");
                else combo_BaudRate.Items.Add(br.ToString());
                combo_BaudRate.Items.Add("4800");
                combo_BaudRate.SelectedIndex = 0;
                combo_BaudRate.Height = 25;
                //combo_BaudRate.Width = 45;
                combo_BaudRate.FontSize = 10;
                combo_BaudRate.HorizontalAlignment = HorizontalAlignment.Center;
                combo_BaudRate.VerticalAlignment = VerticalAlignment.Top;
                combo_BaudRate.Margin = new Thickness(2, 0, 2, 0);

                this.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                Grid.SetColumn(combo_BaudRate, 2);

                return combo_BaudRate;
            }

            ComboBox SetBaudRateOut(double br = 0)
            {
                if (br == 0) combo_BaudRateOut.Items.Add("9600");
                else combo_BaudRateOut.Items.Add(br.ToString());
                combo_BaudRateOut.Items.Add("4800");
                combo_BaudRateOut.SelectedIndex = 0;
                combo_BaudRateOut.Height = 25;
                //combo_BaudRate.Width = 45;
                combo_BaudRateOut.FontSize = 10;
                combo_BaudRateOut.HorizontalAlignment = HorizontalAlignment.Center;
                combo_BaudRateOut.VerticalAlignment = VerticalAlignment.Top;
                combo_BaudRateOut.Margin = new Thickness(2, 22, 2, 0);
                Grid.SetColumn(combo_BaudRateOut, 2);

                return combo_BaudRateOut;
            }

            ComboBox SetDataBits()
            {
                combo_DataBits.Items.Add("8");
                combo_DataBits.SelectedIndex =1;
                //combo_DataBits.Items.Add("16");
                combo_DataBits.Height = 20;
                combo_DataBits.Width = 50;
                combo_DataBits.HorizontalAlignment = HorizontalAlignment.Left;
                combo_DataBits.Margin = new Thickness(20 + combo_Device.Width + combo_PortName.Width + combo_BaudRate.Width + 5 + 5 + 5, 0, 0, 0);
                return combo_DataBits;
            }

            ComboBox SetDataBits(double db)
            {
                combo_DataBits.SelectedIndex = 0;
                combo_DataBits.Items.Add(db.ToString());
                //combo_DataBits.Items.Add("16");
                combo_DataBits.Height = 20;
                combo_DataBits.Width = 50;
                combo_DataBits.HorizontalAlignment = HorizontalAlignment.Left;
                combo_DataBits.Margin = new Thickness(20 + combo_Device.Width + combo_PortName.Width + combo_BaudRate.Width + 5 + 5 + 5, 0, 0, 0);
                return combo_DataBits;
            }


        }
    }


}
