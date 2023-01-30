using MassTransit;

namespace TriviaGame
{
    public class EndGame : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; init; }
        public string? PlayerId { get; init; }
    }
}