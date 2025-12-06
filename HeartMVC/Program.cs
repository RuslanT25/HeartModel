using HeartMVC.Services;

namespace HeartMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<FastForestClassificationService>();
            builder.Services.AddScoped<LogisticRegressionService>();
            builder.Services.AddScoped<ModelEvaluationService>();
            builder.Services.AddScoped<AdvancedAnalyticsService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Heart}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
