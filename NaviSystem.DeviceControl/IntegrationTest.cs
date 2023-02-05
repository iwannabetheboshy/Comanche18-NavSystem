using System;
using System.Threading;

namespace NaviSystem.DeviceControl
{
    public class IntegrationTest
    {
       
            static Random ran = new Random();
            public static class TrackLinkTest
            {
                //167.11$167.2$55.48$55.43
                //b_x = 167.2751, b_y = 55.4048, s_x = 167.2338, s_y = 55.3797
                static string date = "9,05/04/17,14:54:31,  5544.63,16718.45,  5544.645,16718.47,353.7, 198.4\r";
                static string date1 = "8,05/04/17,14:54:31,  5544.63,16718.45,  5544.643,16718.465,353.7, 178.4\r";
                static int num = 0;
                public static object testread()
                {
                    string[] sep = date.Split(',');
                    string[] sep2 = date.Split(',');
                    if (num == 0)
                    {
                        sep = date.Split(',');

                    }
                    else
                    {
                        sep = date1.Split(',');

                    }

                    double b = double.Parse(sep[0], System.Globalization.CultureInfo.InvariantCulture);
                    double s_lat = double.Parse(sep2[3], System.Globalization.CultureInfo.InvariantCulture) / 100;
                    double s_long = double.Parse(sep2[4], System.Globalization.CultureInfo.InvariantCulture) / 100;
                    double b_lat = double.Parse(sep[5], System.Globalization.CultureInfo.InvariantCulture) / 100;
                    double b_long = double.Parse(sep[6], System.Globalization.CultureInfo.InvariantCulture) / 100;
                    double s_h = double.Parse(sep[7], System.Globalization.CultureInfo.InvariantCulture);
                    double b_d = double.Parse(sep[8], System.Globalization.CultureInfo.InvariantCulture);


                    s_lat += (ran.NextDouble() - 0.3) / 8000;
                    s_long += (ran.NextDouble() - 0.5) / 8000;
                    b_lat += (ran.NextDouble() - 0.3) / 8000;
                    b_long += (ran.NextDouble() - 0.5) / 8000;
                    s_h += (ran.NextDouble() - 0.2) / 4;
                    b_d += (ran.NextDouble() - 0.5) * 10;


                    sep[3] = (s_lat * 100).ToString().Replace(',', '.');
                    sep[4] = (s_long * 100).ToString().Replace(',', '.');
                    sep[5] = (b_lat * 100).ToString().Replace(',', '.');
                    sep[6] = (b_long * 100).ToString().Replace(',', '.');
                    sep[7] = (s_h).ToString().Replace(',', '.');
                    sep[8] = (b_d).ToString().Replace(',', '.');


                    if (num == 0)
                    {
                        date = "";
                        foreach (var item in sep)
                        {
                            date += item + ",";
                        }
                        num = 1;
                        Thread.Sleep(300);
                        return date;
                    }
                    else
                    {
                        date1 = "";
                        foreach (var item in sep)
                        {
                            date1 += item + ",";
                        }
                        num = 0;
                        Thread.Sleep(300);
                        return date1;
                    }

                }
            }
            public static class SonarDyneTest
            {

            }
            public static class DVLTest
            {

            }
            public static class GPSTest
            {

            }

        

    }
}
