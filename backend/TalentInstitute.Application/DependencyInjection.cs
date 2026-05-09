using Microsoft.Extensions.DependencyInjection;
using TalentInstitute.Application.UseCases;

namespace TalentInstitute.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<CheckStudentPaceProgressUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<RegistrarMeritoUseCase>();

        return services;
    }
}
