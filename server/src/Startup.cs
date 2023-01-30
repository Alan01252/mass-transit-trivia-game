using System.Text.Encodings.Web;
using System.Text.Json;
using MassTransit;
using TriviaGame;

namespace Acp.Arc.Service.ThreeEtl
{
    public class Startup
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public Startup(
            IWebHostEnvironment hostingEnvironment,
            IConfiguration configuration)
        {
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Uri schedulerEndpoint = new("loopback://localhost/quartz");
            services
                .AddLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Trace);
                    if (_hostingEnvironment.IsDevelopment())
                    {
                        logging.AddConsole();
                    }
                    else
                    {
                        logging.AddJsonConsole(options =>
                        {
                            options.UseUtcTimestamp = true;
                            options.IncludeScopes = true;
                            options.TimestampFormat = "o"; //2021-07-14T04:31:08.0739380+01:00
                            options.JsonWriterOptions = new JsonWriterOptions
                            {
                                Indented = false,
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            };
                        });
                    }
                });

            services.AddMassTransit(x =>
            {
                x.AddMessageScheduler(schedulerEndpoint);
                x.AddSagaStateMachine<TriviaGameStateMachine, TriviaGameState>().InMemoryRepository();

                x.UsingInMemory((context, cfg) =>
                {
                    var options = new ServiceInstanceOptions()
                        .SetEndpointNameFormatter(KebabCaseEndpointNameFormatter.Instance);

                    cfg.UseMessageScheduler(schedulerEndpoint);
                    cfg.ServiceInstance(options, instance =>
                    {
                        instance.ConfigureJobServiceEndpoints();
                        instance.ConfigureEndpoints(context);
                    });
                    cfg.UseMessageRetry(r => r.Exponential(
                            5,
                            TimeSpan.FromMilliseconds(200),
                            TimeSpan.FromMinutes(5),
                            TimeSpan.FromMilliseconds(200)
                    ));
                    cfg.UseInMemoryScheduler();
                    cfg.UsePrometheusMetrics(serviceName: "TriviaService");
                });

                x.AddConsumersFromNamespaceContaining<SendQuestionConsumer>();

            });

            services.AddOptions();
            services.AddControllers();
            services.AddSignalR();
            services.AddHostedService<SignalRServer>();
        }

        public static void Configure(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseCors(builder =>
            {
                builder.WithOrigins("http://triviagame.com:50001")
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST")
                    .AllowCredentials();
            });

            applicationBuilder.UseRouting();

            applicationBuilder.UseAuthentication();
            applicationBuilder.UseAuthorization();

            applicationBuilder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<TriviaHub>("/TriviaHub");
            });
        }
    }
}