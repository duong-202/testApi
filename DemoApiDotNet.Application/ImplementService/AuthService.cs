using DemoApiDotNet.Application.Handle.HandleEmail;
using DemoApiDotNet.Application.InterfaceService;
using DemoApiDotNet.Application.Payloads.Mappers;
using DemoApiDotNet.Application.Payloads.RequestModels.UserRequests;
using DemoApiDotNet.Application.Payloads.ResponseModels.DataUsers;
using DemoApiDotNet.Application.Payloads.Responses;
using DemoApiDotNet.Domain;
using DemoApiDotNet.Domain.ConstantsDomain;
using DemoApiDotNet.Domain.Entities;
using DemoApiDotNet.Domain.InterfaceRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using BcryptNet = BCrypt.Net.BCrypt;

namespace DemoApiDotNet.Application.ImplementService
{
    public class AuthService : IAuthService
    {
        private readonly IBaseRepository<User> _baseUserRepository;
        private readonly UserConverter _userConverter;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IBaseRepository<ConfirmEmail> _baseConfirmEmailRepository;
        private readonly IBaseRepository<Permission> _basePermissionRepository;
        private readonly IBaseRepository<Role> _baseRoleRepository;
        private readonly IBaseRepository<RefreshToken> _baseRefreshTokenRepository;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthService(IBaseRepository<User> baseUserRepository, UserConverter userConverter, IConfiguration configuration, IUserRepository userRepository,
        IEmailService emailService, IBaseRepository<ConfirmEmail> baseConfirmEmailRepository, IBaseRepository<Permission> basePermissionRepository,
        IBaseRepository<Role> baseRoleRepository, IBaseRepository<RefreshToken> baseRefreshTokenRepository, IHttpContextAccessor contextAccessor)
        {
            _baseUserRepository = baseUserRepository;
            _userConverter = userConverter;
            _configuration = configuration;
            _userRepository = userRepository;
            _emailService = emailService;
            _baseConfirmEmailRepository = baseConfirmEmailRepository;
            _basePermissionRepository = basePermissionRepository;
            _baseRoleRepository = baseRoleRepository;
            _baseRefreshTokenRepository = baseRefreshTokenRepository;
            _contextAccessor = contextAccessor;
        }


        public AuthService()
        {

        }

        public async Task<ResponseObject<DataResponseUser>> Register(Request_Register request)
        {
            try
            {
                if (!ValidateInput.IsValidEmail(request.Email))
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Định dạng email không hợp lệ",
                        Data = null
                    };
                }
                if (!ValidateInput.IsValidPhoneNumber(request.PhoneNumber))
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Định dạng số điện thoại không hợp lệ",
                        Data = null
                    };
                }
                if (await _userRepository.GetUserByEmail(request.Email) != null)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Email đã tồn tại trong hệ thống! vui lòng sử dụng email khác",
                        Data = null
                    };
                }
                if (await _userRepository.GetUserByPhoneNumber(request.PhoneNumber) != null)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Số điện thoại đã tồn tại trong hệ thống! vui lòng sử dụng số điện thoại khác",
                        Data = null
                    };
                }
                if (await _userRepository.GetUserByUsername(request.UserName) != null)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "UserName đã tồn tại trong hệ thống! vui lòng sử dụng UserName khác",
                        Data = null
                    };
                }
                var user = new User
                {
                    Avatar = "data:image/png;base64,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",
                    IsActive = true,
                    CreateTime = DateTime.Now,
                    DateOfBirth = request.DateOfBirth,
                    Email = request.Email,
                    FullName = request.FullName,
                    Password = BcryptNet.HashPassword(request.Password),
                    PhoneNumber = request.PhoneNumber,
                    UserName = request.UserName,
                    UserStatus = Domain.ConstantsDomain.Enumerates.UserStatusEnum.UnActivated
                };
                user = await _baseUserRepository.CreateAsync(user);
                await _userRepository.AddRolesToUserAsync(user, new List<string> { "User" });
                ConfirmEmail confirmEmail = new ConfirmEmail
                {
                    IsActive = true,
                    ComfirmCode = GenerateCodeActive(),
                    ExpiryTime = DateTime.Now.AddMinutes(1),
                    IsConfirm = false,
                    UserId = user.Id,
                };
                confirmEmail = await _baseConfirmEmailRepository.CreateAsync(confirmEmail);
                var message = new EmailMessage(new string[] { request.Email }, "Nhận mã xác nhận tại đây: ", $"Mã xác nhận: {confirmEmail.ComfirmCode}");
                var responseMessage = _emailService.SendEmail(message);
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Bạn đã gửi yêu cầu đăng ký! Vui lòng nhận mã xác nhận tại email để đăng ký tài khoản",
                    Data = _userConverter.EntityToDTO(user)
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = "Error: " + ex.Message,
                    Data = null
                };
            }
        }
        private string GenerateCodeActive()
        {
            string str = "theduong_" + DateTime.Now.Ticks.ToString();
            return str;
        }

        public async Task<string> ConfirmRegisterAccount(string confirmCode)
        {
            try
            {
                var code = await _baseConfirmEmailRepository.GetAsync(x => x.ComfirmCode.Equals(confirmCode));
                if (code == null)
                {
                    return "Mã xác nhận không hợp lệ";
                }
                var user = await _baseUserRepository.GetAsync(x => x.Id == code.UserId);
                if (code.ExpiryTime < DateTime.Now)
                {
                    return "Mã xác nhận đã hết hạn";
                }
                user.UserStatus = Domain.ConstantsDomain.Enumerates.UserStatusEnum.Activated;
                code.IsConfirm = true;
                await _baseUserRepository.UpdateAsync(user);
                await _baseConfirmEmailRepository.UpdateAsync(code);
                return "Xác nhận đăng ký tài khoản thành công, từ giờ bạn có thể sử dụng tài khoản này để đăng nhập";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<ResponseObject<DataResponseLogin>> GetJwtTokenAsync(User user)
        {
            var permissions = await _basePermissionRepository.GetAllAsync(x => x.UsertId == user.Id);
            var roles = await _baseRoleRepository.GetAllAsync();
            var authClaims = new List<Claim>
            {
                new Claim("Id", user.Id.ToString()),
                new Claim("UserName", user.UserName.ToString()),
                new Claim("Email", user.Email.ToString()),
                new Claim("PhoneNumber", user.PhoneNumber.ToString()),
            };
            foreach(var permission in permissions)
            {
                foreach(var role in roles)
                {
                    if(role.Id == permission.Id)
                    {
                        authClaims.Add(new Claim("Permission", role.RoleName));
                    }
                }
            }
            var userRoles = await _userRepository.GetRolesOfUserAsync(user);
            foreach(var item in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, item));
            }
            var jwtToken = GetToken(authClaims);
            var refreshToken = GenerateRefreshToken();
            _ = int.TryParse(_configuration["JWT: RefreshTokenValidity"], out int refreshTokenValidity);

            RefreshToken rf = new RefreshToken
            {
                IsActive = true,
                ExpiryTime = DateTime.Now.AddHours(refreshTokenValidity),
                UserId = user.Id,
                Token = refreshToken
            };
            rf = await _baseRefreshTokenRepository.CreateAsync(rf);
            return new ResponseObject<DataResponseLogin>
            {
                Status = StatusCodes.Status200OK,
                Message = "Tạo token thành công",
                Data = new DataResponseLogin
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    RefreshToken = refreshToken,
                }
            };
        }

        public async Task<ResponseObject<DataResponseLogin>> Login(Request_Login request)
        {
            var user = await _baseUserRepository.GetAsync(x => x.UserName.Equals(request.UserName));
            if (user == null)
            {
                return new ResponseObject<DataResponseLogin>
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = "Sai tên tài khoản",
                    Data = null,
                }; 
            }
            if (user.UserStatus.ToString().Equals(Enumerates.UserStatusEnum.UnActivated.ToString()))
            {
                return new ResponseObject<DataResponseLogin>
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = "Tài khoản chưa được xác thực",
                    Data = null,
                };
            }
            bool checkPass = BcryptNet.Verify(request.Password, user.Password);
            if (!checkPass)
            {
                return new ResponseObject<DataResponseLogin>
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = "Mật khẩu không chính xác",
                    Data = null,
                };
            }
            return new ResponseObject<DataResponseLogin>
            {
                Status = StatusCodes.Status200OK,
                Message = "Đăng nhập thành công",
                Data = new DataResponseLogin
                {
                    AccessToken = GetJwtTokenAsync(user).Result.Data.AccessToken,
                    RefreshToken = GetJwtTokenAsync(user).Result.Data.RefreshToken,
                }
            };
        }

        public async Task<ResponseObject<DataResponseUser>> ChangePassword(long userId, Request_ChangePassword request)
        {
            try
            {
                var user = await _baseUserRepository.GetByIdAsync(userId); // Không cần phải check nó có tồn tại hay không vì mình sẽ lấy thông tin của tài khoản đang trong phiên đăng nhập
                bool checkPass = BcryptNet.Verify(request.OldPassword, user.Password);
                if (!checkPass)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Mật khẩu không chính xác",
                        Data = null,
                    };
                }
                if (!request.NewPassword.Equals(request.ConfirmPassword))
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Mật khẩu không trùng khớp",
                        Data = null,
                    };
                }
                user.Password = BcryptNet.HashPassword(request.NewPassword);
                user.UpdateTime = DateTime.Now;
                await _baseUserRepository.UpdateAsync(user);
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Đổi mật khẩu thành công",
                    Data = _userConverter.EntityToDTO(user)
                };
            }
            catch(Exception ex)
            {
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = null,
                };
            }
        }

        public async Task<string> ForgotPassword(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmail(email);
                if (user == null)
                {
                    return "Email không tồn tại trong hệ thống";
                }
                //var listConfirmCode = await _baseConfirmEmailRepository.GetAllAsync(x => x.UserId == user.Id);
                //if (listConfirmCode.ToList().Count > 0)
                //{
                //    foreach (var confirmCode in listConfirmCode)
                //    {
                //        await _baseConfirmEmailRepository.DeleteAsync(confirmCode.Id);
                //    }
                //}
                ConfirmEmail confirmEmail = new ConfirmEmail
                {
                    IsActive = true,
                    ComfirmCode = GenerateCodeActive(),
                    ExpiryTime = DateTime.Now.AddMinutes(1),
                    UserId = user.Id,
                    IsConfirm = false
                };
                confirmEmail = await _baseConfirmEmailRepository.CreateAsync(confirmEmail);
                var message = new EmailMessage(new string[] { user.Email }, "Nhận mã xác nhận tại đây: ", $"Mã xác nhận là: {confirmEmail.ComfirmCode}");
                var send = _emailService.SendEmail(message);
                return "Gửi mã xác nhận về email thành công! Vui lòng kiểm tra email";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> ConfirmCreateNewPassword(Request_CreateNewPassword request)
        {
            try
            {
                var confirmEmail = await _baseConfirmEmailRepository.GetAsync(x => x.ComfirmCode.Equals(request.ConfirmCode));
                if (confirmEmail == null)
                {
                    return "Mã xác nhận không hợp lệ";
                }
                if (confirmEmail.ExpiryTime < DateTime.Now)
                {
                    return "Mã xác nhận đã hết hạn";
                }
                if (!request.NewPassword.Equals(request.ConfirmPassword))
                {
                    return "Mật khẩu không trùng khớp";
                }
                var user = await _baseUserRepository.GetAsync(x => x.Id == confirmEmail.UserId);
                user.Password = BcryptNet.HashPassword(request.ConfirmPassword);
                user.UpdateTime = DateTime.Now;
                await _baseUserRepository.UpdateAsync(user);
                return "Tạo mật khẩu mới thành công";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> AddRolesToUser(long userId, List<string> roles)
        {
            var currenUser = _contextAccessor.HttpContext.User;
            try
            {
                if (!currenUser.Identity.IsAuthenticated)
                {
                    return "Người dùng chưa được xác thực";
                }
                if (!currenUser.IsInRole("Admin"))
                {
                    return "Bạn không có quyền thực hiện chức năng này";
                }
                var user = await _baseUserRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return "Không tìm thấy người dùng";
                }
                await _userRepository.AddRolesToUserAsync(user, roles);
                return "Thêm quyền cho người dùng thành công";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> DeleteRoles(long userId, List<string> roles)
        {
            var currenUser = _contextAccessor.HttpContext.User;
            try
            {
                if (!currenUser.Identity.IsAuthenticated)
                {
                    return "Người dùng chưa được xác thực";
                }
                if (!currenUser.IsInRole("Admin"))
                {
                    return "Bạn không có quyền thực hiện chức năng này";
                }
                var user = await _baseUserRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return "Người dùng không tồn tại";
                }
                await _userRepository.DeleteRolesAsync(user, roles);
                return "Xóa quyền thành công";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #region Private Methods
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            _ = int.TryParse(_configuration["JWT:TokenValidityInHours"], out int tokenValidityInHours);
            var expirationUTC = DateTime.Now.AddHours(tokenValidityInHours);

            var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: expirationUTC,
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }

        // tạo ra token mới để duy trì phiên đăng nhập
        private string GenerateRefreshToken()
        {
            var randomNumber = new Byte[64];
            var range = RandomNumberGenerator.Create();
            range.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        #endregion
    }
}