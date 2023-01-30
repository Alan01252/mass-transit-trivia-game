using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace TriviaGame
{
    public class SendQuestionConsumer : IConsumer<SendQuestion>
    {
        private readonly IHubContext<TriviaHub> _hubContext;

        public SendQuestionConsumer(
            IHubContext<TriviaHub> hubContext
        )
        {
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<SendQuestion> context)
        {
            // Send the question to the client, but don't include the correct answer
            await _hubContext.Clients.All.SendAsync("Question", new TriviaGame.Question
            {
                Text = context.Message.Question.Text,
                Answers = context.Message.Question.Answers
            });
        }
    }
}