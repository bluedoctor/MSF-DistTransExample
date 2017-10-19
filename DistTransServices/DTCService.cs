/*
 * 3阶段提交事务协议
 * 参考 http://blog.csdn.net/zbuger/article/details/52453427
 * 请重点看到 “二阶段提交看起来确实能够提供原子性的操作，但是不幸的事，二阶段提交还是有几个缺点的：”这句话下面提出的问题
 * 根据2阶段提交的问题，实际上文章中有关3阶段提交的过程描述并不完全准确，请读者仔细思考。
 * 
 * 在本程序实现过程中，将协调器发出并且资源服务器都接收到 PreCommit作为在1阶段，所有资源服务器都成功执行事务操作的依据，因此，
 * PreCommit操作应该作为最后doCommit 操作过程中，资源服务器认为“别人都准备好提交了，我也准备好了”的信号，这样，在第三阶段，
 * 资源服务器不管是否收到doCommit指令，只要收到过 PreCommit指令，那么最后都应该提交本地事务，因为别人也成功提交的可能性很大，
 * 没有收到消息是因为网络问题或者协调服务器出现了故障。
 * 
 * 在本程序的设计中，在1阶段各个资源服务器执行完事务操作成后，如果执行成功将向协调服务器回复成功消息，但是此消息可能因为网络原因导致
 * 协调服务器无法收到，也可能因为协调服务器宕机而无法处理，这种情况下资源服务器在等待超时后都没有收到PreCommit指令，那么它应该回滚；
 * 如果因为网络原因，协调服务器没有收到某个资源服务器在1阶段处理的结果，那么协调服务器将向网络发出Abort终止指令；
 * 因此，只有在所有资源服务器都成功操作了事务并且协调服务器工作正常，网络正常，那么资源服务器才可能收到收到 PreCommit指令；
 * 但是，由于网络原因，协调服务器发出的PreCommit指令某个资源服务器无法收到，或者某个资源服务器宕机，这种可能性有，但是概率相当低，
 * 我们应该假设此资源服务器它是正常的，所以协调服务器在发出PreCommit指令之后，如果超过超时时间都没有收到某个资源服务器的响应，那么
 * 还是应该向其它资源服务器发出 doCommit提交指令.
 * 
 * 简单说，3PC与2PC的最大的区别，就是确保所有资源服务器都接收到了“提交”或者“预备提交”指令，这样所有资源服务器才能最大程度上的“同时”
 * 提交事务，确保事务的一致性。
 */

using PWMIS.DataProvider.Data;
using PWMIS.EnterpriseFramework.Common;
using PWMIS.EnterpriseFramework.Service.Basic;
using PWMIS.EnterpriseFramework.Service.Client;
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
    public class DTController
    {
        private static Dictionary<string, DTController> dictDTC = new Dictionary<string, DTController>();
        private static object sync_obj = new object();

        private int TransResourceCount = 0;
        private int PrearedOKCount = 0;
        private int PreCommitOKCount = 0;
        const int MAX_WAIT_TIME = 1000 * 60 * 5;

        private static void WriteLog(string format, params object[] args)
        {
            string logText = string.Format(format, args);

        }

        /// <summary>
        /// 区别一个分布式事务的标记
        /// </summary>
        public string TransIdentity { get; private set; }
        /// <summary>
        /// 当前分布式事务是否已经发出了终止指令
        /// </summary>
        public bool TransAbort { get; private set; }
        public DTController(string identity)
        {
            TransIdentity = identity;
            TransAbort = false;
        }

        public static DTController CheckStartController(string transIdentity)
        {
            DTController controller = null;
            lock (sync_obj)
            {
                if (dictDTC.ContainsKey(transIdentity))
                {
                    controller= dictDTC[transIdentity];
                }
                else
                {
                    controller = new DTController(transIdentity);
                    dictDTC.Add(transIdentity, controller);
                }
            }
            controller.AddResourceCount();
            return controller;
        }

        /// <summary>
        /// 移除一个事务控制器
        /// </summary>
        /// <param name="transIdentity"></param>
        /// <returns></returns>
        public static bool RemoveController(string transIdentity)
        {
            lock (sync_obj)
            {
                if (dictDTC.ContainsKey(transIdentity))
                {
                    return dictDTC.Remove(transIdentity);
                }
                return false;
            }
        }

        /// <summary>
        /// 累计事务资源服务器
        /// </summary>
        public void AddResourceCount()
        {
            System.Threading.Interlocked.Increment(ref TransResourceCount);
        }

        /// <summary>
        /// 获取分布式事务的状态，注意某个调用线程可能会被阻塞等待
        /// </summary>
        /// <param name="resourceState">资源服务器响应的状态</param>
        /// <returns></returns>
        public DistTrans3PCState GetDTCState(DistTrans3PCState resourceState)
        {
            if (resourceState == DistTrans3PCState.Rep_No_1PC)
            {
                //在1阶段，只要有一个资源服务器响应为不成功，则协调器将发出 终止指令。
                this.TransAbort = true;
                return DistTrans3PCState.Abort;
            }
            else if (resourceState == DistTrans3PCState.Rep_Yes_1PC)
            {
                //当前线程增加一个事务准备成功的计数器
                System.Threading.Interlocked.Increment(ref PrearedOKCount);
                int waiteTime = 0;
                int timeSpan = 10;//毫秒
                while (PrearedOKCount < TransResourceCount)
                {
                    //如果等待超时，则应该取消当前分布式事务
                    if (waiteTime >= MAX_WAIT_TIME)
                    {
                        this.TransAbort = true;
                        WriteLog("MSF DTC{0} 1PC waite timeout ({1} ms),transaction Abort.", this.TransIdentity,MAX_WAIT_TIME);                    
                    }
                    if (this.TransAbort)
                    {
                        return DistTrans3PCState.Abort;
                    }
                    System.Threading.Thread.Sleep(timeSpan);
                    waiteTime += timeSpan;
                }
                //如果每个事务资源服务器执行都成功，进入第二阶段：预提交阶段
                return DistTrans3PCState.PreCommit;
            }
            else if (resourceState == DistTrans3PCState.ACK_No_2PC)
            {
                return DistTrans3PCState.Completed;
            }
            else if (resourceState == DistTrans3PCState.ACK_Yes_2PC)
            { 
                //如果收到资源服务器预备提交的回复，得等待所有资源服务器确认回复此信息，然后发出提交指令
                //如果等待到超时，也没有等到预提交的全部回复，说明某个资源服务器或者网络出现问题，应该发出终止指令
                //注意：这里的等待，应该比资源服务器等待提交指令的时间要短
                //当前线程增加一个事务准备成功的计数器
                System.Threading.Interlocked.Increment(ref PreCommitOKCount);
                int waiteTime = 0;
                int timeSpan = 10;//毫秒
                while (PreCommitOKCount < TransResourceCount)
                {
                    //如果等待超时(10秒)，则应该取消当前分布式事务
                    if (waiteTime >= 10000)
                    {
                        this.TransAbort = true;
                        WriteLog("MSF DTC({0}) 2PC waite timeout ({1} ms),transaction Abort.", this.TransIdentity,waiteTime);     
                    }
                    
                    if (this.TransAbort)
                    {
                        return DistTrans3PCState.Abort;
                    }
                    System.Threading.Thread.Sleep(timeSpan);
                    waiteTime += timeSpan;
                }
                //如果每个事务资源服务器都已经回复，进入第三阶段：提交阶段
                return DistTrans3PCState.DoCommit;
            }
            else if (resourceState == DistTrans3PCState.Rep_No_3PC)
            {
                return DistTrans3PCState.Completed;
            }
            else if (resourceState == DistTrans3PCState.Rep_Yes_3PC)
            {
                return DistTrans3PCState.Completed;
            }
            else
            {
                return DistTrans3PCState.Completed;
            }
        }

        /// <summary>
        /// 3阶段分布式事务请求函数，执行完本地事务操作后，请求线程将继续工作，处理分布式提交的问题
        /// </summary>
        /// <typeparam name="T">本地事务操作函数的返回类型</typeparam>
        /// <param name="client">服务代理客户端</param>
        /// <param name="transIdentity">分布式事务的标识</param>
        /// <param name="dbHelper">数据访问对象</param>
        /// <param name="transFunction">事务操作函数</param>
        /// <returns>返回事务操作函数的结果</returns>
        public static T DistTrans3PCRequest<T>(Proxy client,string transIdentity, AdoHelper dbHelper, Func<AdoHelper,T> transFunction)
        {
            ServiceRequest request = new ServiceRequest();
            request.ServiceName = "DTCService";
            request.MethodName = "AttendTransaction";
            request.Parameters = new object[] { transIdentity };

            DistTrans3PCState currDTState = DistTrans3PCState.CanCommit;
            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            var tcs = new TaskCompletionSource<T>();
            dbHelper.BeginTransaction();

            DataType resultDataType = MessageConverter<T>.GetResponseDataType();
            client.ErrorMessage += client_ErrorMessage;
            client.RequestService<bool, DistTrans3PCState, DistTrans3PCState>(request.ServiceUrl, resultDataType,
                r=>
                {
                    WriteLog("MSF DTC({0}) Controller Process Reuslt:{1},Receive time:{2}", transIdentity, r,DateTime.Now);
                    client.Close();
                },
                s =>
                {
                    WriteLog("MSF DTC({0}) Resource at {1} receive DTC Controller state:{2}",transIdentity, DateTime.Now, s);
                    if (s == DistTrans3PCState.CanCommit)
                    {
                        try
                        {
                            T t= transFunction(dbHelper);
                            currDTState = DistTrans3PCState.Rep_Yes_1PC;
                            tcs.SetResult(t);
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.Message);
                            currDTState = DistTrans3PCState.Rep_No_1PC;
                            tcs.SetException(ex);
                        }
                        //警告：如果自此之后，很长时间没有收到协调服务器的任何回复，本地应回滚事务
                        new Task(() =>
                        {
                            DateTime currOptTime = DateTime.Now;
                            WriteLog("MSF DTC({0}) 1PC,Child moniter task has started at time:{1}",transIdentity, currOptTime);

                            while (currDTState != DistTrans3PCState.Completed)
                            {
                                System.Threading.Thread.Sleep(10);
                                if (currDTState != DistTrans3PCState.Rep_Yes_1PC && currDTState != DistTrans3PCState.Rep_No_1PC)
                                {
                                    WriteLog("MSF DTC({0}) 1PC,Child moniter task find DistTrans3PCState has changed,Now is {1},task break!",transIdentity, currDTState);
                                    break;
                                }
                                else
                                {
                                    //在1阶段回复消息后，超过一分钟，资源服务器没有收到协调服务器的任何响应，回滚本地事务
                                    if (DateTime.Now.Subtract(currOptTime).TotalSeconds > 60)
                                    {
                                        try
                                        {
                                            dbHelper.Rollback();
                                        }
                                        catch
                                        {

                                        }
                                        client.Close();
                                        WriteLog("MSF DTC({0}) 1PC,Child moniter task check Opreation timeout,Rollback Transaction,task break!", transIdentity);
                                        break;
                                    }
                                }
                            }

                        }, TaskCreationOptions.None).Start();

                        return currDTState;
                    }
                    else if (s == DistTrans3PCState.PreCommit)
                    {
                        currDTState = DistTrans3PCState.ACK_Yes_2PC;
                        //警告：如果自此之后，很长时间没有收到协调服务器的任何回复，本地应提交事务
                        new Task(() =>
                        {
                            DateTime currOptTime = DateTime.Now;
                            WriteLog("MSF DTC({0}) 2PC,Child moniter task has started at time:{1}",transIdentity, currOptTime);

                            while (currDTState != DistTrans3PCState.Completed)
                            {
                                System.Threading.Thread.Sleep(10);
                                if (currDTState != DistTrans3PCState.ACK_Yes_2PC)
                                {
                                    WriteLog("MSF DTC({0}) 2PC,Child moniter task find DistTrans3PCState has changed,Now is {1},task break!", transIdentity,currDTState);
                                    break;
                                }
                                else
                                {
                                    //在2阶段，超过30秒，资源服务器没有收到协调服务器的任何响应，提交本地事务
                                    if (DateTime.Now.Subtract(currOptTime).TotalSeconds > 30)
                                    {
                                        try
                                        {
                                            dbHelper.Commit();
                                        }
                                        catch
                                        {

                                        }
                                        client.Close();
                                        WriteLog("MSF DTC({0}) 2PC,Child moniter task check Opreation timeout,Commit Transaction,task break!", transIdentity);
                                        break;
                                    }
                                }
                            }
                        }, TaskCreationOptions.None).Start();

                        return currDTState;

                    }
                    else if (s == DistTrans3PCState.Abort)
                    {
                        try
                        {
                            dbHelper.Rollback();
                        }
                        catch
                        {

                        }
                        currDTState = DistTrans3PCState.ACK_No_2PC;
                        return currDTState;
                    }
                    else if (s == DistTrans3PCState.DoCommit)
                    {
                        try
                        {
                            dbHelper.Commit();
                            currDTState = DistTrans3PCState.Rep_Yes_3PC;
                        }
                        catch
                        {
                            currDTState = DistTrans3PCState.Rep_No_3PC;
                        }
                        return currDTState;
                    }
                    else
                    {
                        //其它参数，原样返回
                        currDTState = s;
                        return s;
                    }
                });

          

            try
            {
                tcs.Task.Wait();
                return tcs.Task.Result;
            }
            catch (Exception ex)
            {
                WriteLog("Task Error:{0}", ex.Message);
                WriteLog("Try Rollback..");
                try
                {
                    dbHelper.Rollback();
                    WriteLog("Try Rollback..OK!");
                }
                catch (Exception ex1)
                {
                    WriteLog("Try Rollback..Error:{0}", ex1.Message);
                }
            }

            return default(T);
        }

        static void client_ErrorMessage(object sender, MessageSubscriber.MessageEventArgs e)
        {
            WriteLog("MSF DTC Service Proxy Error:" + e.MessageText);
        }

    }

    /// <summary>
    /// 分布式事务协调器服务，基于3PC过程。
    /// </summary>
    public class DTCService:ServiceBase
    {
        private int TransactionResourceCount;
        private DistTrans3PCState CurrentDTCState;

        //private static System.Collections.Concurrent.ConcurrentBag<DistTransInfo> DTResourceList = new System.Collections.Concurrent.ConcurrentBag<DistTransInfo>();

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

        /// <summary>
        /// 参加指定标识的分布式事务，直到事务执行完成。一个分布式事务包含若干本地事务
        /// </summary>
        /// <param name="identity">标识一个分布式事务</param>
        /// <returns></returns>
        public bool AttendTransaction(string identity)
        {
            DistTransInfo info = new DistTransInfo();
            info.ClientIdentity = base.CurrentContext.Request.ClientIdentity;
            info.CurrentDTCState = DistTrans3PCState.CanCommit;
            info.LastStateTime = DateTime.Now;
            info.TransIdentity = identity;
            //DTResourceList.Add(info);
            //获取一个当前事务标识的协调器线程
            DTController controller = DTController.CheckStartController(identity);

            CurrentDTCState = DistTrans3PCState.CanCommit;
            while (CurrentDTCState != DistTrans3PCState.Completed)
            {
                //获取资源服务器的事务状态，资源服务器可能自身或者因为网络情况出错
                try
                {
                    info.CurrentDTCState = base.CurrentContext.CallBackFunction<DistTrans3PCState, DistTrans3PCState>(CurrentDTCState);
                    info.LastStateTime = DateTime.Now;
                    CurrentDTCState = controller.GetDTCState(info.CurrentDTCState);
                    Console.WriteLine("Callback Message:{0}", CurrentDTCState);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DTCService Callback Error:{0}", ex.Message);
                    DTController.RemoveController(identity);
                    return false;
                }
            }
            DTController.RemoveController(identity);
            return true;
        }

        


        public override bool ProcessRequest(IServiceContext context)
        {
            return base.ProcessRequest(context);
        }
    }
}
