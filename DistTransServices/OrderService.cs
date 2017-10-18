using DistTransDto;
using DistTransServices.DbContexts;
using DistTransServices.Entitys;
using PWMIS.Core.Extensions;
using PWMIS.DataProvider.Data;
using PWMIS.EnterpriseFramework.Common;
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
        public bool CreateOrder(int orderId,int userId,IEnumerable<BuyProductDto> buyItems)
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

            return DTController.DistTrans3PCRequest<bool>(productProxy, 
                context.CurrentDataBase,
                request,
                c =>
                {
                    context.Add<OrderEntity>(order);
                    context.AddList<OrderItemEntity>(orderItems);
                });
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
