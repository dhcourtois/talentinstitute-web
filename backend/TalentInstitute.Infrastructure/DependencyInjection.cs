using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Infrastructure.Authentication;
using TalentInstitute.Infrastructure.Data;
using TalentInstitute.Infrastructure.Repositories;

namespace TalentInstitute.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TalentInstituteDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAlumnoRepository, AlumnoRepository>();
        services.AddScoped<IPaceRepository, PaceRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IMeritoRepository, MeritoRepository>();
        services.AddScoped<IAnotacionRepository, AnotacionRepository>();
        services.AddScoped<IMetaRepository, MetaRepository>();
        services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();

        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IEntrevistaRepository, EntrevistaRepository>();

        return services;
    }
}
