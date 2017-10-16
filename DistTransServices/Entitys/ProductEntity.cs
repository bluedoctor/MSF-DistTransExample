using DistTransCommon.Interface;
using PWMIS.DataMap.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransServices.Entitys
{
    public class ProductEntity:EntityBase, IProduct
    {
        public ProductEntity()
        {
            TableName = "Products";
            PrimaryKeys.Add("ProductID");
            IdentityName = "ProductID";
        }

        public int ID 
        {
            get { return getProperty<int>("ProductID"); }
            set { setProperty("ProductID", value); }
        }

        public string ProductName
        {
            get { return getProperty<string>("ProductName"); }
            set { setProperty("ProductName", value,50); }
        }

        public float Price
        {
            get { return getProperty<float>("Price"); }
            set { setProperty("Price", value); }
        }

        public int Onhand
        {
            get { return getProperty<int>("Onhand"); }
            set { setProperty("Onhand", value); }
        }
    }
}
