using System.IO;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Formatters;

namespace RedDog.Engine.Sample
{
    public class ConsoleFormatter : IEventTextFormatter
    {
        public void WriteEvent(EventEntry eventEntry, TextWriter writer)
        {
            writer.WriteLine("{0} {1,-32} {2}", eventEntry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"), "[" + Limit(eventEntry.Schema.ProviderName, 30) + "]", eventEntry.FormattedMessage);
        }

        private string Limit(string text, int length)
        {
            if (text.Length > length)
                return text.Substring(0, length - 3) + "...";
            return text;
        }
    }
}