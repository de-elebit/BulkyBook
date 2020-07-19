using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BulkyBook.Models
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }
        
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Range(1, 10, ErrorMessage = "올바른 갯수를 입력해주세요.")]
        public int Count { get; set; }

        [NotMapped]
        public double Price { get; set; }

        public ShoppingCart()
        {
            Count = 1;
        }
    }
}
