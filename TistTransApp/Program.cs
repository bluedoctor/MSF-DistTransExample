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
            System.Diagnostics.Process.Start(MSF_Host);
        }
    }
}
