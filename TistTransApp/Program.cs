using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TistTransApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("MSF 分布式事务测试程序");
            Console.WriteLine("本测试程序将创建一个订单，并且扣减库存。");
            Console.WriteLine("在继续前，请确保事务协调器程序已经启动（程序位于 Host目录下的文件 PdfNetEF.MessageServiceHost.exe，双击启动，端口号：12345）");

            string MSF_Host = @"..\..\..\Host\PdfNetEF.MessageServiceHost.exe";
            if (!System.IO.File.Exists(MSF_Host))
                MSF_Host = "PdfNetEF.MessageServiceHost.exe";
            if (!System.IO.File.Exists(MSF_Host))
            {
                Console.Write("请输入MSF Host文件的完整地址(PdfNetEF.MessageServiceHost.exe)：");
                MSF_Host = Console.ReadLine();
            }
            if (!System.IO.File.Exists(MSF_Host))
            {
                Console.WriteLine("MSF Host文件不存在。");
                return;
            }

            if (!System.IO.Directory.Exists(@"..\..\..\Host\DataBase"))
            {
                System.IO.Directory.CreateDirectory(@"..\..\..\Host\DataBase");
            }
            Console.Write("需要初始化数据库么？(y/n，默认n)");
            var keyInfo1 = Console.ReadKey();
            if (keyInfo1.KeyChar == 'y')
            {
                System.IO.File.Copy(@"DataBase\OrdersDb.mdf", @"..\..\..\Host\DataBase\OrdersDb1.mdf", true);
                System.IO.File.Copy(@"DataBase\OrdersDb_log.ldf", @"..\..\..\Host\DataBase\OrdersDb_log1.ldf", true);
                System.IO.File.Copy(@"DataBase\ProductsDb.mdf", @"..\..\..\Host\DataBase\ProductsDb1.mdf", true);
                System.IO.File.Copy(@"DataBase\ProductsDb_log.ldf", @"..\..\..\Host\DataBase\ProductsDb1_log.ldf", true);
                Console.WriteLine(" 已经复制4个文件");
            }
            Console.WriteLine();



            string dir = System.IO.Path.GetDirectoryName(MSF_Host);
            System.IO.Directory.SetCurrentDirectory(dir);

            Console.Write("需要启动 【事务协调器器】服务宿主么？(y/n，默认n)");
            var keyInfo = Console.ReadKey();
            if (keyInfo.KeyChar == 'y')
            {
                System.Diagnostics.Process.Start("PdfNetEF.MessageServiceHost.exe");
            }
            Console.WriteLine();

            int portProduct = 12306;
            Console.WriteLine("启动 【商品服务】宿主，端口号：{0}", portProduct);
            System.Diagnostics.Process.Start("PdfNetEF.MessageServiceHost.exe", "127.0.0.1 "+portProduct);

            int portOrder = 12308;
            Console.WriteLine("启动 【商品服务】宿主，端口号：{0}", portOrder);
            System.Diagnostics.Process.Start("PdfNetEF.MessageServiceHost.exe", "127.0.0.1 " + portOrder);
            Console.WriteLine("服务全部启动完成，按任意键关闭本程序");
            Console.Read();
        }
    }
}
