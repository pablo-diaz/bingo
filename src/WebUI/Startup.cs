using System.Linq;
using System.Threading.Tasks;

using WebUI.Services;
using WebUI.ViewModels;
using WebUI.Infrastructure;

using Blazored.Toast;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.IdentityModel.Tokens;

namespace WebUI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddBlazoredToast();

            services.AddSingleton<BingoHub>();
            services.AddSingleton<BingoSecurity>();
            services.AddSingleton<GameApplication>();

            services.AddTransient<GameAdmonViewModel>();
            services.AddTransient<GamePlayerViewModel>();

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                var signingKey = this.Configuration["Bingo.Security:JWTSigningKey"];

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(signingKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context => {
                        var accessTokenProvided = context.Request.Headers["Authorization"];
                        var requestedPath = context.HttpContext.Request.Path;
                        var shouldBingoHubRequestBeAuthorized = 
                            !string.IsNullOrEmpty(accessTokenProvided) 
                            && requestedPath.StartsWithSegments("/bingoHub");
                        if (shouldBingoHubRequestBeAuthorized)
                        {
                            var onlyTokenValue = accessTokenProvided.ToString().Replace("Bearer ", "");
                            context.Token = onlyTokenValue;
                        }
                        return Task.CompletedTask;
                    },
                    
                    /*OnAuthenticationFailed = context => {
                        return Task.CompletedTask;
                    }*/
                };
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapHub<BingoHub>("/bingoHub");
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
