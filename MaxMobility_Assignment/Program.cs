using MaxMobility_Assignment.Data;
using Microsoft.EntityFrameworkCore;

namespace MaxMobility_Assignment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                 options.UseSqlServer(builder.Configuration.GetConnectionString("MaxMobility_connection"))
            );

            //AddRazorPages: Adds support for Razor Pages, which are a simpler way of building web UIs compared to MVC.
            builder.Services.AddRazorPages();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.MapRazorPages(); //enable the routing of Razor Pages
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Upload}/{action=UploadFile}/{id?}");

            app.Run();
        }
    }
}
