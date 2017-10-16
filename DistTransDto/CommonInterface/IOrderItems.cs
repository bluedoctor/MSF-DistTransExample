using System;
namespace DistTransCommon.Interface
{
    public interface IOrderItems
    {
        int BuyNumber { get; set; }
        int ID { get; set; }
        float OnePrice { get; set; }
        int OrderID { get; set; }
        int ProductID { get; set; }
        string ProductName { get; set; }
    }
}
