using CM_Project.Models;
using CM_Project.Models.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CM_Project.ViewModels
{
    public enum SelectType
    {
        Informal,
        Formal
    }

    public class LogInViewModel
    {
        [Required(ErrorMessage = "Please enter your username")]
        [EmailAddress(ErrorMessage ="Please enter a valid email address!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
    public class CustomerViewModel
    {

        [Required, MaxLength(100)]
        //[DisplayName("First Name/s")]

        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        //[DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required, EmailAddress]
        //[DisplayName("Email Address")]
        [StringLength(255)]
        public string Email { get; set; }


        [Required, StringLength(10)]
        //[DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        [MaxLength(200)]
        //[DisplayName("Home Address")]
        public string Address { get; set; }

        // Hashed password stored

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage ="Password must be atleast 8 characters long")]
        [RegularExpression(@".{8,}")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [NotMapped]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

    }

    public class ShopOwnerViewModel
    {
        [Required, MaxLength(50)] public string Names { get; set; }
        [Required, MaxLength(50)] public string Surname { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required, MaxLength(200)] public string ResidentialAddress { get; set; }
        [Required, StringLength(13, MinimumLength = 13)] public string IdBookNumber { get; set; }
        [Required, MaxLength(100)] public string AccountNumber { get; set; }
        [Required, MaxLength(50)] public string BankType { get; set; }
        [Required, MaxLength(100)] public string ShopName { get; set; }
        [MaxLength(200)] public string ShopAddress { get; set; }
        [Required] public string Shoptype { get; set; }

        // File uploads
        public IFormFile OwnerIdentityBook { get; set; }
        public IFormFile BusRegCertificate { get; set; }

        [Required, DataType(DataType.Password)] public string Password { get; set; }
        [Required, Compare("Password"), DataType(DataType.Password)] public string ConfirmPassword { get; set; }
    }

    public class DeliveryDriverViewModel
    {

        [EmailAddress]
        [Required(ErrorMessage ="Please enter email")]
        public string Email { get; set; }

        [Required(ErrorMessage ="Name is required")]

        public string DriverName { get; set; }

        [Required(ErrorMessage ="Surname is required")]

        public string DriverSurname { get; set; }

        [Required(ErrorMessage = "Phone number is Required")]

        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Residential adress is required")]

        public string ResidentialAddress { get; set; }

        [Required(ErrorMessage = "Licence number is required")]

        public string LicenceNumber { get; set; }

        [Required(ErrorMessage = "Enter your vehicle type")]
        public string VehicleType { get; set; }

        [Required(ErrorMessage = "Vehicle number is required")]

        public string VehicleNumber { get; set; }

        [Required(ErrorMessage = "Account number is required")]

        public string AccountNumber { get; set; }


        [Required(ErrorMessage = "Enter bank type")]

        public string BankType { get; set; }
       
        [Required(ErrorMessage ="Passwword is required")]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        [Required(ErrorMessage ="Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

       
    }
    
    public class StaffViewModel
    {
        
        [Required(ErrorMessage ="First name is required")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [DisplayName("Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number address is required")]
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        //DIGITAL ACCOUNT SET-UP
        [Required(ErrorMessage = "Password is required")]
        [DisplayName("Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DisplayName("Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        public string UserType { get; set; }

        public DateTime RegisteredOn { get; set; }
    }

    public class AddProductViewModel
    {
       

        //[MaxLength(100)]
        [Required(ErrorMessage ="Product name is required")]
        [DisplayName("Product Name")]
        public string ProdName { get; set; }


        [DisplayName("Product Category")]
        [Required(ErrorMessage = "Product category is required")]

        public SelectCategory ProdCategory { get; set; }

        [DisplayName("Product Description")]
        [Required(ErrorMessage = "Product description is required")]
        public string ProdDescription { get; set; }

        [DisplayName("Product Price")]
        [Required(ErrorMessage ="Product price is required")]
        public double ProdPrice { get; set; }

        [DisplayName("Product Size(e.g kg, mm, ect..)")]
        [Required(ErrorMessage ="Product size is required")]
        public string ProdSize { get; set; } 

        [DisplayName("Product Quantity")]
        [Required(ErrorMessage ="Product quantity is required")]
        public int ProdQuantity { get; set; }

        [DisplayName("Product Picture")]
        [Required(ErrorMessage = "Picture of the product is required")]
      public IFormFile ProductPicture { get; set; }

  
    }
}