
using System.ComponentModel.DataAnnotations;

namespace FYP_Project.Models
{
    public class PlayerLogin
    {
        [Required(ErrorMessage = "Please enter Username!")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter Password!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public string UserRole { get; set; }
    }
}
