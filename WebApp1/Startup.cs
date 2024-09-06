using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SSO.DataModels;
using SSO.Services;
using SSOIdentity;
using System;

namespace WebApp1
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
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.Cookie = new CookieBuilder
                {
                    HttpOnly = true,
                    Name = ".webapp1.login.auttoken",
                    SameSite = SameSiteMode.None,
                    SecurePolicy = CookieSecurePolicy.SameAsRequest
                };

                options.LoginPath = new PathString("/account/login");
                options.LogoutPath = new PathString("/account/logout");
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });

            services.AddRouting(options => options.LowercaseUrls = true);

            MyAppSettings myAppSettings = new MyAppSettings();
            new ConfigureFromConfigurationOptions<MyAppSettings>(Configuration.GetSection("MyAppSettings")).Configure(myAppSettings);
            services.AddSingleton(myAppSettings);

            services.AddScoped<IToken, Token>();
            services.AddScoped<IAccountService, AccountService>();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            var cookiePolicyOptions = new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.None,
            };
            app.UseCookiePolicy(cookiePolicyOptions);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
