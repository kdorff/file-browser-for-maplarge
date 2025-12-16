using FileBrowser.Server.Services;

namespace TestProject
{
    public class Program 
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.Configure<FileBrowserOptions>(builder.Configuration.GetSection("FileBrowser"));
            builder.Services.AddScoped<IFileService, FileService>();

            builder.Services.AddControllers();

            var app = builder.Build();

            // Disabled for simplified local testing coverage
            // app.UseHttpsRedirection(); 

            app.UseDefaultFiles(); // Serve index.html
            app.UseStaticFiles();  // Serve wwwroot content

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
