using CM_Project.Models.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CM_Project.Models
{
    public enum Status { Pending, Approved, Rejected }

    public class DeliveryDriver
    {
        [Key]
        public int DriverId { get; set; }

        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; }

        [Required, StringLength(100)]
        public string DriverName { get; set; }

        [Required, StringLength(100)]
        public string DriverSurname { get; set; }

        [Required, StringLength(100)]
        public string PhoneNumber { get; set; }

        [Required, StringLength(250)]
        public string ResidentialAddress { get; set; }

        [Required] public string LicenceNumber { get; set; }
        [Required, StringLength(100)] public string VehicleType { get; set; }
        [Required] public string VehicleNumber { get; set; }
        [Required] public string AccountNumber { get; set; }
        public string BankType { get; set; }
        public bool IsAvailable { get; set; }
        public Status ApplicationStatus { get; set; }

        //Navigation properties
        //Added =  sign as you said
        public virtual ICollection<DriverIdentityBook> DriverIdentityBooks { get; set; } = new List<DriverIdentityBook>();
        public virtual ICollection<DriversLicence> DriversLicences { get; set; } = new List<DriversLicence>();

        [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
        public string Password { get; set; }

        [NotMapped, DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string UserType { get; set; }
        public DateTime RegisteredOn { get; set; }
    }
}
