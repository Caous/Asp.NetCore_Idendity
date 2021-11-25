using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace WebApi.Domain
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; }

        public string Member { get; set; } = "Member";
        public string OrgId { get; set; }
        public List<UserRole> UserRoles { get; set; }

     
    }
}
