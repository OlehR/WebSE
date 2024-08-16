using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSE.Filters;

namespace WebSE
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        static public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            /*
            services.AddCors(options =>
            {  
               //.SetIsOriginAllowed((host) => true)

                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder //.WithOrigins("http://localhost:3000") 
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials();
                    });
            });
*/
            services.AddCors();
            services.AddScoped<ClientIPAddressFilterAttribute>();
            services.AddControllersWithViews();
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromHours(4);
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebSE", Version = "v1" });
            });

            services.Configure<IPWhitelistConfiguration>(
                Configuration.GetSection("IPAddressWhitelistConfiguration"));
            services.AddSingleton<IIPWhitelistConfiguration>(
                resolver => resolver.GetRequiredService<IOptions<IPWhitelistConfiguration>>().Value);
            services.AddMemoryCache();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // «м≥нено на SameAsRequest
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.Name = "YourCookieName";
                    options.Cookie.Path = "/";
                    options.AccessDeniedPath = "/api/login/Forbidden/";
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebSE v1");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

          

            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors(
         options => options.AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed((host) => true)
                .AllowCredentials() //WithOrigins("http://websrv.vopak.local").AllowAnyMethod()
                            );

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
