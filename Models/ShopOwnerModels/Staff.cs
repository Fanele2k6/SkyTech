using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyTech.Models
{
    public class Staff
    {
        [Key]
        public int StaffId { get; set; }

        public int OwnerId { get; set; }
        public virtual ShopOwner Owner { get; set; }

        [Required, MaxLength(100)] public string FirstName { get; set; }
        [Required, MaxLength(100)] public string LastName { get; set; }

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(10), Phone]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@".{8,}")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [NotMapped]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string UserType { get; set; }
        public DateTime RegisteredOn { get; set; }
    }
}