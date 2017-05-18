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

namespace WebApplication1
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
      services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

      services.AddMvc();
      services.AddEntityFramework()
              .AddEntityFrameworkSqlite()
              .AddDbContext<FileDbContext>(options => options.UseSqlite("Data Source=filedb.sqlite"));

      services.AddTransient<FileService>();

    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"))
                   .AddDebug();

      app.UseMvc();

      InitializeDatabase(app);
    }

    private void InitializeDatabase(IApplicationBuilder app)
    {
      using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
      {
        var db = serviceScope.ServiceProvider.GetService<FileDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
      }
    }
  }
}
