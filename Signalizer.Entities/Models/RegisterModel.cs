using System.ComponentModel.DataAnnotations;

namespace Signalizer.Entities.Models
{
    public class RegisterModel : LoginModel
    {
        [Required]
        public string ConfirmPassword { get; set; }
    }
}
