using DistTransServices;
using PWMIS.EnterpriseFramework.Service.Basic;
using PWMIS.EnterpriseFramework.Service.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransClient
{
    class Program
    {
        static void Main(string[] args)
        {
            DistTrans3PCState state ;//= DistTrans3PCState.Rep_Yes_1PC;
            if (typeof(DistTrans3PCState).IsEnum)
            {
                state = (DistTrans3PCState)Enum.Parse(typeof(DistTrans3PCState), "Rep_Yes_1PC");
                
            }

            Proxy client = new Proxy();
            client.ErrorMessage += client_ErrorMessage;
            Console.Write("请输入服务器的主机名或者IP地址(默认 127.0.0.1)：");
            string host = Console.ReadLine();
            if (string.IsNullOrEmpty(host))
                host = "127.0.0.1";
            Console.WriteLine("服务地址：{0}", host);

            Console.Write("请输入服务的端口号(默认 8888)：");
            string port = Console.ReadLine();
            if (string.IsNullOrEmpty(port))
                port = "8888";
            Console.WriteLine("服务端口号：{0}", port);

            client.ServiceBaseUri = string.Format("net.tcp://{0}:{1}", host, port);
            Console.WriteLine("当前客户端代理的服务基础地址是：{0}", client.ServiceBaseUri);
            Console.WriteLine();
            Console.WriteLine("MSF 分布式事务 模式调用示例：");
            /*
            ServiceRequest request = new ServiceRequest();
            request.ServiceName = "DTCService";
            request.MethodName = "RegisterTransaction";
            request.Parameters = new object[] { "1234567890" };

            client.Subscribe<string, DistTrans3PCState, DistTrans3PCState>(request,
                s =>
                {
                    Console.WriteLine("ServerMessage:{0}", s);
                    client.SendTextMessage("client ....");

                },
                para => {
                    Console.WriteLine("服务器回调消息：{0}",para);
                    Console.Write("事务是否执行成功？(yes/no)");
                    string repMsg1 = Console.ReadLine();
                    if (repMsg1 == "yes")
                        return DistTrans3PCState.Rep_Yes_1PC;
                    else
                        return DistTrans3PCState.Rep_No_1PC;
                }
            );
            */

            ServiceRequest request = new ServiceRequest();
            request.ServiceName = "DemoService";
            request.MethodName = "TransactionTest";
            request.Parameters = new object[] { 3,2 };
            
            /*
            Task<bool> task = client.RequestServiceAsync<bool, string, string>(request,
                s =>
                {
                    Console.WriteLine("接收到服务器指令：{0}", s);
                    Console.WriteLine("即将回复服务器，请输入回复内容(yes/no/错误信息)，也可以直接关闭本进程。");
                    string rep = Console.ReadLine();
                    if (s == "CanCommit")
                    {
                        Console.WriteLine("回复结果:是否可以提交:{0}", rep);
                    }
                    else if (s == "DoCommit")
                    {
                        Console.WriteLine("回复结果:提交是否成功:{0}", rep);
                    }
                    Console.WriteLine();
                    return rep;
                }
                );

            try
            {
                task.Wait();
                Console.WriteLine("服务访问完成，结果：{0}", task.Result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("访问服务错误：{0}",ex.Message);
            }
             */ 

            client.RequestService<bool, string, string>(request.ServiceUrl, PWMIS.EnterpriseFramework.Common.DataType.Text, 
                r=>{
                    Console.WriteLine("服务访问完成，结果：{0}", r);
                },
                s => {
                    Console.WriteLine("接收到服务器指令：{0}", s);
                    Console.WriteLine("即将回复服务器，请输入回复内容(yes/no/错误信息)，也可以直接关闭本进程。");
                    string rep = Console.ReadLine();
                    if (s == "CanCommit")
                    {
                        Console.WriteLine("回复结果:是否可以提交:{0}", rep);
                    }
                    else if (s == "DoCommit")
                    {
                        Console.WriteLine("回复结果:提交是否成功:{0}", rep);
                    }
                    Console.WriteLine();
                    return rep;
                });


            System.Threading.Thread.Sleep(1000 * 60 * 60);
            string repMsg = "ok";
            while (repMsg != "")
            {
                Console.Write("回复服务器(输入为空，则退出)：>>");
                repMsg = Console.ReadLine();
                client.SendTextMessage(repMsg);
            }

          
            Console.ReadLine();

        }

        static void client_ErrorMessage(object sender, MessageSubscriber.MessageEventArgs e)
        {
            //如果是分布式事务，这里应该回滚
            Console.WriteLine("请求服务器错误：{0}", e.MessageText);
        }
    }
}
