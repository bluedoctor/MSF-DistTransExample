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
            Console.WriteLine("--当前进程ID：{0}--",System.Diagnostics.Process.GetCurrentProcess().Id);

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
                System.IO.File.Copy(@"DataBase\OrdersDB_data.mdf", @"..\..\..\Host\DataBase\OrdersDB_data.mdf", true);
                System.IO.File.Copy(@"DataBase\OrdersDB_log.ldf", @"..\..\..\Host\DataBase\OrdersDB_log.ldf", true);
                System.IO.File.Copy(@"DataBase\ProductsDB_data.mdf", @"..\..\..\Host\DataBase\ProductsDB_data.mdf", true);
                System.IO.File.Copy(@"DataBase\ProductsDB_log.ldf", @"..\..\..\Host\DataBase\ProductsDB_log.ldf", true);
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
            Console.Write("启动 【商品服务】宿主，端口号：{0}", portProduct);
            var processProduct =System.Diagnostics.Process.Start("PdfNetEF.MessageServiceHost.exe", "127.0.0.1 "+portProduct);
            Console.WriteLine(" ,进程ID：{0}",processProduct.Id);
            Console.WriteLine();

            int portOrder = 12308;
            Console.Write("启动 【订单服务】宿主，端口号：{0}", portOrder);
            var processOrder = System.Diagnostics.Process.Start("PdfNetEF.MessageServiceHost.exe", "127.0.0.1 " + portOrder);
            Console.WriteLine(" ,进程ID：{0}", processOrder.Id);
            Console.WriteLine();
            Console.WriteLine("服务全部启动完成，按任意键关闭本程序");
            Console.Read();
        }
    }
}
