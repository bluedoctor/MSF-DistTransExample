using DistTransCommon.Interface;
using PWMIS.DataMap.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransServices.Entitys
{
    class OrderItemEntity:EntityBase, IOrderItems
    {
        public OrderItemEntity()
        {
            TableName = "OrderItems";
            PrimaryKeys.Add("ID");
            IdentityName = "ID";
        }

        public int ID
        {
            get { return getProperty<int>("ID"); }
            set { setProperty("ID", value); }
        }

        public int OrderID
        {
            get { return getProperty<int>("OerderID"); }
            set { setProperty("OerderID", value); }
        }

        public int ProductID
        {
            get { return getProperty<int>("ProductID"); }
            set { setProperty("ProductID", value); }
        }

        public string ProductName
        {
            get { return getProperty<string>("ProductName"); }
            set { setProperty("ProductName", value, 50); }
        }

        public float OnePrice
        {
            get { return getProperty<float>("Price"); }
            set { setProperty("Price", value); }
        }

        public int BuyNumber
        {
            get { return getProperty<int>("BuyNumber"); }
            set { setProperty("BuyNumber", value); }
        }

        /// <summary>
        /// 发货地点
        /// </summary>
        public string StoreHouse
        {
            get { return getProperty<string>("StoreHouse"); }
            set { setProperty("StoreHouse", value, 50); }
        }
    }
}
