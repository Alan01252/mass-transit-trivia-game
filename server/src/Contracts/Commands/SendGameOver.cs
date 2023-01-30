using MassTransit;

namespace TriviaGame
{
    public class SendGameOver : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; init; }

        public Question Question { get; init; }
    }
}