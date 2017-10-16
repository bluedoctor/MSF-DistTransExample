using DistTransServices.Entitys;
using PWMIS.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransServices.DbContexts
{
    public class OrderDbContext:DbContext
    {
        public OrderDbContext()
            : base("OrdersDb")
        { 
        
        }

        protected override bool CheckAllTableExists()
        {
            base.CheckTableExists<OrderEntity>();
            base.CheckTableExists<OrderItemEntity>();
            return true;
        }
    }
}
