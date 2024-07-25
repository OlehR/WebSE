using System.ComponentModel.DataAnnotations;

namespace Supplyer.ViewModel
{
    public class LoginModelVM
    {
        [Required()]
        public string Login { get; set; }
        [Required()]
        public string Password { get; set; }
    }
}
