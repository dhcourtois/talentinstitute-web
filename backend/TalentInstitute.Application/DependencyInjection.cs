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
        services.AddScoped<ObtenerAlumnosUseCase>();
        services.AddScoped<ObtenerDetalleAlumnoUseCase>();
        services.AddScoped<CrearAlumnoUseCase>();
        services.AddScoped<AsignarPaceUseCase>();
        services.AddScoped<ObtenerPacesAlumnoUseCase>();
        services.AddScoped<ObtenerMetasSemanaUseCase>();
        services.AddScoped<RegistrarMetaUseCase>();
        services.AddScoped<ActualizarEstatusMetaUseCase>();
        services.AddScoped<ObtenerMeritosAlumnoUseCase>();
        services.AddScoped<RevocarMeritoUseCase>();
        services.AddScoped<ObtenerResumenDashboardUseCase>();
        services.AddScoped<ObtenerAlertasDashboardUseCase>();
        services.AddScoped<ObtenerConfiguracionPrivilegiosUseCase>();
        services.AddScoped<ActualizarConfiguracionPrivilegiosUseCase>();
        services.AddScoped<ObtenerStaffUseCase>();
        services.AddScoped<CrearStaffUseCase>();
        services.AddScoped<EditarStaffUseCase>();
        services.AddScoped<DesactivarStaffUseCase>();

        return services;
    }
}
