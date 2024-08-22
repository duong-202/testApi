using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoApiDotNet.Domain.ConstantsDomain;

namespace DemoApiDotNet.Domain.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public DateTime CreateTime { get; set; }
        [MaybeNull]
        public DateTime UpdateTime { get; set; }
        public string Avatar {  get; set; }
        public DateTime DateOfBirth { get; set; }
        public Enumerates.UserStatusEnum UserStatus { get; set; } = Enumerates.UserStatusEnum.UnActivated;
        public virtual ICollection<Permission>? Permissions { get; set; }
}
}
