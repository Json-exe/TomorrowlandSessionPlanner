using Microsoft.Extensions.DependencyInjection;

namespace TomorrowlandSessionPlanner.Core.ViewModel;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddViewModel(this IServiceCollection services)
    {
        return services
            .AddTransient<UploadPlanViewModel>()
            .AddTransient<SessionAnalyzerViewModel>();
    }
}