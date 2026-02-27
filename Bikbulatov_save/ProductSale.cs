
namespace Bikbulatov_save
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductSale
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public int AgentID { get; set; }
        public System.DateTime SaleDate { get; set; }
        public int ProductCount { get; set; }

        public virtual Agent Agent { get; set; }
        public virtual Product Product { get; set; }
        public decimal Stoimost
        {
            get
            {
                decimal s;
                s = ProductCount * Product.MinCostForAgent;
                return s;
            }
        }
    }
}
