using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.Interactivity.Extensions;

namespace RemoraPaginationSystem;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPagination(this IServiceCollection services)
    {
        services.AddInteractivity();
        services.AddInteractionGroup<PaginationHandler>();

        return services;
    }
}