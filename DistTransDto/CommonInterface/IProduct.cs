using System;
namespace DistTransCommon.Interface
{
   public  interface IProduct
    {
        int ID { get; set; }
        int Onhand { get; set; }
        float Price { get; set; }
        string ProductName { get; set; }
    }
}
