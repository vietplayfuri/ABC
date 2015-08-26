namespace ABC.Model
{
    using Core;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Categories")]
    public partial class Category
    {
        public Category()
        {
            Products = new HashSet<Products>();
        }

        [PrimaryKey] //TODO: must have
        public int CategoryID { get; set; }
        
        public string CategoryName { get; set; }
                
        public string Description { get; set; }
                
        public byte[] Picture { get; set; }

        [External] //TODO: must have
        public virtual ICollection<Products> Products { get; set; }
    }
}
