using DistTransDto;
using DistTransServices.DbContexts;
using DistTransServices.Entitys;
using PWMIS.DataMap.Entity;
using PWMIS.EnterpriseFramework.Service.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransServices
{
    public class ProductService:ServiceBase
    {

        public ProductDto GetProductInfo(int productId)
        {
            ProductEntity entity= OQL.From<ProductEntity>()
                .Select()
                .Where((cmp, p) => cmp.Comparer(p.ID,"=",productId))
                .END
                .ToObject();
            ProductDto dto = new ProductDto();
            if (entity != null)
            {
                entity.MapToPOCO(dto);
            }
            return dto;
        }

        /// <summary>
        /// 更新商品库存，并返回商品售卖简要信息
        /// </summary>
        /// <param name="buyItems">购买的商品精简信息</param>
        /// <returns></returns>
        public List<SellProductDto> UpdateProductOnhand(IEnumerable<BuyProductDto> buyItems)
        {
            //这里调用分布式服务 DTCService.AttendTransaction
            ProductDbContext context = new ProductDbContext();
            List<SellProductDto> result = new List<SellProductDto>();

            foreach (BuyProductDto item in buyItems)
            {
                ProductEntity entity = new ProductEntity() { 
                 ID= item.ProductId,
                 // Onhand= item.BuyNumber
                };
                OQL q = OQL.From(entity)
                    .UpdateSelf('-', entity.Onhand)
                    .Where(cmp=>cmp.EqualValue(entity.ID) & cmp.Comparer(entity.Onhand,">=",item.BuyNumber))
                    .END;

              
                int count= context.ProductQuery.ExecuteOql(q);
                //if (count <= 0)
                //    return false;
            }

            return result;
        }
    }
}
