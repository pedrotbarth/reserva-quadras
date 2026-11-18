using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Clientes;

public class SqliteHealthCheck(IServiceScopeFactory escopos) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var escopo = escopos.CreateScope();
            var banco = escopo.ServiceProvider.GetRequiredService<DataContext>();
            await banco.Clientes.AnyAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception erro) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("SQLite inacessível.", erro);
        }
    }
}
