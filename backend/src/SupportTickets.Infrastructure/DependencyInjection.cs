using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportTickets.Application.Interfaces;
using SupportTickets.Infrastructure.Identity;
using SupportTickets.Infrastructure.Persistence;
using SupportTickets.Infrastructure.Services;

namespace SupportTickets.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";

        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlServer"));
            }
            else
            {
                options.UseSqlite(configuration.GetConnectionString("Sqlite") ?? "Data Source=dev.db");
            }
        });

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ITimeEntryService, TimeEntryService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
