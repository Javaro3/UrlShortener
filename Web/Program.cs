using Domains.Domains;
using Domains.ViewModels;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Repositories;
using Services.DataServices;
using Web.Middleware;

namespace Web {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            string connectionString = builder.Configuration.GetConnectionString("MariaDB");
            List<int> databaseVersionData = builder.Configuration["DatabaseVersion"]
                .Split(".")
                .Select(e => int.Parse(e))
                .ToList();

            var databaseVersion = new MySqlServerVersion(new Version(databaseVersionData[0], databaseVersionData[1], databaseVersionData[2]));

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<UrlShortnerContext>(option => option.UseMySql(connectionString, databaseVersion));
            builder.Services.AddScoped<IRepository<ShortUrl>, ShortUrlRepository>();
            builder.Services.AddScoped<ShortUrlDataService>();
            builder.Services.AddSession();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment()) {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSession();
            app.UseDbInitializerMiddleware();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=ShortUrl}/{action=Index}/{id?}");

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "hash",
                    pattern: "/{hash}",
                    defaults: new { controller = "ShortUrl", action = "RedirectToUrl" }
                    );
            });

            app.Run();
        }
    }
}