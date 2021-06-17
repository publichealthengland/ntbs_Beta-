using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EFAuditer;
using Hangfire;
using Hangfire.Console;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ntbs_service.Authentication;
using ntbs_service.DataAccess;
using ntbs_service.DataMigration;
using ntbs_service.Helpers;
using ntbs_service.Jobs;
using ntbs_service.Middleware;
using ntbs_service.Models;
using ntbs_service.Models.Entities;
using ntbs_service.Properties;
using ntbs_service.Services;
using Serilog;
using Serilog.Events;
using ZNetCS.AspNetCore.Authentication.Basic;
using ZNetCS.AspNetCore.Authentication.Basic.Events;

namespace ntbs_service
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        private IConfiguration Configuration { get; }
        private IWebHostEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // This was helpful for identifying issues with ADFS login - but shouldn't be on usually
            // IdentityModelEventSource.ShowPII = true;
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // Configuration
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var adConfig = Configuration.GetSection("AdOptions");
            var adfsConfig = Configuration.GetSection("AdfsOptions");
            var azureAdConfig = Configuration.GetSection("AzureAdOptions");

            var adOptions = new AdOptions();
            var adfsOptions = new AdfsOptions();
            var azureAdOptions = new AzureAdOptions();

            adConfig.Bind(adOptions);
            adfsConfig.Bind(adfsOptions);
            azureAdConfig.Bind(azureAdOptions);

            services.Configure<AdOptions>(adConfig);
            services.Configure<AdfsOptions>(adfsConfig);
            services.Configure<AzureAdOptions>(azureAdConfig);
            services.Configure<LdapSettings>(Configuration.GetSection("LdapSettings"));
            services.Configure<MigrationConfig>(Configuration.GetSection("MigrationConfig"));

            // Plugin services
            if (Env.IsEnvironment("CI"))
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddDistributedSqlServerCache(options =>
                {
                    options.ConnectionString = Configuration.GetConnectionString("ntbsContext");
                    options.SchemaName = "dbo";
                    options.TableName = "SessionState";
                });
            }

            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
            });

            // select authentication method
            var httpBasicAuthConfig = Configuration.GetSection("HttpBasicAuth");
            var basicAuthEnabled = httpBasicAuthConfig.GetValue("Enabled", false);
            var azureAdAuthEnabled = azureAdOptions.Enabled;

            Log.Information($"Basic Auth Enabled: {basicAuthEnabled}");
            Log.Information($"Azure Ad Auth Enabled: {azureAdAuthEnabled}");

            var baseUserGroupRole = adOptions.BaseUserGroup;

            if (adOptions.UseDummyAuth)
            {
                UseDummyAuth(services);
            }
            else if (basicAuthEnabled)
            {
                UseHttpBasicAuth(services, httpBasicAuthConfig, adOptions);
            }
            else if (azureAdAuthEnabled)
            {
                UseAzureAdAuthentication(services, adOptions, azureAdOptions);
            }
            else
            {
                UseAdfsAuthentication(services, adOptions, adfsOptions);
            }

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireRole(baseUserGroupRole)
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddRazorPages(options =>
            {
                options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
                options.Conventions.AllowAnonymousToPage("/Logout");
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireRole(GetAdminRoleName());
                });
            });
            SetupHangfire(services);

            // DB Contexts
            services.AddDbContext<NtbsContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ntbsContext"))
            );

            services.AddSingleton<NtbsContextDesignTimeFactory>();

            var auditDbConnectionString = Configuration.GetConnectionString("auditContext");

            services.AddDbContext<AuditDatabaseContext>(options =>
                options.UseSqlServer(auditDbConnectionString)
            );

            // Add a DbContext for Data Protection key storage
            services.AddDbContext<KeysContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("keysContext")));
            services.AddDataProtection().PersistKeysToDbContext<KeysContext>();

            // Repositories
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();
            services.AddScoped<IAlertRepository, AlertRepository>();
            services.AddScoped<INotificationImportRepository, NotificationImportRepository>();
            services.AddScoped<IMigratedNotificationsMarker, MigratedNotificationsMarker>();
            services.AddScoped<INotificationImportHelper, NotificationImportHelper>();
            services.AddScoped<IMigrationRepository, MigrationRepository>();
            services.AddScoped<IItemRepository<ManualTestResult>, TestResultRepository>();
            services.AddScoped<IItemRepository<SocialContextVenue>, SocialContextVenueRepository>();
            services.AddScoped<IItemRepository<SocialContextAddress>, SocialContextAddressRepository>();
            services.AddScoped<ITreatmentEventRepository, TreatmentEventRepository>();
            services.AddScoped<IItemRepository<MBovisExposureToKnownCase>, MBovisExposureToKnownCaseRepository>();
            services.AddScoped<IItemRepository<MBovisUnpasteurisedMilkConsumption>, MBovisUnpasteurisedMilkConsumptionRepository>();
            services.AddScoped<IItemRepository<MBovisOccupationExposure>, MBovisOccupationExposureRepository>();
            services.AddScoped<IItemRepository<MBovisAnimalExposure>, MBovisAnimalExposureRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDataQualityRepository, DataQualityRepository>();
            services.AddScoped<IDrugResistanceProfileRepository, DrugResistanceProfileRepository>();
            services.AddScoped<IFaqRepository, FaqRepository>();

            // Services
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAlertService, AlertService>();
            services.AddScoped<INotificationMapper, NotificationMapper>();
            services.AddScoped<IImportLogger, ImportLogger>();
            services.AddScoped<INotificationImportService, NotificationImportService>();
            services.AddScoped<IImportValidator, ImportValidator>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<INotificationChangesService, NotificationChangesService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPostcodeService, PostcodeService>();
            services.AddScoped<Services.IAuthorizationService, AuthorizationService>();
            services.AddScoped<ILegacySearchService, LegacySearchService>();
            services.AddScoped<IAdDirectoryServiceFactory, AdDirectoryServiceFactory>();
            services.AddScoped<IEnhancedSurveillanceAlertsService, EnhancedSurveillanceAlertsService>();
            services.AddScoped<IUserSearchService, UserSearchService>();
            services.AddScoped<IClusterImportService, ClusterImportService>();
            services.AddScoped<ISpecimenImportService, SpecimenImportService>();
            services.AddScoped<ICaseManagerImportService, CaseManagerImportService>();
            services.AddScoped<IAdUserService, AdUserService>();
            services.AddScoped<IReportingLinksService, ReportingLinksService>();
            services.AddScoped<ITreatmentEventMapper, TreatmentEventMapper>();
            services.AddScoped<IUserHelper, UserHelper>();
            services.AddScoped<ILogService, LogService>();

            AddAuditService(services, auditDbConnectionString);
            AddReferenceLabResultServices(services);
            AddNotificationClusterRepository(services);
            AddReportingServices(services);
            AddMicrosoftGraphServices(services, azureAdOptions);
            AddAdImportService(services, azureAdOptions);

        }

        private void SetupHangfire(IServiceCollection services)
        {
            if (Configuration.GetValue<bool>(Constants.HangfireEnabled))
            {
                services.AddHangfire(config =>
                {
                    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseSqlServerStorage(Configuration.GetConnectionString("ntbsContext"),
                            new SqlServerStorageOptions
                            {
                                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                                QueuePollInterval = TimeSpan.Zero,
                                UseRecommendedIsolationLevel = true,
                                UsePageLocksOnDequeue = true,
                                DisableGlobalLocks = true
                            })
                        .UseSerilogLogProvider()
                        .UseConsole();
                });
            }
        }

        private static void UseDummyAuth(IServiceCollection services)
        {
            var authSetup = services.AddAuthentication(DummyAuthHandler.Name);
            authSetup.AddScheme<AuthenticationSchemeOptions, DummyAuthHandler>(DummyAuthHandler.Name, o => { });
        }

        private static void UseAdfsAuthentication(IServiceCollection services, AdOptions adOptions, AdfsOptions adfsOptions)
        {
            var authSetup = services
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
                })
                .AddWsFederation(options =>
                {
                    options.CallbackPath = "/Index";
                    options.SkipUnrecognizedRequests = true;
                    options.MetadataAddress = adfsOptions.AdfsUrl + "/FederationMetadata/2007-06/FederationMetadata.xml";
                    options.Wtrealm = adfsOptions.Wtrealm;
                    options.CorrelationCookie.SameSite = SameSiteMode.None;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                    /*
                     * Below event handler is to prevent stale logins from showing a 500 error screen, instead to force
                     * back to the landing page - and cause a re-challenge or continue if already authenticated.
                     * https://community.auth0.com/t/asp-net-core-2-intermittent-correlation-failed-errors/11918/14
                     */
                    options.Events.OnRemoteFailure += context =>
                    {
                        if (context.Failure.Message == "Correlation failed.")
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/");
                        }

                        return Task.CompletedTask;
                    };

                    options.Events.OnSecurityTokenValidated += async context =>
                    {
                        var username = context.Principal.Username();
                        if (username != null)
                        {
                            var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                            await userService.RecordUserLoginAsync(username);
                        }
                    };
                })
                .AddCookie(options =>
                {
                    options.ForwardAuthenticate = null;
                    options.SlidingExpiration = false;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(adOptions.MaxSessionCookieLifetimeInMinutes);
                });
        }

        private static void UseAzureAdAuthentication(IServiceCollection services, AdOptions adOptions, AzureAdOptions azureAdOptions)
        {
            var authSetup = services
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultAuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.ForwardAuthenticate = null;
                    options.SlidingExpiration = false;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(adOptions.MaxSessionCookieLifetimeInMinutes);
                })
                .AddOpenIdConnect(options =>
                {
                    options.ClientId = azureAdOptions.ClientId;
                    options.ClientSecret = azureAdOptions.ClientSecret;
                    options.Authority = azureAdOptions.Authority;
                    options.CallbackPath = azureAdOptions.CallbackPath;
                    options.CorrelationCookie.SameSite = SameSiteMode.None;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;

                    options.Events = new OpenIdConnectEvents();
                    options.Events.OnTokenValidated += async context =>
                    {
                        var username = context.Principal.Username();
                        if (username == null)
                        {
                            username = context.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                        }

                        // add group claims.
                        if (username != null)
                        {

                            var claims = new List<Claim>();
                            var azureAdFactory = context.HttpContext.RequestServices.GetRequiredService<IAzureAdDirectoryServiceFactory>();
                            var azureAdDirectoryService = azureAdFactory.Create();

                            var groupClaims = await azureAdDirectoryService.BuildRoleClaimsForUser(username);
                            claims.AddRange(groupClaims);

                            var upnClaim = new Claim(ClaimTypes.Upn, username);
                            claims.Add(upnClaim);

                            var applicationClaimsIdentity = new ClaimsIdentity(claims);
                            context.Principal.AddIdentity(applicationClaimsIdentity);
                        }

                        if (username != null)
                        {
                            var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                            await userService.RecordUserLoginAsync(username);
                        }
                    };

                    /*
                     * Below event handler is to prevent stale logins from showing a 500 error screen, instead to force
                     * back to the landing page - and cause a re-challenge or continue if already authenticated.
                     * https://community.auth0.com/t/asp-net-core-2-intermittent-correlation-failed-errors/11918/14
                     */
                    options.Events.OnRemoteFailure += context =>
                    {
                        if (context.Failure.Message == "Correlation failed.")
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/");
                        }

                        return Task.CompletedTask;
                    };

                });
        }

        private static void UseHttpBasicAuth(IServiceCollection services,
            IConfigurationSection httpBasicAuthConfig,
            AdOptions adOptions)
        {
            services
                .AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                .AddBasicAuthentication(
                    options =>
                    {
                        options.Realm = "Basic Realm";
                        options.Events = new BasicAuthenticationEvents
                        {
                            OnValidatePrincipal = context =>
                            {
                                if ((context.UserName.ToLower() == httpBasicAuthConfig["Username"].ToLower())
                                    && (context.Password == httpBasicAuthConfig["Password"]))
                                {
                                    var groupAdmin = adOptions.AdminUserGroup;
                                    var groupDev = adOptions.DevGroup ?? adOptions.NationalTeamAdGroup;
                                    var claims = new List<Claim>
                                    {
                                        new Claim(ClaimTypes.Name,
                                            context.UserName,
                                            context.Options.ClaimsIssuer),
                                        new Claim(ClaimTypes.Role, adOptions.BaseUserGroup, ClaimValueTypes.String),

                                        new Claim(ClaimTypes.Role, groupAdmin, ClaimValueTypes.String),
                                        new Claim(ClaimTypes.Role, groupDev, ClaimValueTypes.String)
                                    };

                                    var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
                                        claims,
                                        BasicAuthenticationDefaults.AuthenticationScheme));
                                    var ticket = new AuthenticationTicket(
                                        claimsPrincipal,
                                        new AuthenticationProperties(),
                                        BasicAuthenticationDefaults.AuthenticationScheme);

                                    context.Principal = claimsPrincipal;

                                    // Returning the ticket, though it doesn't seem to be used (at least in the initial
                                    // logic) - hence setting the Principal above, too.
                                    return Task.FromResult(AuthenticateResult.Success(ticket));
                                }

                                return Task.FromResult(AuthenticateResult.Fail("Authentication  really failed."));
                            }
                        };
                    });
        }

        private void AddAuditService(IServiceCollection services, string auditDbConnectionString)
        {
            if (Configuration.GetValue<bool>(Constants.AuditEnabledConfigValue))
            {
                services.AddEFAuditer(auditDbConnectionString);
            }
            else
            {
                Audit.Core.Configuration.AuditDisabled = true;
            }
        }

        private void AddAdImportService(IServiceCollection services, AzureAdOptions azureAdOptions)
        {
            if (azureAdOptions.Enabled)
            {
                services.AddScoped<IAdImportService, AzureAdImportService>();
            }
            else
            {
                services.AddScoped<IAdImportService, AdImportService>();
            }

        }

        private void AddNotificationClusterRepository(IServiceCollection services)
        {
            var clusterMatchingConfig = Configuration.GetSection(Constants.ClusterMatchingConfig);
            if (clusterMatchingConfig.GetValue<bool>(Constants.ClusterMatchingConfigMockOut))
            {
                var notificationClusterValues = new List<NotificationClusterValue>();
                clusterMatchingConfig.Bind("MockedNotificationClusterValues", notificationClusterValues);

                services.AddScoped<INotificationClusterRepository>(sp =>
                    new MockNotificationClusterRepository(notificationClusterValues));
            }
            else
            {
                services.AddScoped<INotificationClusterRepository, NotificationClusterRepository>();
            }
        }

        private void AddReferenceLabResultServices(IServiceCollection services)
        {
            var referenceLabResultsConfig = Configuration.GetSection(Constants.ReferenceLabResultsConfig);
            if (referenceLabResultsConfig.GetValue<bool>(Constants.ReferenceLabResultsConfigMockOut))
            {
                var notificationId = referenceLabResultsConfig.GetValue<int>("MockedNotificationId");
                var tbServiceCode = referenceLabResultsConfig.GetValue<string>("MockedTbServiceCode");
                var phecCode = referenceLabResultsConfig.GetValue<string>("MockedPhecCode");
                services.AddScoped<ICultureAndResistanceService>(
                    sp => new MockCultureAndResistanceService(notificationId));
                services.AddScoped<ISpecimenService>(
                    sp => new MockSpecimenService(notificationId, tbServiceCode, phecCode));
            }
            else
            {
                services.AddScoped<ICultureAndResistanceService, CultureAndResistanceService>();
                services.AddScoped<ISpecimenService, SpecimenService>();
            }
        }

        private void AddReportingServices(IServiceCollection services)
        {
            if (string.IsNullOrEmpty(Configuration.GetConnectionString(Constants.DbConnectionStringReporting)))
            {
                services.AddScoped<IHomepageKpiService, MockHomepageKpiService>();
                services.AddScoped<IDrugResistanceProfileService, MockDrugResistanceProfileService>();
            }
            else
            {
                services.AddScoped<IHomepageKpiService, HomepageKpiService>();
                services.AddScoped<IDrugResistanceProfileService, DrugResistanceProfileService>();
            }
        }

        private void AddMicrosoftGraphServices(IServiceCollection services, AzureAdOptions azureAdOptions)
        {
            if (azureAdOptions.Enabled)
            {
                services.AddScoped<IAzureAdDirectoryServiceFactory, AzureAdDirectoryServiceFactory>();
                services.AddScoped<IAzureAdDirectoryService, AzureAdDirectoryService>();
            }
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseForwardedHeaders();
                app.UseDeveloperExceptionPage();

                // TODO Find an alternative for using webpack middleware in dotnet 5.0
                // app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                // {
                //     HotModuleReplacement = true, ConfigFile = "webpack.dev.js"
                // });
                // We only need to turn this on in development, as in production this
                // This behaviour is by default provided by the nginx ingress
                // (see https://kubernetes.github.io/ingress-nginx/user-guide/nginx-configuration/annotations/#server-side-https-enforcement-through-redirect)
                // (also see  HSTS setting below)
                app.UseHttpsRedirection();

            }
            else
            {
                app.UseForwardedHeaders();
                app.UseStatusCodePagesWithReExecute("/errors/{0}");
                app.UseExceptionHandler("/errors/500");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();

            if (!Env.IsEnvironment("CI"))
            {
                /*
                Making this conditional is the result of serilog not playing nicely with WebApplicationFactory
                used by the ui tests, see: https://github.com/serilog/serilog-aspnetcore/issues/105
                Using env directly as check is an unsatisfying solution, but configuration values were not picked up consistently correctly here.
                */
                app.UseSerilogRequestLogging(options => // Needs to be before MVC handlers
                {
                    options.MessageTemplate =
                        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms ({RequestId})";
                    // 400s get thrown e.g. on antiforgery token validation failures. In those cases we don't have
                    // an exception logged in Sentry, so we want to log at Warning level to make sure we are able to
                    // identify and cure false positives.
                    // Otherwise setting to Information to prevent duplicated exceptions in sentry.
                    options.GetLevel = (context, _, __) => context.Response.StatusCode == StatusCodes.Status400BadRequest
                        ? LogEventLevel.Warning
                        : LogEventLevel.Information;
                });
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCookiePolicy();
            app.UseSession();

            if (!Env.IsEnvironment("CI"))
            {
                app.UseMiddleware<ActivityDetectionMiddleware>();
            }

            if (Configuration.GetValue<bool>(Constants.AuditEnabledConfigValue))
            {
                app.UseMiddleware<AuditGetRequestMiddleWare>();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

            ConfigureHangfire(app);

            var cultureInfo = new CultureInfo("en-GB");
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        }

        private void ConfigureHangfire(IApplicationBuilder app)
        {
            if (!Configuration.GetValue<bool>(Constants.HangfireEnabled))
            {
                return;
            }

            var dashboardOptions = new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorisationFilter(GetAdminRoleName()) },
                DisplayStorageConnectionString = false,
            };
            app.UseHangfireDashboard("/hangfire", dashboardOptions);
            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                WorkerCount = Configuration.GetValue<int>(Constants.HangfireWorkerCount)
            });
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });

            var scheduledJobConfig = new ScheduledJobsConfig();
            Configuration.GetSection(Constants.ScheduledJobsConfig).Bind(scheduledJobConfig);
            HangfireJobScheduler.ScheduleRecurringJobs(scheduledJobConfig);
        }

        private string GetAdminRoleName()
        {
            var adConfig = Configuration.GetSection("AdOptions");
            return adConfig["AdminUserGroup"];
        }
    }
}
