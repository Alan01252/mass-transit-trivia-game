using MassTransit;

namespace TriviaGame
{
    public class SendQuestion : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; init; }

        public Question Question { get; init; }
    }
}