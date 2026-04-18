using System.ComponentModel.DataAnnotations;

namespace CSI402_Project.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        [Display(Name = "อีเมล")]
        public string Email { get; set; } = string.Empty;
    }
}
