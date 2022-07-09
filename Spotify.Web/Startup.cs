using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spotify.Library;
using Spotify.Library.Core;
using Spotify.Library.Services;
using Spotify.Web.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
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
            services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.Converters.Add(new ItemTypeJsonConverter());
                });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = _configuration["Auth:Cookie:Name"];

                    options.LoginPath = "/Auth/Login";
                    options.AccessDeniedPath = "/Auth/Denied";

                    options.Cookie.MaxAge = TimeSpan.FromHours(1);

                    //options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.Cookie.SameSite = SameSiteMode.Lax;
                });


            services.AddAuthorization();

            services.AddSingleton<IDbConnectionFactory>(new OrmLiteConnectionFactory(_configuration["ConnectionStrings:SteamRoller"], SqlServerDialect.Provider));
            if (_configuration.GetValue<bool>("DeepLogging:SQL"))
            {
                OrmLiteConfig.BeforeExecFilter = sqlCmd =>
                {
                    var builder = new System.Text.StringBuilder();
                    builder.AppendLine();
                    builder.AppendLine(sqlCmd.Connection.ConnectionString);
                    foreach (SqlParameter parm in sqlCmd.Parameters)
                    {
                        builder.AppendLine($"declare {parm.ParameterName} {parm.SqlDbType} = {(parm.SqlDbType == System.Data.SqlDbType.DateTime ? "\"" + parm.Value + "\"" : parm.Value)}");
                    }
                    builder.AppendLine();
                    builder.AppendLine(sqlCmd.CommandText);
                    builder.AppendLine();
                    Console.WriteLine(builder.ToString());
                    _logger.Trace("Running SQL: {NewLine} {SQL}", Environment.NewLine, builder.ToString());
                };
            }

            services.AddSingleton<ICustomCache, CustomFileSystemCache>();
            services.AddSingleton<ISpotifyTokenService, SpotifyTokenService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();


            services.AddSingleton<IServiceClient>(new JsonServiceClient
            {
                BaseUri = _configuration.GetValue<string>("Spotify:ApiUri"),

                // Moving all the logic here, so that all settings are read at start up, rather than with each Spotify request.
                ResponseFilter = 
                    _configuration.GetValue<bool>("DeepLogging:API") ?
                        (
                            _configuration.GetValue<bool>("DeepLogging:ReadApiStream") ?
                                (response) => 
                                {
                                    _logger.Trace("{StatusCode} {StatusDescription}: {RequestUri}", (int)response.StatusCode, response.StatusDescription, response.ResponseUri);
                                    using (var stream = response.GetResponseStream())
                                    using (var reader = new StreamReader(stream))
                                    {
                                        _logger.Trace(reader.ReadToEnd());
                                    }
                                } :
                                (response) => 
                                {
                                    _logger.Trace("{StatusCode} {StatusDescription}: {RequestUri}", (int)response.StatusCode, response.StatusDescription, response.ResponseUri);
                                }
                        ) :  
                        null,
                ExceptionFilter = (exception, response, str, type) =>
                {
                    var httpResponse = (HttpWebResponse)response;
                    _logger.Trace("{StatusCode} {StatusDescription}: {RequestUri}", (int)httpResponse.StatusCode, httpResponse.StatusDescription, httpResponse.ResponseUri);

                    _logger.Error("HTTP: {Error}", exception.Message);

                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();

                        if (json is null) // An error occurred before reaching the endpoint (HTTP Error)
                        {
                            throw exception;
                        }
                        else // An error occurred after reaching the endpoint (Spotify Error)
                        {
                            var error = json.FromJson<ErrorWrapper>();
                            _logger.Error("Spotify: {Error}", error.Error.Message);
                            throw new Exception(error.Error.Message);
                        }
                    }
                }
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
