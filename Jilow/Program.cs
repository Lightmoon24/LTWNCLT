using Jilow.Data;
using Microsoft.EntityFrameworkCore;
using Supabase;

namespace Jilow
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đọc cấu hình Supabase
            var supabaseUrl = builder.Configuration["Supabase:Url"];
            var supabaseAnonKey = builder.Configuration["Supabase:AnonKey"];
            // Razor Pages
            builder.Services.AddRazorPages();
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            // Entity Framework
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // Supabase
            builder.Services.AddSingleton<Client>(_ =>
            {
                var options = new SupabaseOptions
                {
                    AutoConnectRealtime = false
                };

                var client = new Client(
                    supabaseUrl!,
                    supabaseAnonKey!,
                    options);

                client.InitializeAsync().GetAwaiter().GetResult();

                return client;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages().WithStaticAssets();

            app.Run();
        }
    }
}