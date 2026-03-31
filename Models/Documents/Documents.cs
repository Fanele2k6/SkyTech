using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace CM_Project.Models.Documents
{

    //-----------------------------SHOP OWNER DOCS-----------------------------
    public class BusRegCertificate
    {
        [Key]
        public int CertId { get; set; }

        // FK to ShopOwner
        public int OwnerId { get; set; }
        [ForeignKey("OwnerId")]   // ✅ explicitly tie FK to OwnerId
        public virtual ShopOwner ShopOwner { get; set; }

        public byte[] FileData { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public DateTime SavedOn { get; set; }
    }


    public class OwnerIdentityBook
    {
        [Key]
        public int OwnerIdentityBookId { get; set; }

        // FK to ShopOwner
        public int OwnerId { get; set; }
        [ForeignKey("OwnerId")]   // ✅ explicitly tie FK to OwnerId
        public virtual ShopOwner ShopOwner { get; set; }

        public byte[] FileData { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public DateTime SavedOn { get; set; }
    }


    public class ProductPicture
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProdPictureId { get; set; }
        public int OwnerId { get; set; }
        public virtual ShopOwner ShopOwner { get; set; }
        public byte[] FileData { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public DateTime SavedOn { get; set; }
    }

    //---------------------------------DRIVER DOCS--------------------------
   
    public class DriversLicence
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LicenceId { get; set; }
        public int DriverId { get; set; }
        public virtual DeliveryDriver Driver { get; set; }
        public byte[] FileData { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public DateTime SavedOn { get; set; }
    }

    public class DriverIdentityBook
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DriverIdentityBookId { get; set; }
        public int DriverId { get; set; }
        public virtual DeliveryDriver Driver { get; set; }
        public byte[] FileData { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public DateTime SavedOn { get; set; }
    }

}