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
using Spotify.Library.Core;
using Spotify.Library.Services;
using Spotify.Web.Services;
using System;
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

            services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddCssBundle("/css/bundles.css", new string[]
                {
                    "/css/bootstrap.min.css",
                    "/css/bootstrap-select.css",
                    "/css/bootstrap-datepicker3.min.css",
                    "/css/font-awesome.min.css",
                    "/css/icheck/blue.css",
                    "/css/AdminLTE.css",
                    "/css/skins/skin-blue.css",
                    "/lib/jqgrid/ui.jqgrid-bootstrap.css",
                    "/css/site.css"
                });
                pipeline.AddJavaScriptBundle("/js/bundles.js", new string[]
                {
                    "/lib/jquery/dist/jquery-3.3.1.js",
                    "/lib/bootstrap/dist/bootstrap.min.js",
                    "/lib/bootstrap-select/dist/bootstrap-select.js",
                    "/lib/fastclick/dist/fastclick.js",
                    "/lib/slimscroll/dist/jquery.slimscroll.js",
                    "/lib/moment/dist/moment.js",
                    "/lib/datepicker/dist/bootstrap-datepicker.js",
                    "/lib/icheck/dist/icheck.js",
                    "/lib/validator/dist/validator.js",
                    "/lib/inputmask/dist/jquery.inputmask.bundle.js",
                    "/lib/jqgrid/grid.locale-en.js",
                    "/lib/jqgrid/jquery.jqGrid.js",
                    "/js/adminlte.js",
                    "/js/init.js",
                    "/js/config.js",
                    "/js/router.js",
                    "/js/helpers.js",
                    "/js/spotify.js"
                });
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
                                }
                :
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
            app.UseWebOptimizer();
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
