using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace FYP_Project.Models
{
    public class Player
    {

        // FOR BOTH PLAYER AND ADMIN \\
        // FOR BOTH PLAYER AND ADMIN \\
        // FOR BOTH PLAYER AND ADMIN \\


        [Required(ErrorMessage = "Please Enter Username!")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "8-50 characters only!")]
        [Remote(action: "VerifyUserID", controller: "Account")]
       // [Remote(action: "ManageAccount2", controller: "Account")]

        public string UserName { get; set; }

        [Required(ErrorMessage = "Please Enter Your Name!")]
        [StringLength(50, ErrorMessage = "Maximum 50 characters only!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please Enter Email!")]
        [EmailAddress(ErrorMessage = "Invalid Email!")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        //Password Validataion
        // Must have 1 Uppercase, 1 Lowercase 1 number and 1 Symbol.

        [Required(ErrorMessage = "Please enter Password")]     
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Password must be 8 characters or more!")]
        [RegularExpression(@"^.*(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*\(\)_\-+=]).*$", ErrorMessage = "Include 1 Uppercase, 1 Lowercase and 1 Symbol!")]
        [DataType(DataType.Password)]
        public string PlayerPw { get; set; }

        [Compare("PlayerPw", ErrorMessage = "Passwords do not match!")]
        public string PlayerPw2 { get; set; }

        public string UserRole { get; set; }

        public string PlayerID { get; set; }

        public string Rank { get; set; }

        public DateTime LastLogin { get; set; }


    }
}
