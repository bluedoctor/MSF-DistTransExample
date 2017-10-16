using DistTransCommon.Interface;
using PWMIS.DataMap.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransServices.Entitys
{
    public class OrderEntity:EntityBase, IOrders
    {
        public OrderEntity()
        {
            TableName = "Orders";
            PrimaryKeys.Add("OerderID");
          
        }

        public int ID
        {
            get { return getProperty<int>("OerderID"); }
            set { setProperty("OerderID", value); }
        }

        public string OrderName
        {
            get { return getProperty<string>("OrderName"); }
            set { setProperty("OrderName", value, 100); }
        }

        public float AmountPrice
        {
            get { return getProperty<float>("AmountPrice"); }
            set { setProperty("AmountPrice", value); }
        }

        public int OwnerID
        {
            get { return getProperty<int>("OwnerID"); }
            set { setProperty("OwnerID", value); }
        }

        public DateTime OrderTime
        {
            get { return getProperty<DateTime>("OrderTime"); }
            set { setProperty("OrderTime", value); }
        }
    }
}
