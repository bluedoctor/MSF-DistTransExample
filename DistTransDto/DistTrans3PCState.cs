using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransServices
{
    /// <summary>
    /// 分布式事务3阶段提交状态枚举
    /// </summary>
    public enum DistTrans3PCState
    {
        /// <summary>
        /// 协调服务器：1阶段，执行事务，准备提交
        /// </summary>
        CanCommit,
        /// <summary>
        /// 资源服务器：1阶段，执行事务操作完成，可以提交（然后进入第2阶段）
        /// </summary>
        Rep_Yes_1PC,
        /// <summary>
        /// 资源服务器：1阶段，执行事务操作遇到错误，不能提交（然后进入第2阶段）
        /// </summary>
        Rep_No_1PC,
        /// <summary>
        /// 协调服务器：2阶段，预备提交
        /// </summary>
        PreCommit,
        /// <summary>
        /// 资源服务器：2阶段，确认操作（预备提交），等待DoCommit指令
        /// </summary>
        ACK_Yes_2PC,
        /// <summary>
        ///  协调服务器：2阶段，终止提交。如果在1阶段有某个资源服务器一直没有响应，协调服务器将发出此指令。
        ///  如果资源服务器已经进入2阶段但一直没有收到协调服务器的PreCommit指令，将默认执行此Abort指令
        /// </summary>
        Abort,
        /// <summary>
        /// 资源服务器：2阶段，确认操作（终止提交），资源服务器将立即执行回滚操作然后再向协调服务器回复此信息
        /// </summary>
        ACK_No_2PC,
        /// <summary>
        /// 协调服务器：3阶段，执行正式提交
        /// </summary>
        DoCommit,
        /// <summary>
        /// 资源服务器：3阶段，提交操作成功
        /// </summary>
        Rep_Yes_3PC,
        /// <summary>
        /// 资源服务器：3阶段，提交操作失败
        /// </summary>
        Rep_No_3PC,
        /// <summary>
        /// 事务协调器：事务操作结束
        /// </summary>
        Completed 
    }
}
