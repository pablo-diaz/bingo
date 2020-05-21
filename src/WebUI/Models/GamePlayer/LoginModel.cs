using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.GamePlayer
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Por favor escribe un login")]
        [StringLength(100, ErrorMessage = "El login es muy largo. Intenta con uno más corto")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Por favor escribe una contraseña")]
        [StringLength(100, ErrorMessage = "La contraseña es muy larga. Intenta con una más corta")]
        public string Passwd { get; set; }
    }
}
