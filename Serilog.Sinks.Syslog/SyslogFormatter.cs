using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Serilog.Sinks.Syslog
{
  static class StringExtensions
  {
    public static string UnescapeQuotes(this string input)
    {
      return input.Replace(@"\""", @"""");
    }
  }

  class SyslogFormatter
  {
    readonly string _application;
    readonly Facility _facility;
    readonly MessageTemplateTextFormatter _messageTemplateTextFormatter;

    public SyslogFormatter(string application, Facility facility, MessageTemplateTextFormatter messageTemplateTextFormatter = null)
    {
      _application = application;
      _facility = facility;
      _messageTemplateTextFormatter = messageTemplateTextFormatter;
    }

    Severity MapLogEventLevelToSeverity(LogEventLevel logEventLevel)
    {
      switch (logEventLevel)
      {
        case LogEventLevel.Debug: return Severity.Debug;
        case LogEventLevel.Error: return Severity.Error;
        case LogEventLevel.Fatal: return Severity.Emergency;
        case LogEventLevel.Information: return Severity.Info;
        case LogEventLevel.Warning: return Severity.Warn;
        default: return Severity.Notice;
      }
    }

    SyslogEvent MapToSyslogEvent(LogEvent logEvent)
    {

      string escapeChars(LogEventPropertyValue logEventPropertyValue)
      {
        var input = logEventPropertyValue.ToString();
        if (input.StartsWith(@""""))
        {
          input = input.Substring(1);
        }
        if (input.EndsWith(@""""))
        {
          input = input.Substring(0, input.Length - 1);
        }
        input = input.UnescapeQuotes();
        input = Regex.Replace(input, @"[\]\\""]", match => $@"\{match}");
        return input;
      }

      string getLogEventProperty(string property)
      {
        LogEventPropertyValue logEventPropertyValue;
        return
          logEvent.Properties.TryGetValue(property, out logEventPropertyValue)
          ? escapeChars(logEventPropertyValue)
          : "-";
      }

      string getHostName()
      {
        try
        {
          return Dns.GetHostName();
        }
        catch
        {
          return new[] { "COMPUTERNAME", "HOSTNAME" }.Select(Environment.GetEnvironmentVariable).FirstOrDefault() ?? "-";
        }
      }

      var severity = MapLogEventLevelToSeverity(logEvent.Level);
      var priority = (int)_facility * 8 + (int)severity;
      var processId = getLogEventProperty("ProcessId");
      var messageId = getLogEventProperty("SourceContext");
      var properties = logEvent.Properties.Select(kvp => (Key: kvp.Key, Value: escapeChars(kvp.Value)));
      var structuredDataKvps = String.Join(" ", properties.Select(t => $@"{t.Key}=""{t.Value}"""));
      var structuredData = String.IsNullOrEmpty(structuredDataKvps) ? "-" : $"[structuredData@0 {structuredDataKvps}]";
      var syslogEvent = new SyslogEvent
      {
        IsoTimeStamp = logEvent.Timestamp.ToString("O"),
        HostName = getHostName(),
        Application = _application,
        ProcessId = processId,
        MessageId = messageId,
        Priority = priority,
        StructuredData = structuredData
      };
      using (var stringWriter = new StringWriter())
      {
        if (_messageTemplateTextFormatter != null)
        {
          _messageTemplateTextFormatter.Format(logEvent, stringWriter);
        }
        else
        {
          stringWriter.Write(logEvent.RenderMessage().UnescapeQuotes());
        }
        syslogEvent.Message = stringWriter.ToString();
      }
      return syslogEvent;
    }

    public byte[] Format(LogEvent logEvent)
    {
      var syslogEvent = MapToSyslogEvent(logEvent);
      var message = $"<{syslogEvent.Priority}>{syslogEvent.Version} {syslogEvent.IsoTimeStamp} {syslogEvent.HostName} {syslogEvent.Application} {syslogEvent.ProcessId} {syslogEvent.MessageId} {syslogEvent.StructuredData} {syslogEvent.Message}";
      return Encoding.UTF8.GetBytes(message);
    }
  }
}