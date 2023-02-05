using NaviSystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NaviSystem.DeviceControl
{
    public class TaskManager
    {
        //CancellationTokenSource tokenSource2 = new CancellationTokenSource();
        //Dictionary<string, Task_and_TokenSource> current_tasks_dic = new Dictionary<string, Task_and_TokenSource>();
        Dictionary<string, Task_and_TokenSource> current_tasks_dic = new Dictionary<string, Task_and_TokenSource>();

        struct Task_and_TokenSource  // структура с сылками на задачу и источник ее отмены
        {
            public string DeviceName;
            public CancellationTokenSource TSource;
            public Task tsk;
            public Task_and_TokenSource(CancellationTokenSource S, Task T, string name)
            {
                DeviceName = name;
                TSource = S;
                tsk = T;
            }
        }

        public bool IsActive(string name)
        {
            return current_tasks_dic.ContainsKey(name);
        }

        public void Cansel(ACDevice device)
        {
            var actionList = device.GetActions();

            for (int i = 0; i < actionList.Count; i++)
            {
                if (current_tasks_dic.ContainsKey(actionList[i].name))
                {
                    current_tasks_dic[actionList[i].name].TSource.Cancel();
                    current_tasks_dic[actionList[i].name].tsk.Wait();
                    Status_List.push("Task for "  + actionList[i].name + " CANCELED " + DateTimeOffset.Now.ToString());
                    //освободить ресурсы
                    current_tasks_dic[actionList[i].name].TSource.Dispose();
                    current_tasks_dic[actionList[i].name].tsk.Dispose();
                    current_tasks_dic.Remove(actionList[i].name);
                }

            }
            
        }
        public bool Start(ACDevice device)
        {
            bool result = false;
            foreach (ActionInfo item in device.GetActions())
            {
                if (!current_tasks_dic.ContainsKey(item.name))
                {

                    CancellationTokenSource cancelTokSource = new CancellationTokenSource(); // источник признака отмены задачи
                    Task t = new Task((object n) =>
                    {
                        Status_List.push("Task for " + item.name + " STARTED " + DateTimeOffset.Now.ToString());
                        while (!cancelTokSource.IsCancellationRequested)
                        {
                            item.function();  // запуск функции
                            Thread.Sleep(item.sleep_time);
                        }
                        item.clean?.Invoke();

                    }, cancelTokSource.Token, cancelTokSource.Token);

                    current_tasks_dic.Add(item.name, new Task_and_TokenSource(cancelTokSource, t, item.name)); // структура содержащая источник признака отмены, саму задачу, name
                    t.Start();
                    result = true;
                }
            }
            return result;
        }


    }
}
