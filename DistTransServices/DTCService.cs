using PWMIS.EnterpriseFramework.Service.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransServices
{
    class DistTransInfo
    {
        public string ClientIdentity { get; set; }
        public string TransIdentity { get; set; }
        public DistTrans3PCState CurrentDTCState { get; set; }
        /// <summary>
        /// 上次状态改变的时间
        /// </summary>
        public DateTime LastStateTime { get; set; }

    }

    /// <summary>
    /// 分布式事务控制器
    /// </summary>
    class DTController
    {
        public string TransIdentity { get; set; }
        public DTController(string identity)
        {
            TransIdentity = identity;
        }

        public static DTController CheckStartController(string transIdentity)
        {
            return null;
        }

        /// <summary>
        /// 获取分布式事务的状态，注意某个调用线程可能会被阻塞等待
        /// </summary>
        /// <param name="resourceState">资源服务器的状态</param>
        /// <returns></returns>
        public DistTrans3PCState GetDTCState(DistTrans3PCState resourceState)
        {
            if (resourceState == DistTrans3PCState.Rep_No_1PC)
            {
                return DistTrans3PCState.Abort;
            }
            else
            {

            }

            return DistTrans3PCState.CanCommit;
        }
    }

    /// <summary>
    /// 分布式事务协调器服务，基于3PC过程。
    /// </summary>
    public class DTCService:ServiceBase
    {
        private int TransactionResourceCount;
        private DistTrans3PCState CurrentDTCState;

        private static System.Collections.Concurrent.ConcurrentBag<DistTransInfo> DTResourceList = new System.Collections.Concurrent.ConcurrentBag<DistTransInfo>();

        /// <summary>
        /// （资源服务器向协调器服务）注册事务操作
        /// </summary>
        /// <param name="identity">事务标识，需要在一个分布式事务下的服务请求必须使用同一个事务标识</param>
        /// <returns></returns>
        public ServiceEventSource RegisterTransaction(string identity)
        {
            //事务计数器累加
            System.Threading.Interlocked.Increment(ref TransactionResourceCount);
           
            return new ServiceEventSource(new object(),5, () => {
                //注册即向资源服务器发出 CanCommit请求
                //base.CurrentContext.PublishData(DistTrans3PCState.CanCommit);
                CurrentDTCState = DistTrans3PCState.CanCommit;
                while (CurrentDTCState != DistTrans3PCState.Completed)
                {
                    CurrentDTCState = base.CurrentContext.CallBackFunction<DistTrans3PCState, DistTrans3PCState>(CurrentDTCState);
                    Console.WriteLine("Callback Message:{0}", CurrentDTCState);
                }
             
            });
        }

        public bool BeginTransaction(string identity)
        {
            DistTransInfo info = new DistTransInfo();
            info.ClientIdentity = base.CurrentContext.Request.ClientIdentity;
            info.CurrentDTCState = DistTrans3PCState.CanCommit;
            info.LastStateTime = DateTime.Now;
            info.TransIdentity = identity;
            DTResourceList.Add(info);
            //获取一个当前事务标识的协调器线程
            DTController controller = DTController.CheckStartController(identity);

            CurrentDTCState = DistTrans3PCState.CanCommit;
            while (CurrentDTCState != DistTrans3PCState.Completed)
            {
                //获取资源服务器的事务状态
                info.CurrentDTCState = base.CurrentContext.CallBackFunction<DistTrans3PCState, DistTrans3PCState>(CurrentDTCState);
                CurrentDTCState = controller.GetDTCState(info.CurrentDTCState);
               
                Console.WriteLine("Callback Message:{0}", CurrentDTCState);
            }
            return true;
        }

        


        public override bool ProcessRequest(IServiceContext context)
        {
            return base.ProcessRequest(context);
        }
    }
}
