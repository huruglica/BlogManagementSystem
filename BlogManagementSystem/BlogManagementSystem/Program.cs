using AutoMapper;
using BlogManagementSystem.Helpers;
using Contratcs.IRepository;
using Contratcs.IServices;
using FireBase;
using FirebaseAdmin;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ModelDto.BlogDto;
using ModelDto.CommentDto;
using ModelDto.UserDto;
using Persistence.Datebase;
using Persistence.Repository;
using Services.AutoMapper;
using Services.Services;
using Services.Services.IServices;
using Services.Validators.BlogValidator;
using Services.Validators.CommentValidator;
using Services.Validators.UserValidator;
using System.Text;

namespace BlogManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            var mapperConfiguration = new MapperConfiguration(
                mc => mc.AddProfile(new AutoMapperConfigurations()));

            IMapper mapper = mapperConfiguration.CreateMapper();

            builder.Services.AddSingleton(mapper);

            builder.Services.AddDbContext<BlogManagementDatabase>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DeafultConnection")));

            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<ICommentService, CommentService>();
            builder.Services.AddTransient<IBlogService, BlogService>();
            builder.Services.AddTransient<ILikesService, LikesService>();
            builder.Services.AddTransient<ITagService, TagService>();

            builder.Services.AddTransient<FireBaseNotification>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<IBlogRepository, BlogRepository>();
            builder.Services.AddScoped<ILikesRepository, LikesRepository>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();

            builder.Services.AddScoped<IValidator<UserCreateDto>, UserCreateDtoValidator>();
            builder.Services.AddScoped<IValidator<UserUpdateDto>, UserUpdateDtoValidator>();
            builder.Services.AddScoped<IValidator<CommentCreateDto>, CommentCreateDtoValidator>();
            builder.Services.AddScoped<IValidator<CommentUpdateDto>, CommentUpdateDtoValidator>();
            builder.Services.AddScoped<IValidator<BlogCreateDto>, BlogCreateDtoValidator>();
            builder.Services.AddScoped<IValidator<BlogUpdateDto>, BlogUpdateDtoValidator>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "dev-nq3upfdndrxpn4bz.us.auth0.com",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("3RiMMI3eusj2CJu15cJQIpXP8YallpXQQj8ad_13GiLu4uS7sUxL3Wezw6HpzfLL"))
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "BlogManagementSystem", Version = "v1" });
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization",
                    In = ParameterLocation.Header
                });
                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            var rootPath = builder.Environment.ContentRootPath;
            string[] pathParts = rootPath.Split(new string[] { "\\" }, StringSplitOptions.None);
            string combinedString = string.Join("\\", pathParts.Take(pathParts.Length - 1));
            var path = Path.Combine(combinedString, "FireBase\\private_key.json");

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(path)
            });

            var app = builder.Build();

            app.UseSession();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.DisplayRequestDuration();
                    c.DefaultModelExpandDepth(0);

                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlogManagementSystem");
                    c.OAuthClientId("pwhlJHybcPXXkCWZ2MGLXlDFTXD9oTK4");
                    c.OAuthClientSecret("3RiMMI3eusj2CJu15cJQIpXP8YallpXQQj8ad_13GiLu4uS7sUxL3Wezw6HpzfLL");
                    c.OAuthAppName("BlogManagementSystem");
                    //c.OAuthUsePkce();
                    //c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                    c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}