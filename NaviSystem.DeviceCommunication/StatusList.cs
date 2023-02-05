using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviSystem.DeviceCommunication
{
    public static class Status_List // список событий системы
    {
        public static List<string> list = new List<string>();
        public static void push(string status)
        {
            list.Add(status);
            Console.WriteLine(status);
        }
    }
}
