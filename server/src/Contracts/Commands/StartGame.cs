using MassTransit;

namespace TriviaGame
{
    public class StartGame : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; init; }
        public string? PlayerId { get; init; }
    }
}