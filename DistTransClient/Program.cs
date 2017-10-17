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
            Console.WriteLine("请求服务器错误：{0}", e.MessageText);
        }
    }
}
