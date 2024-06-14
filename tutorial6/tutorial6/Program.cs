using tutorial6.Repos;
using tutorial6.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

        IConfiguration _configuration;

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddScoped<IProductWarehouseRepository, ProductWarehouseRepository>();
        builder.Services.AddScoped<IProductWarehouseService, ProductWarehouseService>();

        var app = builder.Build();
        app.Configuration.GetConnectionString("Default");
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.MapControllers();
        app.Run();
    }
}