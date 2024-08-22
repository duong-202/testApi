using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApiDotNet.Application.Payloads.RequestModels.UserRequests
{
    public class Request_UpdateUser
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public IFormFile Avatar { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
