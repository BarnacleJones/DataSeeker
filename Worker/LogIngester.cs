using Microsoft.Extensions.DependencyInjection;
using Service.Contract;

namespace Worker;

public class LogIngester
{
    private readonly IServiceProvider _provider;

    public LogIngester(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task IngestLogs()
    {
        using var scope = _provider.CreateScope();
        var ingestionService = scope.ServiceProvider.GetRequiredService<ILogIngestionService>();
        await ingestionService.IngestLogsAsync();
    }
}