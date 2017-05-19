using FileStore.Database;
using FileStore.Models;
using FileStore.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FileStore.Middleware;

namespace FileStore
{
  public class Startup
  {
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables();

      Configuration = builder.Build();
    }

    public IConfigurationRoot Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
      var appsettingsConfig = Configuration.GetSection("AppSettings");
      ValidateAppSettings(appsettingsConfig);

      services.AddMvc();
      services.Configure<AppSettings>(appsettingsConfig)
              .AddEntityFramework()
              .AddEntityFrameworkSqlite()
              .AddDbContext<FileDbContext>(options => options.UseSqlite("Data Source=filedb.sqlite"))
              .AddTransient<FileService>();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"))
                   .AddDebug();

      InitializeDatabase(app);
      SetAuthenticationMiddleware(app);

      app.UseMvc();

    }

    private void ValidateAppSettings(IConfigurationSection appsettingsConfig)
    {
      var storagePath = appsettingsConfig.GetValue<string>("FileStoragePath");

      if (!Directory.Exists(storagePath))
        throw new InvalidOperationException("Provided FileStoragePath in appsettings does not exist");
    }

    private void InitializeDatabase(IApplicationBuilder app)
    {
      using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
      {
        var db = serviceScope.ServiceProvider.GetService<FileDbContext>();
        db.Database.EnsureCreated();
      }
    }


    //Authentication middleware code copied from https://stormpath.com/blog/token-authentication-asp-net-core
    private static readonly string secretAuthKey = "hemligthemligt123";
    private void SetAuthenticationMiddleware(IApplicationBuilder app)
    {
      var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretAuthKey));
      var options = new TokenProviderOptions
      {
        Audience = "ExampleAudience",
        Issuer = "ExampleIssuer",
        SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
        Expiration = TimeSpan.FromDays(2)
      };

      app.UseMiddleware<TokenProviderMiddleware>(Options.Create(options));

      var tokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        
        ValidateIssuer = true,
        ValidIssuer = "ExampleIssuer",
        
        ValidateAudience = true,
        ValidAudience = "ExampleAudience",
        
        ClockSkew = TimeSpan.Zero
      };

      app.UseJwtBearerAuthentication(new JwtBearerOptions
      {
        AutomaticAuthenticate = true,
        AutomaticChallenge = true,
        TokenValidationParameters = tokenValidationParameters
      });
    }
  }
}
