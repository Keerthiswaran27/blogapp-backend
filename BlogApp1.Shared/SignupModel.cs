using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared
{
    public class SignupModel
    {

        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<string> Role { get; set; }
    }
    public class CreateProfileRequest
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string>? Role { get; set; }
    }
    public class SignupResponseDto
    {
        public Guid UserId { get; set; }
    }
}
