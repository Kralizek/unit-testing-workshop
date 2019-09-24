using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QueueProcessor.Handlers;
using Nybus;
using Nybus.Configuration;
using QueueProcessor.Messages;
using Amazon.Translate;
using QueueProcessor.Services;

namespace QueueProcessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder();

            builder.ConfigureHostConfiguration(configuration =>
            {
                configuration.SetBasePath(Directory.GetCurrentDirectory());

                configuration.AddJsonFile("hostsettings.json", true);
                configuration.AddEnvironmentVariables(prefix: "NYBUS_");
                configuration.AddCommandLine(args);
            });

            builder.ConfigureAppConfiguration((context, configuration) =>
            {
                configuration.AddJsonFile("appsettings.json", true);
                configuration.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true);
                configuration.AddEnvironmentVariables(prefix: "NYBUS_");
                configuration.AddCommandLine(args);
            });

            builder.ConfigureServices((context, services) =>
            {
                services.AddHostedService<NybusHostedService>();

                services.AddNybus(nybus =>
                {
                    nybus.UseConfiguration(context.Configuration);

                    nybus.UseRabbitMqBusEngine(rabbitMq =>
                    {
                        rabbitMq.UseConfiguration();

                        rabbitMq.Configure(configuration => configuration.CommandQueueFactory = new StaticQueueFactory("QueueProcessor"));
                    });

                    nybus.SubscribeToCommand<TranslateEducationCommand>();
                });

                
                services.AddDefaultAWSOptions(context.Configuration.GetAWSOptions("AWS"));
                services.AddAWSService<IAmazonTranslate>();
                services.AddAWSService<IAmazonS3>();

                services.AddHttpClient();
                //services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient());

                services.Configure<TranslateOptions>(context.Configuration.GetSection("Translator"));

                //services.AddCommandHandler<SingleTranslateCommandHandler>();
                
                services.AddCommandHandler<ImprovedTranslateCommandHandler>();

                services.AddSingleton<IEducationProfileDownloader, HttpClientEducationProfileDownloader>();
                services.AddSingleton<ITextExtractor, HtmlTextExtractor>();
                services.AddSingleton<ITranslator, AmazonTranslateTranslator>();
                services.AddSingleton<ITranslationPersister, AmazonS3TranslationPersister>();

                services.AddHttpClient<IEducationProfileDownloader, HttpClientEducationProfileDownloader>();
                
            });

            builder.ConfigureLogging((context, logging) =>
            {
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                logging.AddAWSProvider(context.Configuration.GetAWSLoggingConfigSection());
                logging.AddConsole();
            });

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
