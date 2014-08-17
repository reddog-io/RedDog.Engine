using System;
using System.Diagnostics.Tracing;

namespace RedDog.Engine.TableStorage.Diagnostics
{
    [EventSource(Name = "RedDog-Engine-TableStorage")]
    public class TableStorageEventSource : EventSource
    {
        public static readonly TableStorageEventSource Log = new TableStorageEventSource();

        [NonEvent]
        internal void Verbose(string message, params  object[] args)
        {
            if (IsEnabled())
                Verbose(String.Format(message, args));
        }

        [Event(10, Level = EventLevel.Verbose, Message = "{0}")]
        internal void Verbose(string message)
        {
            if (IsEnabled())
                WriteEvent(10, message);
        }

        [NonEvent]
        internal void Info(string message, params  object[] args)
        {
            if (IsEnabled())
                Info(String.Format(message, args));
        }

        [Event(20, Level = EventLevel.Informational, Message = "{0}")]
        internal void Info(string message)
        {
            if (IsEnabled())
                WriteEvent(20, message);
        }

        [NonEvent]
        internal void Error(string message, params object[] args)
        {
            if (IsEnabled())
                Error(String.Format(message, args));
        }

        [Event(30, Level = EventLevel.Error, Message = "{0}")]
        internal void Error(string message)
        {
            if (IsEnabled())
                WriteEvent(30, message);
        }

        [NonEvent]
        internal void ErrorDetails(Exception exception, string message, params object[] args)
        {
            if (IsEnabled())
                ErrorDetails(exception.Message, exception.GetType().Name, exception.StackTrace, String.Format(message, args));
        }

        [Event(31, Level = EventLevel.Error, Message = "{3}. {1}: {0}")]
        internal void ErrorDetails(string errorMessage, string errorType, string stackTrace, string message)
        {
            if (IsEnabled())
                WriteEvent(31, errorMessage, errorType, stackTrace, message);
        }


        [NonEvent]
        internal void Critical(string message, params object[] args)
        {
            if (IsEnabled())
                Critical(String.Format(message, args));
        }

        [Event(40, Level = EventLevel.Critical, Message = "{0}")]
        internal void Critical(string message)
        {
            if (IsEnabled())
                WriteEvent(40, message);
        }

        [NonEvent]
        internal void CriticalDetails(Exception exception, string message, params object[] args)
        {
            if (IsEnabled())
                CriticalDetails(exception.Message, exception.GetType().Name, exception.StackTrace, String.Format(message, args));
        }

        [Event(41, Level = EventLevel.Critical, Message = "{3}. {1}: {0}")]
        internal void CriticalDetails(string errorMessage, string errorType, string stackTrace, string message)
        {
            if (IsEnabled())
                WriteEvent(41, errorMessage, errorType, stackTrace, message);
        }
    }
}
