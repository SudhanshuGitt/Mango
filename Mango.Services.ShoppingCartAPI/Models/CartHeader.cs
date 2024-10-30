using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.ShoppingCartAPI.Models
{
    public class CartHeader
    {
        [Key]
        public int CartHeaderId { get; set; }
        public string? UserId { get; set; }
        public string? CouponCode { get; set; }
        // we don't wan to store them in db
        [NotMapped]
        public double Discount { get; set; }
        [NotMapped]
        public double CartTotal { get; set; }

    }
}
