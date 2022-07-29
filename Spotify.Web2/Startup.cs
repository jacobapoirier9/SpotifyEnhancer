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
using ServiceStack.Text;
using Spotify.Library;
using Spotify.Library.Core;
using Spotify.Library.Services;
using Spotify.Web.Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Spotify.Web
{
    public class Startup
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private static void AddSqlLogger()
        {
            OrmLiteConfig.BeforeExecFilter = sqlCmd =>
            {
                var builder = new System.Text.StringBuilder();

                builder.AppendLine();
                builder.AppendLine(sqlCmd.Connection.ConnectionString);

                foreach (SqlParameter parm in sqlCmd.Parameters)
                {
                    // Determine the raw sql value
                    var sqlValue = parm.SqlDbType switch
                    {
                        SqlDbType.VarChar or SqlDbType.NVarChar or SqlDbType.DateTime
                        => $"'{parm.Value}'",

                        _ => parm.Value
                    };

                    // Determine the datatype (apply overrides)
                    var sqlDbType = parm.SqlDbType switch
                    {
                        SqlDbType.VarChar => $"{SqlDbType.NVarChar}(1000)",

                        _ => parm.SqlDbType.ToString()
                    };

                    builder.AppendLine($"declare {parm.ParameterName} {sqlDbType} = {sqlValue}");
                }

                builder.AppendLine();
                builder.AppendLine(sqlCmd.CommandText);
                builder.AppendLine();

                _logger.Trace("Running SQL: {SQL}", builder.ToString());
            };
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _logger.Debug("Configuring Services..");

            services.AddRazorPages();
            services.AddControllersWithViews()
                #if DEBUG
                .AddRazorRuntimeCompilation()
                #endif
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
            if (_configuration.GetValue<bool>("TraceLogging:Sql"))
            {
                AddSqlLogger();
            }

            services.AddSingleton<ICustomCache, CustomFileSystemCache>();
            services.AddSingleton<ISpotifyTokenService, SpotifyTokenService>();

            JsConfig.TextCase = TextCase.SnakeCase;
            services.AddSingleton<IServiceClient>(new JsonServiceClient
            {
                BaseUri = _configuration.GetValue<string>("Spotify:ApiUri"),
                ResponseFilter =_configuration.GetValue<bool>("TraceLogging:ApiDirtyReads") ? LogApiResponseDirty : LogApiResponse,
                ExceptionFilter = LogApiException
            });
        }

        private ExceptionFilterDelegate LogApiException(WebException exception, WebResponse response, string str, Type type)
        {
            LogApiResponse((HttpWebResponse)response);
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

        private void LogApiResponse(HttpWebResponse response)
        {
            _logger.Trace("{StatusCode} {StatusDescription}: {RequestUri}", (int)response.StatusCode, response.StatusDescription, response.ResponseUri);
        }

        private void LogApiResponseDirty(HttpWebResponse response)
        {
            LogApiResponse(response);
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                _logger.Trace(reader.ReadToEnd());
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
                app.UseExceptionHandler("/Error");
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            _logger.Debug("URLs: {URLs}", app.ApplicationServices.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>().Addresses.JoinToString(", "));
        }
    }
}
