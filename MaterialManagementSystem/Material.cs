
using System;

namespace MaterialManagementSystem
{
    public class Material
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MaterialType { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
        public int PackageQuantity { get; set; }
        public int MinQuantity { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
