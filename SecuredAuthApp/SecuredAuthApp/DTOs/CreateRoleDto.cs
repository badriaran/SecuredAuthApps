using System.ComponentModel.DataAnnotations;

namespace SecuredAuthApp.DTOs
{
    public class CreateRoleDto
    {
        [Required(ErrorMessage ="Roles Name is Required")]
        public string RoleName { get; set; }
    }
}
