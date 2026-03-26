using System.ComponentModel.DataAnnotations;

namespace CSI402_Project.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกชื่อผู้ใช้")]
        [Display(Name = "ชื่อผู้ใช้")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่าน")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "จดจำฉัน")]
        public bool RememberMe { get; set; }
    }
}
