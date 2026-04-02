using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Bikbulatov_save
{
    public partial class Agent : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

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
        public int Priority { get; set; }

        private string _logo;
        public string Logo
        {
            get => _logo;
            set
            {
                _logo = value;
                OnPropertyChanged(nameof(Logo));
            }
        }

        public int SaleForYear
        {
            get
            {
                int sales = 0;
                foreach (ProductSale productSale in ProductSale)
                {
                    // Раскомментировать для учёта только за последний год
                    // TimeSpan diff = DateTime.Today - productSale.SaleDate;
                    // if (diff.TotalDays <= 365)
                    sales += productSale.ProductCount;
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
                if (this.Sales >= 500000) return 25;
                if (this.Sales >= 150000) return 20;
                if (this.Sales >= 50000) return 10;
                if (this.Sales >= 10000) return 5;
                return 0;
            }
        }

        public string FonStyle
        {
            get
            {
                if (Discount >= 25) return "LightGreen";
                else return "white";
            }
        }

        public string AgentTypeName => AgentType?.Title ?? "Не указан";

        public string AgentPhotoPath
        {
            get
            {
                if (string.IsNullOrEmpty(Logo)) return null;
                string fileName = Logo;
                int lastSlash = fileName.LastIndexOfAny(new[] { '\\', '/' });
                if (lastSlash >= 0) fileName = fileName.Substring(lastSlash + 1);
                fileName = fileName.Trim().TrimStart('\\', '/', '"').TrimEnd('"');
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agents", fileName);
                return File.Exists(fullPath) ? fullPath : null;
            }
        }

        public virtual AgentType AgentType { get; set; }
        public virtual ICollection<AgentPriorityHistory> AgentPriorityHistory { get; set; }
        public virtual ICollection<ProductSale> ProductSale { get; set; }
        public virtual ICollection<Shop> Shop { get; set; }
    }
}