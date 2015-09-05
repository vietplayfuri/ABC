namespace ABC.Model
{
    using Utility;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    //TODO: name of real table, not entity in code
    [Table("OrderDetails")]
    public partial class OrderDetails
    {
        [Key]
        [ForeignKey("Orders")]                  //TODO: name of real table, not entity in code
        public int OrderID { get; set; }

        [Key]
        [ForeignKey("Products")]                //TODO: name of real table, not entity in code
        public int ProductID { get; set; }

        [Column(TypeName = "money")]
        public decimal UnitPrice { get; set; }

        public short Quantity { get; set; }

        public float Discount { get; set; }

        [External]
        public virtual Orders Orders { get; set; }
        [External]
        public virtual Products Products { get; set; }
    }
}
