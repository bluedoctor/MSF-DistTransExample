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
            : base("ProductDb")
        { 
        
        }

        protected override bool CheckAllTableExists()
        {
            base.CheckTableExists<ProductEntity>();
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
