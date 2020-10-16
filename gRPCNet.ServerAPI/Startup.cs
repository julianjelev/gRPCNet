using gRPCNet.ServerAPI.gRPCServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Server;
using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace gRPCNet.ServerAPI
{
    //Creating And Validating JWT Tokens In ASP.NET Core https://dotnetcoretutorials.com/2020/01/15/creating-and-validating-jwt-tokens-in-asp-net-core/
    //A look behind the JWT bearer authentication middleware in ASP.NET Core https://andrewlock.net/a-look-behind-the-jwt-bearer-authentication-middleware-in-asp-net-core/
    //JWT Validation and Authorization in ASP.NET Core https://devblogs.microsoft.com/aspnet/jwt-validation-and-authorization-in-asp-net-core/
    //Token Authentication in ASP.NET Core 2.0 - A Complete Guide https://developer.okta.com/blog/2018/03/23/token-authentication-aspnetcore-complete-guide 
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IFileLogger _fileLogger;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory, IFileLogger fileLogger) 
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<Startup>();
            _fileLogger = fileLogger;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCodeFirstGrpc(options => 
            {
                //Configure services options (https://docs.microsoft.com/en-us/aspnet/core/grpc/configuration?view=aspnetcore-3.1)
                options.EnableDetailedErrors = true;//def false
                options.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
            });
            //services.AddCodeFirstGrpcReflection();

            var sharedKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JwtBearerOptions:SharedKey").Get<string>()));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => 
            {
                options.TokenValidationParameters = new TokenValidationParameters {
                    // Clock skew compensates for server time drift.
                    // We recommend 5 minutes or less:
                    ClockSkew = TimeSpan.FromMinutes(5),
                    
                    // Specify the key used to sign the token:
                    RequireSignedTokens = true,
                    IssuerSigningKey = sharedKey,
                    ValidateIssuerSigningKey = true,

                    // Ensure the token hasn't expired:
                    RequireExpirationTime = true,
                    ValidateLifetime = true,

                    // Ensure the token audience (aud parameter in the token) matches our audience value (default true):
                    ValidateAudience = true,
                    ValidAudience = _configuration.GetSection("JwtBearerOptions:ValidAudience").Get<string>(),

                    // Ensure the token issuer (iss parameter in the token) matches our issuer value (default true):
                    ValidateIssuer = true,
                    ValidIssuer = _configuration.GetSection("JwtBearerOptions:ValidIssuer").Get<string>()
                };
            });
            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);

            app.UseRouting();

            //Authenticate users calling a gRPC service
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<KeepaliveService>();
                endpoints.MapPost("/oauth/token", async context => 
                {
                    if (!context.Request.Headers.ContainsKey("Authorization")) 
                    {
                        _logger.LogWarning("Authorization failed. No Authorization header");
                        _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Authorization failed. No Authorization header");
                        
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync("");
                    }
                    try 
                    {
                        var authHeader = AuthenticationHeaderValue.Parse(context.Request.Headers["Authorization"]);
                        var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':', 2);
                        if (Authenticate(credentials[0], credentials[1]))
                        {
                            var token = GenerateToken(credentials[0]);

                            _logger.LogInformation($"Authorization pass. Sent JWT token {token}");
                            _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Authorization pass. Sent JWT token {token}");

                            context.Response.ContentType = "text/plain";
                            await context.Response.WriteAsync(token);
                        }
                        else
                        {
                            _logger.LogWarning($"Authorization failed. Unresolved credentials for client {credentials[0]} using secret {credentials[1]}");
                            _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Authorization failed. Unresolved credentials for client {credentials[0]} using secret {credentials[1]}");

                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = "text/plain";
                            await context.Response.WriteAsync("");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Authorization failed: {ex}");
                        _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Authorization failed: {ex}");

                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync("");
                    }
                });
                //endpoints.MapCodeFirstGrpcReflectionService();
            });
        }

        private bool Authenticate(string clientId, string clientSecret) 
        {
            bool isValid = false;
            var allowedClients = _configuration.GetSection("JwtBearerOptions:AllowedClients").GetChildren();
            foreach (var item in allowedClients) 
                if (item.GetValue<string>("ClientId").Equals(clientId, StringComparison.InvariantCultureIgnoreCase) && item.GetValue<string>("ClientSecret") == clientSecret)
                {
                    isValid = true;
                    break;
                }
            
            return isValid;
        }

        private string GenerateToken(string clientId) 
        {
            var sharedKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JwtBearerOptions:SharedKey").Get<string>()));
            var issuer = _configuration.GetSection("JwtBearerOptions:ValidIssuer").Get<string>();
            var audience = _configuration.GetSection("JwtBearerOptions:ValidAudience").Get<string>();

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor 
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(ClaimTypes.NameIdentifier, clientId)
                }),
                Expires = DateTime.Parse(_configuration.GetSection("JwtBearerOptions:Expires").Get<string>()),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(sharedKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        private void OnStarted()
        {
            _logger.LogInformation("Server STARTED");
            _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Server STARTED");
            // Perform post-startup activities here
        }

        private void OnStopping()
        {
            _logger.LogInformation("Server STOPPING");
            // Perform on-stopping activities here
        }

        private void OnStopped()
        {
            _logger.LogInformation("Server STOPED");
            _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Server STOPED");
            // Perform post-stopped activities here
        }
    }
}
