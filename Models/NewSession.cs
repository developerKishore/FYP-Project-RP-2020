using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FYP_Project.Models
{
    public class NewSession
    {

        public string sessionID { get; set; }

        public string sessionKey { get; set; }

        [Required(ErrorMessage = "Please Enter Session Name!")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "8-50 characters only!")]
        public string SessionName { get; set; }

        [Required(ErrorMessage = "This field is required!")]
        //[StringLength(2, ErrorMessage = "10-60 mins only")]
        [Range(10,60, ErrorMessage = "10-60 mins only")]
        public string gameTime { get; set; }

 

    }
}
