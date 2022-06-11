using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spotify.Library;
using Spotify.Library.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Spotify.Web
{
    public class Startup
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        public IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }




        public void ConfigureServices(IServiceCollection services)
        {
            _logger.Debug("Configuring Services..");
            services.AddControllersWithViews();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = _configuration["Auth:Cookie:Name"];

                    options.LoginPath = "/Auth/Login";
                    options.AccessDeniedPath = "/Auth/Denied";

                    options.Cookie.MaxAge = TimeSpan.FromHours(1);

                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.Cookie.SameSite = SameSiteMode.Lax;
                });


            services.AddAuthorization();

            services.AddSingleton<IDbConnectionFactory>(new OrmLiteConnectionFactory(_configuration["ConnectionStrings:SteamRoller"], SqlServerDialect.Provider));
            if (_configuration.GetValue<bool>("DeepLogging:SQL"))
            {
                OrmLiteConfig.BeforeExecFilter = command =>
                {
                    _logger.Trace("Running SQL: {NewLine} {SQL}", Environment.NewLine, command.CommandText);
                };
            }

            services.AddSingleton<ISpotifyTokenService, SpotifyTokenService>();
            services.AddSingleton<IServiceClient>(new JsonServiceClient
            {
                BaseUri = _configuration.Get<string>("Spotify:ApiUri")
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _logger.Debug("Configuring application..");
            _logger.Debug("Environment: {Environment}", env.EnvironmentName);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            _logger.Debug("URLs: {URLs}", app.ApplicationServices.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>().Addresses.JoinToString(", "));
        }
    }
}
