using System.Collections.Generic;

namespace PCPartsWeb.Models
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public List<Review> Reviews { get; set; }
        public List<Product> RelatedProducts { get; set; }
    }
}