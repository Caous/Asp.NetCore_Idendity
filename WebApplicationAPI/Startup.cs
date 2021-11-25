using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebApi.Domain;
using WebApi.Repository;
using WebApplicationAPI.Helper;

namespace WebApplicationAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplicationAPI", Version = "v1" });
            });
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddDbContext<Context>(
                opt => opt.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationAssembly)
                ));

            services.AddIdentity<User, Role>(options =>
            {
                //options.SignIn.RequireConfirmedEmail = true;

                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredLength = 4;

                //options.Lockout.MaxFailedAccessAttempts = 3;
                //options.Lockout.AllowedForNewUsers = true;

            })
                .AddEntityFrameworkStores<Context>()
                .AddRoleValidator<RoleValidator<Role>>()
                .AddRoleManager<RoleManager<Role>>()
                .AddSignInManager<SignInManager<User>>()

                .AddDefaultTokenProviders();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            var mappingConfig = new MapperConfiguration(mc => mc.AddProfile(new AutoMapperProfile()));

            IMapper mapper = mappingConfig.CreateMapper();

            services.AddSingleton(mapper);

            services.AddCors();
            services.AddMvc().AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplicationAPI v1"));
            }

            app.UseCors(opt => opt.AllowAnyOrigin().AllowAnyMethod().AllowAnyOrigin());

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
