using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.WebApi.Application.Identity.Users.Password;
public class VerifyPasswordOtpRequest
{
    public string Email { get; set; }
    public string Otp { get; set; }
}
