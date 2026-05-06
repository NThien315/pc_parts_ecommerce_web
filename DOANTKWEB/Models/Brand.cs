using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DOANTKWEB.Models
{
    [Table("Brands")]
    public class Brand
    {
        [Key]
        public int BrandID { get; set; }

        [Required]
        [StringLength(100)]
        public string BrandName { get; set; } 

        [StringLength(100)]
        public string Origin { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}