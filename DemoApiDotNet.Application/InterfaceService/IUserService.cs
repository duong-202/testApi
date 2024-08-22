using DemoApiDotNet.Application.Payloads.RequestModels.UserRequests;
using DemoApiDotNet.Application.Payloads.ResponseModels.DataUsers;
using DemoApiDotNet.Application.Payloads.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApiDotNet.Application.InterfaceService
{
    public interface IUserService
    {
        Task<ResponseObject<DataResponseUser>> UpdateUser(long userId, Request_UpdateUser resquest);
    }
}
