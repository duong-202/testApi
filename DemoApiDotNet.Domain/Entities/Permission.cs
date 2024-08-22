
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApiDotNet.Domain.Entities
{
    public class Permission : BaseEntity // bảng trung gian của Role và User
    {
        public long UsertId { get; set; }
        public virtual User? User { get; set; }
        public long RoleId { get; set; }
        public virtual Role? Role { get; set; }
    }
}
