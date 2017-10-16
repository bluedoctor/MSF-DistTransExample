using DistTransCommon.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistTransDto
{
    public class ProductDto : IProduct
    {
        public int ID
        {
            get;
            set;
        }

        public int Onhand
        {
            get;
            set;
        }

        public float Price
        {
            get;
            set;
        }

        public string ProductName
        {
            get;
            set;
        }
    }
}
