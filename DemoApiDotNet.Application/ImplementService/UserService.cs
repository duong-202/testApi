using DemoApiDotNet.Application.Handle.HandleFile;
using DemoApiDotNet.Application.InterfaceService;
using DemoApiDotNet.Application.Payloads.Mappers;
using DemoApiDotNet.Application.Payloads.RequestModels.UserRequests;
using DemoApiDotNet.Application.Payloads.ResponseModels.DataUsers;
using DemoApiDotNet.Application.Payloads.Responses;
using DemoApiDotNet.Domain.Entities;
using DemoApiDotNet.Domain.InterfaceRepositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApiDotNet.Application.ImplementService
{
    public class UserService : IUserService
    {
        private readonly IBaseRepository<User> _repository;
        private readonly UserConverter _converter;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(IBaseRepository<User> repository, UserConverter converter, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _converter = converter;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseObject<DataResponseUser>> UpdateUser(long userId, Request_UpdateUser resquest)
        {
            var currentUser = _httpContextAccessor.HttpContext.User;
            try
            {
                if (!currentUser.Identity.IsAuthenticated)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Message = "Người dùng chưa được xác thực",
                        Data = null
                    };
                }
                var user = currentUser.FindFirst("Id").Value;
                var userItem = await _repository.GetByIdAsync(userId);
                if (long.Parse(user) != userId && long.Parse(user) != userItem.Id)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Message = "Bạn không có quyền thực hiện chức năng này",
                        Data = null
                    };
                }
                
                userItem.Avatar = await HandleUploadFile.WriteFile(resquest.Avatar);
                userItem.PhoneNumber = resquest.PhoneNumber;
                userItem.DateOfBirth = resquest.DateOfBirth;
                userItem.Email = resquest.Email;
                userItem.UpdateTime = DateTime.Now;
                userItem.FullName = resquest.FullName;
                await _repository.UpdateAsync(userItem);
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Cập nhật thông tin người dùng thành công",
                    Data = _converter.EntityToDTO(userItem)
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
