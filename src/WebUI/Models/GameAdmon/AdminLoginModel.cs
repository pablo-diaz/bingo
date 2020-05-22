using System.ComponentModel.DataAnnotations;

namespace WebUI.Models.GameAdmon
{
    public class AdminLoginModel
    {
        [Required(ErrorMessage = "Por favor escribe una contraseña")]
        [StringLength(100, ErrorMessage = "La contraseña es muy larga. Intenta con una más corta")]
        public string Passwd { get; set; }
    }
}
