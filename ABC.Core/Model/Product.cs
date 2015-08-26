namespace ABC.Model
{
    using Core;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Products")]
    public partial class Products
    {
        public Products()
        {
            OrderDetails = new HashSet<OrderDetails>();
        }

        [PrimaryKey]
        public int ProductID { get; set; }

        public string ProductName { get; set; }

        public int? SupplierID { get; set; }

        [ForeignKey("Categories")] //TODO: name of real table, not entity in code
        public int? CategoryID { get; set; }

        public string QuantityPerUnit { get; set; }

        public decimal? UnitPrice { get; set; }

        public short? UnitsInStock { get; set; }

        public short? UnitsOnOrder { get; set; }

        public short? ReorderLevel { get; set; }

        public bool Discontinued { get; set; }

        [External] //TODO: must have
        public virtual Category Category { get; set; }

        [External] //TODO: must have
        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
    }
}
