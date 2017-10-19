using PWMIS.EnterpriseFramework.Service.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransServices
{
    public class DemoService:ServiceBase
    {
        public bool TransactionTest(int a, int b)
        {
            Console.WriteLine("TransactionTest Para a={0},b={1}",a,b);
            Console.WriteLine("即将向客户端发出 CanCommit指令，请按回车继续，也可以直接关闭本进程。");
            Console.ReadLine();
            string canCommit = base.CurrentContext.CallBackFunction<string, string>("CanCommit");
            if (canCommit.ToLower() == "yes")
            {
                Console.WriteLine("即将向客户端发出 DoCommit指令，请按回车继续，也可以直接关闭本进程。");
                Console.ReadLine();
                Console.WriteLine("计算 A/B={0}",a/b);
                string doCommit = base.CurrentContext.CallBackFunction<string, string>("DoCommit");
                if (doCommit.ToLower() == "yes")
                {
                    Console.WriteLine("客户端和服务器都已提交本地事务！");
                }
                else
                {
                    Console.WriteLine("客户端放弃提交事务，本地事务也已经回滚。客户端异常信息：{0}", doCommit);
                }
                return true;
            }
            else
            {
                Console.WriteLine("客户端未准备好提交事务，分布式事务已经取消。客户端异常信息：{0}", canCommit);
            }
            return false;
        }
    }
}
