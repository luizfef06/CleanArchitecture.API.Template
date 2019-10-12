using Api.Common.Core.Authentication;
using Api.Common.WebServer.Server;
using Api.Template.ApplicationService.InjectionModules;
using Api.Template.DbEFCore.Repositories;
using Api.Template.Domain.InjectionModules;
using Api.Template.Infrastructure.InjectionModules;
using Autofac;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Api.Template.WebApi.Server
{
    public class Startup
    {
        public Startup(
            IConfiguration configuration,
            IHostingEnvironment environment)
        {
            Configuration = configuration;

            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(this.Configuration)
                .CreateLogger();

            Log.Information($"Starting up: {Assembly.GetEntryAssembly().GetName()}");
            Log.Information($"Environment: {environment.EnvironmentName}");

        }

        private IConfiguration Configuration { get; }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            // IoC Container Module Registration
            builder.RegisterModule(new IoCModuleApplicationService());
            builder.RegisterModule(new IoCModuleInfrastructure());
            builder.RegisterModule(new IoCAutoMapperModule());
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc(options => options.Filters.Add(typeof(ValidateModelAttribute)))
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                });

            var connectionStringApp = Configuration.GetConnectionString("ApiDbConnection");

            services.AddDbContext<EfCoreDbContext>(options => { options.UseCosmosSql("https://vet-dev1.documents.azure.com:443/", "QORSHEFMH7p64PfmelgKDqh3MHuqI8we0OlQeuYcqafnUudoYyO0ZzUpoHbkrofNMOk0Jo9lFn0LF4eKtL3YNA==", "vet-dev1"); });

            //Config Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Api.Template.WebApi", Version = "v1" });

                c.AddSecurityDefinition("Bearer",
                    new ApiKeyScheme
                    {
                        In = "header",
                        Description = "Please insert JWT with Bearer into field",
                        Name = "Authorization",
                        Type = "apiKey"
                    });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", new string[] { } }
                });
            });

            services.AddCors(config =>
            {
                var policy = new CorsPolicy();
                policy.Headers.Add("*");
                policy.Methods.Add("*");
                policy.Origins.Add("*");
                policy.SupportsCredentials = true;
                config.AddPolicy("policy", policy);
            });

            var signingConfigurations = new SigningConfigurations();
            services.AddSingleton(signingConfigurations);

            var tokenConfigurations = new TokenConfigurations();
            services.AddSingleton(tokenConfigurations);

            services.AddAuthentication(
                authOptions =>
                {
                    authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudience = tokenConfigurations.Audience,
                        ValidIssuer = tokenConfigurations.Issuer,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingConfigurations.Key,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build());
            });

            services.AddMemoryCache();
            services.AddApplicationInsightsTelemetry();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseMiddleware<ApiResponseMiddleware>();
            loggerFactory.AddSerilog();
            app.UseMiddleware<SerilogMiddleware>();


            app.UseCors(builder =>
                builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            app.UseSwagger(c => c.RouteTemplate = "swagger/{documentName}/swagger.json");
            app.UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "Api.Template.WebApi Documentation"));
            app.UseAuthentication();
            app.UseMvc();

            //Execute Database Migration
            UpdateDatabaseUsingEfCore(app);
        }

        private async void UpdateDatabaseUsingEfCore(IApplicationBuilder app)
        {
            Log.Information("Starting: Database Migration");
            try
            {
                var service = app.ApplicationServices.GetService<EfCoreDbContext>();
                bool created = await service.CreateTheDatabaseAsync();
                if (created)
                    Log.Information("Created!");
                else
                    Log.Information("Already exists!");
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
            Log.Information("Ending: Database Migration");
        }
    }
}