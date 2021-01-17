using log4net.Layout;
using log4net.Util;
using System.IO;

namespace Log4NetLibrary
{
    /// <summary>
    /// Format for writing exceptions - removes stacktrace line break
    /// </summary>
    public class ExceptionConvertor : PatternConverter
    {
        protected override void Convert(TextWriter writer, object state)
        {
            var loggingEvent = state as log4net.Core.LoggingEvent;

            //we only care about exceptions here
            if (loggingEvent?.ExceptionObject == null)
                return;

            var encodedMessage = loggingEvent.GetExceptionString();
            if (!string.IsNullOrEmpty(encodedMessage))
            {
                if (encodedMessage.Contains("End of stack trace from previous location where exception was thrown"))
                {
                    //remove this and next line
                    var lines = encodedMessage.Split("\r\n");
                    var remove = "--- End of stack trace from previous location where exception was thrown ---\r\n";
                    var index_to_remove = encodedMessage.IndexOf(remove);
                    var index_to_stop = encodedMessage.IndexOf("\r\n", index_to_remove + remove.Length);
                    var total_length = encodedMessage.Length;
                    var count = index_to_stop - index_to_remove;
                    var part_to_remove = encodedMessage.Substring(index_to_remove, count + 2);
                    var newmsg = encodedMessage.Remove(index_to_remove, count + 2);
                    encodedMessage = newmsg;
                }

                encodedMessage = $"-  {encodedMessage.Replace("\r\n   ", "\r\n-  ")}\n";
            }

            writer.Write(encodedMessage);
        }
    }

    /// <summary>
    /// Left-aligned Level
    /// </summary>
    public class LevelConvertorLeft : PatternConverter
    {
        private static readonly int LENGTH = 9;
        protected override void Convert(TextWriter writer, object state)
        {
            var loggingEvent = state as log4net.Core.LoggingEvent;

            //we only care about level here
            if (loggingEvent?.Level == null)
                return;

            var encodedMessage = loggingEvent.Level.Name;
            if (!string.IsNullOrEmpty(encodedMessage))
            {
                //encodedMessage = encodedMessage.Substring(0, 3); //first 3 letters

                var s = $"[{encodedMessage}]";
                encodedMessage = s.PadRight(LENGTH);
            }

            writer.Write(encodedMessage);
        }
    }

    /// <summary>
    /// Centered Level
    /// </summary>
    public class LevelConvertorCenter : PatternConverter
    {
        private static readonly int LENGTH = 9;
        protected override void Convert(TextWriter writer, object state)
        {
            var loggingEvent = state as log4net.Core.LoggingEvent;

            //we only care about level here
            if (loggingEvent?.Level == null)
                return;

            var encodedMessage = loggingEvent.Level.Name;
            if (!string.IsNullOrEmpty(encodedMessage))
            {
                //encodedMessage = encodedMessage.Substring(0, 3); //first 3 letters

                var s = $"[{encodedMessage}]";

                //center it
                var s_part1 = string.Format("{0," + ((LENGTH + s.Length) / 2).ToString() + "}", s);
                encodedMessage = s_part1.PadRight(LENGTH);
            }

            writer.Write(encodedMessage);
        }
    }

    /// <summary>
    /// Centered Name.1
    /// </summary>
    public class LoggerConverterShort : PatternConverter
    {
        private static readonly int LENGTH = 18;
        protected override void Convert(TextWriter writer, object state)
        {
            var loggingEvent = state as log4net.Core.LoggingEvent;

            //we only care about logger here
            if (loggingEvent?.LoggerName == null)
                return;

            var s = loggingEvent.LoggerName;
            if (!string.IsNullOrEmpty(s))
            {
                //grab last part of name
                var split = s.Split('.');
                s = $"({split[split.Length - 1]})";

                //center it
                var s_part1 = string.Format("{0," + ((LENGTH + s.Length) / 2).ToString() + "}", s);
                //s = s_part1.PadLeft(LENGTH);
                s = s_part1.PadRight(LENGTH);
            }

            writer.Write(s);
        }
    }

    /// <summary>
    /// Left-aligned Name.Full
    /// </summary>
    public class LoggerConverterLong : PatternConverter
    {
        private static readonly int LENGTH = 22;
        protected override void Convert(TextWriter writer, object state)
        {
            var loggingEvent = state as log4net.Core.LoggingEvent;

            //we only care about logger here
            if (loggingEvent?.LoggerName == null)
                return;

            var s = loggingEvent.LoggerName;
            if (!string.IsNullOrEmpty(s))
            {
                var x = 2; //for parentheses
                if(s.Length > LENGTH - x)
                {
                    var split = s.Split('.');
                    if(split.Length > 1) //split up the.logger.name
                    {
                        //start removing parts of logger name
                        for(int i = 1; i < split.Length; i++)
                        {
                            s = string.Join('.', split, i, split.Length - i);
                            if (s.Length <= LENGTH - x)
                                break;
                        }
                    }

                    if(s.Length > LENGTH - x) //remove the difference from the beginning of the rest of the name
                    {
                        s = s.Substring(s.Length - LENGTH - x);
                    }
                }

                s = $"({s})";

                //center it
                //var s_part1 = string.Format("{0," + ((LENGTH + s.Length) / 2).ToString() + "}", s);
                s = s.PadRight(LENGTH);
            }

            writer.Write(s);
        }
    }

    /// <summary>
    /// Add converter for reference to log4net.config
    /// </summary>
    public class CustomPatternLayout : PatternLayout
    {
        public CustomPatternLayout()
        {
            AddConverter(new ConverterInfo { Name = "tabex", Type = typeof(ExceptionConvertor) });
            AddConverter(new ConverterInfo { Name = "lvlL", Type = typeof(LevelConvertorLeft) });
            AddConverter(new ConverterInfo { Name = "lvlC", Type = typeof(LevelConvertorCenter) });
            AddConverter(new ConverterInfo { Name = "editCs", Type = typeof(LoggerConverterShort) });
            AddConverter(new ConverterInfo { Name = "editCl", Type = typeof(LoggerConverterLong) });
        }
    }
}
