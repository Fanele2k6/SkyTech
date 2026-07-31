using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace SkyTech.Models.SkytechWorkers
{
    public class Branch
    {
        [Key]
        public int BranchId {  get; set; }

        public string BranchName { get; set; }
        public string BranchLocation { get; set; }
        [ValidateNever]

        public virtual ICollection<Staff> Staffs { get; set; }
    }
}
