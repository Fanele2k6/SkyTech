using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyTech.Models.JobPostServices
{
    public class ApplicationForm
    {
        [Key]
        public int ApplicationId { get; set;  }

        [ForeignKey(nameof(Post))]
        public int PostId { get; set; }
        public virtual JobPost Post { get; set; }

        [ForeignKey(nameof(Customers))]
        public int CustomerId { get; set; }
        public virtual Customer Customers { get; set; }

        public string FirstName { get; set; }
        //public string SecondName { get; set; }
        public string LastName { get; set; }

        public string IDNumber {  get; set; }

        public string Email {  get; set; }
        public string Location { get; set; }
        public string Experience {  get; set; }
        public string Skills { get; set; }
        public string Links { get; set;  }
    }
}
