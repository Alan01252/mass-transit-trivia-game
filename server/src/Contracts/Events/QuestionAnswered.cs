using MassTransit;

namespace TriviaGame
{
    public class QuestionAnswered : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; init; }
        public string PlayerId { get; init; }
        public Question Question { get; init; }
        public string Answer { get; init; }
    }
}