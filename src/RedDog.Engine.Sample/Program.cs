using System;
using System.Diagnostics.Tracing;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using RedDog.Engine.Diagnostics;
using RedDog.Engine.TableStorage;
using RedDog.Engine.TableStorage.Diagnostics;

namespace RedDog.Engine.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var listener = new ObservableEventListener())
            {
                listener.LogToConsole(new ConsoleFormatter());
                listener.EnableEvents(EngineEventSource.Log, EventLevel.Verbose, Keywords.All);
                listener.EnableEvents(TableStorageEventSource.Log, EventLevel.Verbose, Keywords.All);

                var eventContext = new TableStorageJobEventContext(
                    CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageAccount")));

                var host = new JobHost(eventContext);
                host.Add(new LogCleanupJob());
                host.Add(new SendNewsletterJob());
                host.Add(new LongRunningJob());
                host.Start();

                Console.WriteLine("Waiting...");
                Console.ReadLine();
            }
        }
    }
}
