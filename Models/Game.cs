using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System;


namespace FYP_Project.Models
{
    public class Game
    {
        public string Session_ID { get; set; }

        [Required(ErrorMessage = "Please Enter the Game Session Name!")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Min. 5 characters Max. 50 characters!")]
        public string SessionName { get; set; }

        [Required]
        public string SessionKey { get; set; }


        [Required(ErrorMessage = "Please Enter the Game Session ID!")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Game Session IDs are only 6 characters!")]
        public string JoinSession { get; set; }


        [DataType(DataType.DateTime)]
        public DateTime Start { get; set; }


        [DataType(DataType.DateTime)]
        public DateTime End { get; set; }


    }
}
