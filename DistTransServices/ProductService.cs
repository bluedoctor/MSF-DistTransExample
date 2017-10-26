using DistTransDto;
using DistTransServices.DbContexts;
using DistTransServices.Entitys;
using PWMIS.Core.Extensions;
using PWMIS.DataMap.Entity;
using PWMIS.EnterpriseFramework.Service.Client;
using PWMIS.EnterpriseFramework.Service.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransServices
{
    /// <summary>
    /// 商品服务
    /// </summary>
    public class ProductService:ServiceBase
    {
          //Proxy orderProxy;
          Proxy DTS_Proxy;
          public ProductService()
        {
            //orderProxy = new Proxy();
            //orderProxy.ServiceBaseUri = System.Configuration.ConfigurationManager.AppSettings["OrderUri"];

            DTS_Proxy = new Proxy();
            DTS_Proxy.ServiceBaseUri = System.Configuration.ConfigurationManager.AppSettings["MSF_DTS_Uri"];
        }

       /// <summary>
       /// 获取商品信息
       /// </summary>
       /// <param name="productId"></param>
       /// <returns></returns>
        public ProductDto GetProductInfo(int productId)
        {
            Console.WriteLine("---------2,--Session ID:{0}----------", base.CurrentContext.Session.SessionID);
            /*
             * 采用NOLOCK 方式查询
            ProductDbContext context = new ProductDbContext();
            ProductEntity entity= OQL.From<ProductEntity>().With(OQL.SqlServerLock.NOLOCK )
                .Select()
                .Where((cmp, p) => cmp.Comparer(p.ID,"=",productId))
                .END
                .ToObject(context.CurrentDataBase);
            */
            //下面采用更新库存的事务连接对象，不需要NoLock
            ProductDbContext context = base.CurrentContext.Session.Get<ProductDbContext>("DbContext");
            ProductEntity entity = OQL.From<ProductEntity>()
              .Select()
              .Where((cmp, p) => cmp.Comparer(p.ID, "=", productId))
              .END
              .ToObject(context.CurrentDataBase);

            ProductDto dto = new ProductDto();
            if (entity != null)
            {
                //entity.MapToPOCO(dto);
                entity.CopyTo<ProductDto>(dto);
            }
            return dto;
        }

        /// <summary>
        /// 更新商品库存，并返回商品售卖简要信息
        /// </summary>
        /// <param name="transIdentity">分布式事务标识</param>
        /// <param name="buyItems">购买的商品精简信息</param>
        /// <returns></returns>
        public List<SellProductDto> UpdateProductOnhand(string transIdentity, IEnumerable<BuyProductDto> buyItems)
        {
            ProductDbContext context = new ProductDbContext();
            DTController controller = new DTController(transIdentity);
            return controller.DistTrans3PCRequest<List<SellProductDto>>(DTS_Proxy,
                context.CurrentDataBase,
                c =>
                {
                    return InnerUpdateProductOnhand(context,buyItems);
                });
           
        }



        private List<SellProductDto> InnerUpdateProductOnhand(ProductDbContext context, IEnumerable<BuyProductDto> buyItems)
        {
            List<SellProductDto> result = new List<SellProductDto>();

            foreach (BuyProductDto item in buyItems)
            {
                ProductEntity entity = new ProductEntity()
                {
                    ID = item.ProductId,
                    Onhand= item.BuyNumber
                };
                OQL q = OQL.From(entity)
                    .UpdateSelf('-', entity.Onhand)
                    .Where(cmp => cmp.EqualValue(entity.ID) & cmp.Comparer(entity.Onhand, ">=", item.BuyNumber))
                    .END;


                int count = context.ProductQuery.ExecuteOql(q);
                SellProductDto sell = new SellProductDto();
                sell.BuyNumber = item.BuyNumber;
                sell.ProductId = item.ProductId;
                //修改库存成功，才能得到发货地
                if (count > 0)
                    sell.StoreHouse = this.GetStoreHouse(item.ProductId);
                result.Add(sell);
            }
            base.CurrentContext.Session.Set<ProductDbContext>("DbContext", context);
            Console.WriteLine("----------1,-Session ID:{0}----------", base.CurrentContext.Session.SessionID);
            return result;
        }

        /// <summary>
        /// 模拟根据商品获取商品对应的发货地
        /// </summary>
        /// <param name="productId">商品标识</param>
        /// <returns></returns>
        private string GetStoreHouse(int productId)
        {
            string[] city = new string[] {"北京","上海","广州","深圳","天津","重庆","杭州","南京","武汉","成都" };
            int index = new Random().Next(10);
            return city[index];
        }

        public override bool ProcessRequest(IServiceContext context)
        {
            context.SessionRequired = true;
            //客户端（订单服务）将使用事务标识作为连接的 RegisterData，因此采用这种会话模式
            context.SessionModel = SessionModel.RegisterData;
            return base.ProcessRequest(context);
        }
    }
}
