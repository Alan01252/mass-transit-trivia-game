namespace Acp.Arc.Service.ThreeEtl
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //Use default http://localhost:5000
                    //Note if using Microsoft docker image this will be based on ASPNETCORE_URLS and default to port 80)
                    webBuilder
                        .UseKestrel()
                        .ConfigureAppConfiguration((context, configurationBuilder) => ConfigureApp(context, configurationBuilder, args))
                        .UseStartup<Startup>();
                });
        }

        private static void ConfigureApp(WebHostBuilderContext _, IConfigurationBuilder configurationBuilder, string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var basePath = Directory.GetCurrentDirectory();

            configurationBuilder
                .SetBasePath(basePath)
                .AddEnvironmentVariables();
        }
    }
}
