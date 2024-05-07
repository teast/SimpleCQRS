using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UserApi.CQRS;
using UserApi.Database;

namespace UserApi;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddTransient<CommandHandler>();
        services.AddTransient<UserStorage>();
        services.AddTransient<UserRepository>();

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        services.AddDbContext<UserDbContext>(options =>
        {
            options.UseSqlite(connection);
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceScopeFactory serviceScopeFactory)
    {
        app.UseSwagger();
        app.UseSwaggerUI(config =>
        {
            config.InjectStylesheet("/swagger-ui/swaggerdark.css");
        });

        app.UseDeveloperExceptionPage();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        using (var scope = serviceScopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            context.Database.EnsureCreated();
        }
    }
}
