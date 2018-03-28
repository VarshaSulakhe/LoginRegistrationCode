using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace InstructionalResource2.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
        public string ConfirmPassword { get; set; }
    }

    public class UserMetadata
    {
            [Display(Name="First Name")]
            [Required(AllowEmptyStrings=false,ErrorMessage="FirstName Required")] 
          public string FirstName { get; set; }

        [Display(Name="Last Name")]
        [Required(AllowEmptyStrings=false,ErrorMessage="Last Name required")]
            public string LastName { get; set; }
        [Display(Name="Email Id")]
        [Required(AllowEmptyStrings=false,ErrorMessage="Email Id required")]
        [DataType(DataType.EmailAddress)]
        public string EmailId { get; set; }

        [Required(AllowEmptyStrings=false,ErrorMessage="Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6,ErrorMessage="Minimum 6 characters required")]
        public string Password { get; set; }

        [Display(Name="Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage="Confirm password and passwords dont match")]
        public string ConfirmPassword { get; set; }
    }
}