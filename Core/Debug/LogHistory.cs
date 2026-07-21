using System;
using System.Collections.Generic;
using System.Linq;

namespace MothDIed.Debug
{
    public static class LogHistory
    {
        public enum LogType
        {
            Message,
            Warning,
            Error
        }

        public struct Log
        {
            public LogType Type;
            public string Content;

            public Log(LogType type, string content)
            {
                Type = type;
                Content = content;
            }
        }
        
        public static List<Log> Logs = new();
        public static Action<Log> OnNew;
        
        public static void PushAsError(string log)
        {
            Logs.Add(new Log(LogType.Error, log));
            OnNew?.Invoke(Logs.Last());
        }

        public static void PushAsWarning(string log)
        {
            Logs.Add(new Log(LogType.Warning, log));
            OnNew?.Invoke(Logs.Last());
        }
        
        public static void PushAsMessage(string log)
        {
            Logs.Add(new Log(LogType.Message, log));
            OnNew?.Invoke(Logs.Last());
        }
    }
}