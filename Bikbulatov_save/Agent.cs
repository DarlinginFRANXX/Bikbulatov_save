namespace Bikbulatov_save
{
    using System;
    using System.Collections.Generic;

    public partial class Agent
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Agent()
        {
            this.AgentPriorityHistory = new HashSet<AgentPriorityHistory>();
            this.ProductSale = new HashSet<ProductSale>();
            this.Shop = new HashSet<Shop>();
        }

        public int ID { get; set; }
        public string Title { get; set; }
        public int AgentTypeID { get; set; }
        public string Address { get; set; }
        public string INN { get; set; }
        public string KPP { get; set; }
        public string DirectorName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Logo { get; set; }
        public int Priority { get; set; }

        public int SaleForYear
        {
            get
            {
                int sales = 0;
                foreach (ProductSale productSale in ProductSale)
                {
                    TimeSpan diffrenceWithoutTime = DateTime.Today.Date - productSale.SaleDate.Date;
                    //if ((int)diffrenceWithoutTime.TotalDays <= 365) 
                    { 
                        sales += productSale.ProductCount; 
                    }
                }
                return sales;
            }
        }
        public decimal Sales
        {
            get
            {
                decimal sales = 0;
                foreach (ProductSale productSale in ProductSale)
                {
                    sales += productSale.Stoimost;
                }
                return sales;
            }
        }
        public int Discount
        {
            get
            {
                if (this.Sales >= 500000)
                    return 25;
                if (this.Sales >= 150000)
                    return 20;
                if (this.Sales >= 50000)
                    return 10;
                if (this.Sales >= 10000)
                    return 5;
                return 0;
            }
        }

        public string FonStyle
        {
            get
            {
                if (Discount >= 25)
                    return "LightGreen";
                else
                    return "white";
            }
        }
        public virtual AgentType AgentType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AgentPriorityHistory> AgentPriorityHistory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductSale> ProductSale { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Shop> Shop { get; set; }
        public object DataTime { get; private set; }
    }
}