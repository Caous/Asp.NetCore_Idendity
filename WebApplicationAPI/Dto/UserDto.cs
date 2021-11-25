using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplicationAPI.Dto
{
    public class UserDto
    {
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmedPassword { get; set; }
        public string NamedFull { get; set; }
    }
}
