using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SqlmapHelper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //string ip = "192.168.1.7";
            //int port = 8775;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tkratos\t\t\t");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Please Enter SqlmapApi Host:");
            string ip = Console.ReadLine();
            Console.WriteLine("Please Enter Port:");
            int port = int.Parse(Console.ReadLine());
            Console.WriteLine("GET(g)\t\t\t\t/POST(p)");
            string gp = Console.ReadLine();
            if (gp == "g")
            {
                await getFuzz(ip, port);
            }
            else if (gp == "p")
            {
              await  postFuzz(ip, port);
            }

        }

        private static async Task getFuzz(string ip, int port)
        {
            Console.WriteLine("Where is your url path");
            string path = Console.ReadLine();
            if (string.IsNullOrEmpty(ip) && string.IsNullOrEmpty(port.ToString()) && string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Input error! ");
            }
            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist!");
            }
            string[] strs1 = File.ReadAllLines(path);
            Console.WriteLine("---------------------------------------------------->");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(strs1.Count() + "\ttask are running,please wait!");
            List<string> TaskList = new List<string>();
            int urlCount = strs1.Length;
            using (SqlmapSessionManager manager = new SqlmapSessionManager(new SqlmapSession(ip, port)))
            {
                for (int i = 0; i < urlCount; i++)
                {
                    TaskList.Add(await manager.CreateNewTask());
                }
                string[] taskids = TaskList.ToArray();//得到全部taskid
                for (int i = 0; i < urlCount; i++)
                {
                    Dictionary<string, object> options = await manager.GetOptions(taskids[i]);
                    options["url"] = strs1[i];
                    options["flushSession"] = true;
                    await manager.TaskStart(taskids[i], options);

                }
                for (int i = 0; i < urlCount; i++)
                {
                    SqlmapStatusModel statusModel = await manager.GetSqlmapStatus(taskids[i]);
                    while (statusModel.Status != "terminated")
                    {
                        Thread.Sleep(new TimeSpan(0, 0, 10));
                        statusModel = await manager.GetSqlmapStatus(taskids[i]);
                    }
                    List<SqlmapScanLogModel> scanLogModels = await manager.GetLog(taskids[i]);

                    if (scanLogModels != null)
                    {
                        var data = await manager.GetScanData(taskids[i]);
                        foreach (var item in data)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[Luckly]" + item.Url + "\t\t->SQL injection vulnerability exists&&DBMS=" + item.Dbms);
                        }
                    }


                }
            }
        }
        private static async Task postFuzz(string ip, int port)
        {
            Console.Write("Please enter your url:");
            string url = Console.ReadLine();
            Console.Write("POST Data:");
            string datas = Console.ReadLine();
            Console.Write("Level:");
            int level =int.Parse( Console.ReadLine());
            using (SqlmapSessionManager manager = new SqlmapSessionManager(new SqlmapSession(ip, port)))
            {

                string taskid = await manager.CreateNewTask();

                Console.WriteLine("Task is running");
                Dictionary<string, object> options = await manager.GetOptions(taskid);
                options["url"] = url;
                options["data"] = datas;
                options["flushSession"] = true;
                options["level"] = level;
                options["header"] = "{ 'Content-Type':'application/json'}";
                await manager.TaskStart(taskid, options);


            
                    SqlmapStatusModel statusModel = await manager.GetSqlmapStatus(taskid);
                    while (statusModel.Status != "terminated")
                    {
                        Thread.Sleep(new TimeSpan(0, 0, 10));
                        statusModel = await manager.GetSqlmapStatus(taskid);
                    }
                    List<SqlmapScanLogModel> scanLogModels = await manager.GetLog(taskid);

                    if (scanLogModels != null)
                    {
                        var data = await manager.GetScanData(taskid);
                        foreach (var item in data)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[Luckly]" + item.Url + "\t\t->SQL injection vulnerability exists&&DBMS=" + item.Dbms);
                        }
                    }


                
            }
        }
    }
}

