using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace TriviaGame
{
    public class SendGameOverConsumer : IConsumer<SendGameOver>
    {
        private readonly IHubContext<TriviaHub> _hubContext;

        public SendGameOverConsumer(
            IHubContext<TriviaHub> hubContext
        )
        {
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<SendGameOver> context)
        {
            await _hubContext.Clients.All.SendAsync("GameOver");
        }
    }
}