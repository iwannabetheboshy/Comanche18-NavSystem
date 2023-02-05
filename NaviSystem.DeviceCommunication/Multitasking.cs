using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviSystem.DeviceCommunication
{
    /// <summary>
    /// Contains Device's single Action() information 
    /// </summary>
    public class DiviceActionInfo
    {
        /// <summary>
        /// Constructing Action() info
        /// </summary>
        /// <param name="n_name">Action information name "read" or "date_processing"</param>
        /// <param name="n_function">Desired Action()</param>
        /// <param name="n_clean">Will run when n_function's Thread stops</param>
        /// <param name="n_sleep_time">Sleep time between invocations of n_function in loop)</param>
        public DiviceActionInfo(string n_name, Action n_function, Action n_clean = null, int n_sleep_time = 30)
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
    }

    class Multitasking
    {
    }
}
