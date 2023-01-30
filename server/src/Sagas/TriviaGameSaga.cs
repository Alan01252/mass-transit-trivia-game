using MassTransit;

namespace TriviaGame
{
    public interface GenericTimeout
    {
        int CurrentQuestion { get; }
        Guid GameId { get; }
    }

    public class Player
    {
        public string id;
        //QuestionId, Answer
        public Dictionary<string, string> questionsAnswered = new() { };
    }

    public class Question
    {
        public string Text { get; set; }
        public List<string> Answers { get; set; }
        public string CorrectAnswer { get; set; }
    }

    public class TriviaGameState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public State CurrentState { get; set; }

        public Guid? TimeoutTokenId { get; set; }


        public List<Question> questions = new()
        {
            new Question
            {
                Text = "What is the capital of France?",
                Answers = new List<string> { "Paris", "London", "New York", "Tokyo" },
                CorrectAnswer = "Paris"
            },
            new Question
            {
                Text = "What is the largest planet in our solar system?",
                Answers = new List<string> { "Earth", "Mars", "Jupiter", "Saturn" },
                CorrectAnswer = "Jupiter"
            },
            new Question
            {
                Text = "Who was the 16th President of the United States?",
                Answers = new List<string> { "Abraham Lincoln", "John F. Kennedy", "George Washington", "Thomas Jefferson" },
                CorrectAnswer = "Abraham Lincoln"
            },
            new Question
            {
                Text = "What is the currency of Brazil?",
                Answers = new List<string> { "Peso", "Real", "Dollar", "Euro" },
                CorrectAnswer = "Real"
            }
        };


        public Dictionary<string, Player> players = new();
        public int CurrentQuestion = 0;
        public int MaxPlayers = 4;

    }

    public class TriviaGameStateMachine : MassTransitStateMachine<TriviaGameState>
    {
        public State WaitingForPlayers { get; private set; }
        public State AnsweringQuestions { get; private set; }
        public State GameOver { get; private set; }
        public State Completed { get; private set; }

        public TriviaGameStateMachine(
            ILogger<TriviaGameStateMachine> logger
            )
        {

            Event(() => SendQuestionFailed, x => x.CorrelateById(context => context.Message.Message.CorrelationId));

            Schedule(() => ScheduledTimeout, saga => saga.TimeoutTokenId, s =>
            {
                s.Delay = TimeSpan.FromSeconds(20);
                s.Received = r => r.CorrelateById(context => context.Message.GameId);
            });

            Initially(
                When(StartGame)
                    .TransitionTo(WaitingForPlayers)
                    .Schedule(ScheduledTimeout, context => context.Init<GenericTimeout>(new
                    {
                        GameId = context.Saga.CorrelationId,
                    })));

            During(WaitingForPlayers,
                When(JoinGame)
                    .Then(context =>
                    {
                        logger.LogInformation("Player joined the game");
                        context.Saga.players.Add(context.Message.PlayerId, new Player() { id = context.Message.PlayerId });
                    })
                    .If(context => context.Saga.players.Count == context.Saga.MaxPlayers, maxPlayersReached =>
                    {
                        return
                            maxPlayersReached.Unschedule(ScheduledTimeout)
                            .Schedule(ScheduledTimeout, context => context.Init<GenericTimeout>(new
                            {
                                GameId = context.Saga.CorrelationId,
                            }), context => TimeSpan.FromSeconds(1));
                    }),
                When(ScheduledTimeout.Received)
                    .TransitionTo(AnsweringQuestions)
                    .Then(context =>
                    {
                        logger.LogInformation("Starting game");
                        context.Publish(new SendQuestion()
                        {
                            Question = context.Saga.questions[context.Saga.CurrentQuestion]
                        });
                    })
                    .Schedule(ScheduledTimeout, context => context.Init<GenericTimeout>(new
                    {
                        GameId = context.Saga.CorrelationId,
                    }))
            );

            During(AnsweringQuestions,
                    When(QuestionAnswered)
                        .Then(context =>
                        {
                            if (context.Message.Question.Text == context.Saga.questions[context.Saga.CurrentQuestion].Text)
                            {
                                context.Saga.players[context.Message.PlayerId].questionsAnswered.Add(context.Message.Question.Text, context.Message.Answer);
                            }

                        }),
                    When(ScheduledTimeout.Received)
                        .Then(context =>
                        {
                            context.Saga.CurrentQuestion++;
                            // Publish a message with who got the questions answers right?
                            if (context.Saga.CurrentQuestion < context.Saga.questions.Count)
                            {
                                logger.LogInformation("Sending question {CurrentQuestion}", context.Saga.CurrentQuestion);
                                context.Publish(new SendQuestion()
                                {
                                    Question = context.Saga.questions[context.Saga.CurrentQuestion]
                                });
                            }
                            else
                            {
                                context.TransitionToState(GameOver);
                                context.Publish(new EndGame()
                                {
                                    CorrelationId = context.Saga.CorrelationId,
                                });
                            }
                        })
                        .Schedule(ScheduledTimeout, context => context.Init<GenericTimeout>(new
                        {
                            GameId = context.Saga.CorrelationId,
                        }))
                );

            During(GameOver,
                    When(EndGame)
                        .Unschedule(ScheduledTimeout)
                        .Then(context =>
                        {
                            logger.LogInformation("Game Over");
                            context.Publish(new SendGameOver()
                            {
                                CorrelationId = context.Saga.CorrelationId,
                            });

                        }));

            DuringAny(
                    When(SendQuestionFailed)
                        .Then(context =>
                        {
                            //TODO what would we wanna do here if we've failed to send the question for some reason?
                        })
                );

            SetCompletedWhenFinalized();
        }


        public Event<StartGame> StartGame { get; private set; }
        public Event<JoinGame> JoinGame { get; private set; }
        public Event<EndGame> EndGame { get; private set; }
        public Event<QuestionAnswered> QuestionAnswered { get; private set; }
        // Faults processing contents of archive file:
        public Event<Fault<SendQuestion>> SendQuestionFailed { get; private set; }
        public Schedule<TriviaGameState, GenericTimeout> ScheduledTimeout { get; private set; }
    }
}