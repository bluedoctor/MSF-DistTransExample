using DistTransDto;
using DistTransServices.DbContexts;
using DistTransServices.Entitys;
using PWMIS.Core.Extensions;
using PWMIS.EnterpriseFramework.Service.Basic;
using PWMIS.EnterpriseFramework.Service.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransServices
{
    public class OrderService
    {
        Proxy productProxy;
        public OrderService()
        {
            productProxy = new Proxy();
            productProxy.ServiceBaseUri = System.Configuration.ConfigurationManager.AppSettings["ProductUri"];
        }
        public async Task<bool> CreateOrder(int orderId,int userId,IEnumerable<BuyProductDto> buyItems)
        {
            //构造订单明细和订单对象
            List<OrderItemEntity> orderItems = new List<OrderItemEntity>();
            OrderEntity order = new OrderEntity()
            {
                ID = orderId,
                OwnerID = userId,
                OrderTime = DateTime.Now,
                OrderName="Prudoct:"
            };
            foreach (BuyProductDto item in buyItems)
            {
                ProductDto product = this.GetProductInfo(item.ProductId).Result;
              
                OrderItemEntity temp = new OrderItemEntity()
                {
                     OrderID = orderId,
                      ProductID= product.ID,
                       BuyNumber= item.BuyNumber,
                        OnePrice= product.Price,
                         ProductName= product.ProductName
                };
                
                orderItems.Add(temp);
                order.OrderName += "," + temp.ProductName;
                order.AmountPrice += temp.OnePrice * temp.BuyNumber;
            }
            //保存订单到数据库
            //使用3阶段提交的分布式事务
            OrderDbContext context = new OrderDbContext();
            ServiceRequest request = new ServiceRequest();
            request.ServiceName = "ProductService";
            request.MethodName = "UpdateProductOnhand";
            request.Parameters = new object[] { buyItems };

            return await DistTrans3PCRequestAsync<bool>(context,
                request,
                c =>
                {
                    c.Add<OrderEntity>(order);
                    c.AddList<OrderItemEntity>(orderItems);
                });
        }

        public Task<T> DistTrans3PCRequestAsync<T>(DbContext context, ServiceRequest request,Action<DbContext> transactionAction)
        {
            var tcs = new TaskCompletionSource<T>();
            context.CurrentDataBase.BeginTransaction();

            productProxy.RequestService<T, DistTrans3PCState, DistTrans3PCState>(request.ToString(),
                PWMIS.EnterpriseFramework.Common.DataType.Text,
                b =>
                {
                    productProxy.Close();
                    tcs.SetResult(b);
                },
                s =>
                {
                    if (s == DistTrans3PCState.CanCommit)
                    {
                        try
                        {
                            transactionAction(context);
                            return DistTrans3PCState.Rep_Yes_1PC;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            return DistTrans3PCState.Rep_No_1PC;
                        }
                    }
                    else if (s == DistTrans3PCState.PreCommit)
                    {
                        return DistTrans3PCState.ACK_Yes_2PC;
                    }
                    else if (s == DistTrans3PCState.Abort)
                    {
                        try
                        {
                            context.CurrentDataBase.Rollback();
                        }
                        catch
                        {

                        }
                        return DistTrans3PCState.ACK_No_2PC;
                    }
                    else if (s == DistTrans3PCState.DoCommit)
                    {
                        try
                        {
                            context.CurrentDataBase.Commit();
                            return DistTrans3PCState.Rep_Yes_3PC;
                        }
                        catch
                        {
                            return DistTrans3PCState.Rep_No_3PC;
                        }
                    }
                    else
                    {
                        //其它参数，原样返回
                        return s;
                    }
                });
            return tcs.Task;
        }

        private async Task<ProductDto> GetProductInfo(int productId)
        {
            ServiceRequest request = new ServiceRequest();
            request.ServiceName = "ProductService";
            request.MethodName = "GetProductInfo";
            request.Parameters = new object[] { productId};
            return await productProxy.RequestServiceAsync<ProductDto>(request);
            
        }

        private async Task<bool> UpdateProductOnhand(IEnumerable<BuyProductDto> buyItems)
        {
            ServiceRequest request = new ServiceRequest();
            request.ServiceName = "ProductService";
            request.MethodName = "UpdateProductOnhand";
            request.Parameters = new object[] { buyItems };
            return await productProxy.RequestServiceAsync<bool>(request);
        }
    }
}
