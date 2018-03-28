using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InstructionalResource2.Models
{
    public class UserLogin
    {
        [Display(Name="Email Id")]
        [Required(AllowEmptyStrings=false,ErrorMessage="Email Id required")]
        public string EmailId { get; set; }

        [Required(AllowEmptyStrings=false,ErrorMessage="Password required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name="Remember Me")]
        public bool RememberMe { get; set; }
    }
}