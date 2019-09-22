using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nybus;
using Nybus.Configuration;
using RabbitMQ.Client;

namespace MessageSender
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddLogging(logging => { logging.AddConsole(); });

            services.AddNybus(nybus =>
            {
                nybus.UseRabbitMqBusEngine(rabbitMq =>
                {
                    rabbitMq.Configure(configuration =>
                    {
                        configuration.ConnectionFactory = new ConnectionFactory
                        {
                            HostName = "localhost",
                            UserName = "guest",
                            Password = "guest"
                        };
                    });
                });
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            await host.StartAsync();

            await host.Bus.InvokeCommandAsync(new TranslateCommand
            {
                ToLanguage = Language.English,
                EducationId = 294571
            });

            await host.StopAsync();
        }
    }

    [Message("TranslateEducationCommand", "Examples")]
    public class TranslateCommand : ICommand
    {
        public int EducationId { get; set; }

        public Language ToLanguage { get; set; }
    }

    // https://docs.aws.amazon.com/translate/latest/dg/what-is.html
    public enum Language
    {
        English = 1,
        German = 2,
        Swedish = 3,
        Norwegian = 4,
        Finnish = 5,
        Danish = 6,
        French = 7,
        Italian = 8,
        Russian = 9,
        ChineseSimplified = 10
    }
}
