using MassTransit;
using Microsoft.AspNetCore.SignalR;
using TriviaGame;

public class SignalRServer : BackgroundService
{
    private readonly ILogger<SignalRServer> _logger;
    private readonly IHubContext<TriviaHub, ITriviaClient> _triviaHub;
    private readonly IBus _bus;

    public SignalRServer(ILogger<SignalRServer> logger,
    IHubContext<TriviaHub, ITriviaClient> triviaHub,
    IBus bus
    )
    {
        _logger = logger;
        _triviaHub = triviaHub;
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var GameId = NewId.NextGuid();

        while (!stoppingToken.IsCancellationRequested)
        {
            //_logger.LogInformation("Worker running at: {Time}", DateTime.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}