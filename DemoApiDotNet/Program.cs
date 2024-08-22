using DemoApiDotNet.Infrastructure.DataContexts;//
using Microsoft.EntityFrameworkCore;//
using DemoApiDotNet.Application.InterfaceService;
using DemoApiDotNet.Application.ImplementService;//
using DemoApiDotNet.Application.Payloads.Mappers;//
using DemoApiDotNet.Domain.InterfaceRepositories;//
using DemoApiDotNet.Domain.Entities;//
using DemoApiDotNet.Infrastructure.ImplementRepositories;//
using DemoApiDotNet;
using DemoApiDotNet.Application.Handle.HandleEmail;
using Microsoft.AspNetCore.Authentication.JwtBearer;//
using Microsoft.IdentityModel.Tokens;//
using Microsoft.OpenApi.Models;//
using System.Text;//

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString(Constant.AppSettingKeys.DEFAULT_CONNECTION)));
//builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UserConverter>();
builder.Services.AddScoped<IBaseRepository<User>, BaseRepository<User>>();
builder.Services.AddScoped<IDbContext, AppDbContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IBaseRepository<ConfirmEmail>, BaseRepository<ConfirmEmail>>();
builder.Services.AddScoped<IBaseRepository<RefreshToken>, BaseRepository<RefreshToken>>();
builder.Services.AddScoped<IBaseRepository<Permission>, BaseRepository<Permission>>();
builder.Services.AddScoped<IBaseRepository<Role>, BaseRepository<Role>>();
builder.Services.AddScoped<IUserService, UserService>();

var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true, // Xác thực rằng người tạo token của JWT hợp lệ
        ValidateAudience = true, // Xác thực rằng người nhận token của JWT hợp lệ
        ValidateLifetime = true, // Xác thực rằng JWT còn hiệu lực trên thời gian sống (Lifetime)
        ValidateIssuerSigningKey = true, // Xác thực rằng token đã được ký bằng khóa bảo mật hợp lệ
        ClockSkew = TimeSpan.Zero, // Đặt độ lệch thời gian cho việc kiểm tra thời gian sống của token
        ValidAudience = builder.Configuration["JWT:ValidAudience"], // Xác định đối tượng nhận token hợp lệ mà ứng dụng mong đợi
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"], // Xác định người phát hành token hợp lệ mà ứng dụng mong đợi
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"])) // Xác định khóa bí mật được sử dụng để ký và xác thực token
    };
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Auth Api", Version = "v1" }); // Tạo 1 tài liệu Swagger mới cho API có tiêu đề là Auth Apo và phiên bản v1
    option.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme // Định nghĩa các mà người dùng sẽ cung cấp token để xác thực khi gọi API
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header, // Xác định vị trí mà token sẽ được gửi trong yêu cầu HTTP
        Description = "Vui lòng nhập token", // Mô tả
        Name = "Authorization", //Tên của header chưa token
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http, // Xác định loại phương thức xác thực
        BearerFormat = "JWT", // Xác định định dạng của token
        Scheme = "Bearer", // Xác đinh cheme (cơ chế) được sử dụng cho xác thực
    });
    option.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement // AddSecurityRequirement yêu cầu người dùng phải cung cấp token khi gọi bất kỳ API nào
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    }); // Cấu hình phương thức xác thực cần thiết cho yêu cầu API
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseAuthentication(); // xác thực
app.UseAuthorization();

app.MapControllers();

app.Run();
