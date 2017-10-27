MSF-DistTransExample

MSF 分布式事务示例，基于3阶段提交的事务 
============================================

! 框架的基本了解：
MSF是消息服务框架的简称，它是一个消息推送和服务处理框架，详细内容请参考：
“一切都是消息”--MSF（消息服务框架）入门简介 
http://www.cnblogs.com/bluedoctor/p/7605737.html

当前示例的数据库访问层框架，使用的是SOD框架，有关SOD框架的入门简介，请参考：
http://pwmis.codeplex.com
http://www.pwmis.com/sqlmap

! 如何运行本示例解决方案：
1，请设置解决方案为多项目启动，启动 DistTransClient和 TistTransApp
2，如果你没有安装 SQLSERVER 2016/2017 LocalDB,请下载安装，或者修改下面的配置文件：
   PdfNetEF.MessageServiceHost.exe.config
   找到文件内的连接配置，注释其它的，使用你可以使用的连接配置。
3，运行解决方案，根据 TistTransApp 程序的提示，启动下面3个服务：
   # 事务控制器服务
   # 商品服务
   # 订单服务

4，如果你需要调试，根据TistTransApp 程序的提示的各个服务的进程ID，用VS附加调试即可。

! 分布式事务三阶段提交协议的实习
有关3PC的详细理论，请参考网上资料，很多，这里不做详细介绍了，下面说说本示例解决方案对3PC 分布式事务控制的特点：
1，采用MSF的长连接，连接如果有异常可以马上检测到而不用超时等待；
2，对多种异常进行检测，完整实现了3PC理论讨论中的各种异常处理；
3，重点加强了预提交(PreCommit)的处理策略;
4，详细的事务处理日志记录。

bluedoctor, 2017.10.27


