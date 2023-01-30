using MassTransit;

namespace TriviaGame
{
    public class JoinGame : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; init; }
        public string? PlayerId { get; init; }
    }
}