using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CM_Project.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        //public required virtual Customer Customers { get; set; }   === IT Enforces that Customers must always be set when creating an object
        //OR  public virtual Customer? Customers { get; set; } == tells the compiler that its okay for the property to start out as null

        public virtual Customer Customers { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsCheckedOut { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}