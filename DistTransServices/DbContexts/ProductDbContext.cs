using DistTransServices.Entitys;
using PWMIS.Core.Extensions;
using PWMIS.DataMap.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransServices.DbContexts
{
    public class ProductDbContext:DbContext
    {
        public ProductDbContext()
            : base("ProductsDb")
        { 
        
        }

        protected override bool CheckAllTableExists()
        {
            if (!base.CheckTableExists<ProductEntity>())
            {
                //如果是新建的表，初始化商品表的测试数据
                Console.WriteLine("初始化商品信息表...");
                List<ProductEntity> list = new List<ProductEntity>();
                for (int i = 0; i < 10; i++)
                {
                    ProductEntity product = new ProductEntity();
                    product.ProductName = "商品" + i;
                    product.Price = 10 + i;
                    product.Onhand = 100 + i;

                    list.Add(product);
                }
                EntityQuery<ProductEntity> query = new EntityQuery<ProductEntity>(this.CurrentDataBase);
                query.QuickInsert(list);
                Console.WriteLine("初始化商品信息表,完成");
            }
            return true;
        }

        public EntityQuery<ProductEntity> ProductQuery
        {
            get {
                return new EntityQuery<ProductEntity>(this.CurrentDataBase);
            }
        }
    }
}
