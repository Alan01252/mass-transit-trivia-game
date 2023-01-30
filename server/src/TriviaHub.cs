using MassTransit;
using Microsoft.AspNetCore.SignalR;
using TriviaGame;

public interface ITriviaClient
{
    Task<string> GetMessage();
}

public class TriviaHub : Hub<ITriviaClient>
{
    private readonly IBus _bus;

    public TriviaHub(IBus bus)
    {
        _bus = bus;
    }

    public async Task StartGame()
    {
        var gameId = NewId.NextGuid();
        await _bus.Publish(new StartGame()
        {
            CorrelationId = gameId,
            PlayerId = Context.ConnectionId
        });

        await _bus.Publish(new JoinGame()
        {
            CorrelationId = gameId,
            PlayerId = Context.ConnectionId
        });

    }
}