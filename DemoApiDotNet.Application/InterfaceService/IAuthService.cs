using DemoApiDotNet.Application.Payloads.RequestModels.UserRequests;
using DemoApiDotNet.Application.Payloads.ResponseModels.DataUsers;
using DemoApiDotNet.Application.Payloads.Responses;
using DemoApiDotNet.Domain.Entities;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApiDotNet.Application.InterfaceService
{
    public interface IAuthService
    {
        Task<ResponseObject<DataResponseUser>> Register(Request_Register request);
        Task<string> ConfirmRegisterAccount(string confirmCode);
        Task<ResponseObject<DataResponseLogin>> GetJwtTokenAsync(User user);
        Task<ResponseObject<DataResponseLogin>> Login(Request_Login request);
        Task<ResponseObject<DataResponseUser>> ChangePassword(long userId, Request_ChangePassword request);
        Task<string> ForgotPassword(string email);
        Task<string> ConfirmCreateNewPassword(Request_CreateNewPassword request);
        Task<string> AddRolesToUser(long userId, List<string> roles);
        Task<string> DeleteRoles(long userId, List<string> roles);
    }
}
