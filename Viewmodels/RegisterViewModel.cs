using System.ComponentModel.DataAnnotations;

namespace CSI402_Project.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกชื่อผู้ใช้")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "ชื่อผู้ใช้ต้องมีความยาว 3-100 ตัวอักษร")]
        [Display(Name = "ชื่อผู้ใช้")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        [StringLength(150, ErrorMessage = "อีเมลต้องไม่เกิน 150 ตัวอักษร")]
        [Display(Name = "อีเมล")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร")]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่าน")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณายืนยันรหัสผ่าน")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "รหัสผ่านและยืนยันรหัสผ่านไม่ตรงกัน")]
        [Display(Name = "ยืนยันรหัสผ่าน")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
        [Phone(ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
        [StringLength(20, ErrorMessage = "เบอร์โทรศัพท์ต้องไม่เกิน 20 ตัวอักษร")]
        [Display(Name = "เบอร์โทรศัพท์")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกที่อยู่")]
        [StringLength(255, ErrorMessage = "ที่อยู่ต้องไม่เกิน 255 ตัวอักษร")]
        [Display(Name = "ที่อยู่จัดส่ง")]
        public string Address { get; set; } = string.Empty;
    }
}
