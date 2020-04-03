using System;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.Syslog;

namespace Serilog
{
  /// <summary>
  /// Extends Serilog configuration to write events to Scalyr.
  /// </summary>
  public static class LoggerConfigurationSyslogExtensions
  {
    /// <summary>
    /// Adds a sink that writes log events to Syslog
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger configuration.</param>
    /// <param name="server">The address of the syslog server.</param>
    /// <param name="port">The port number of the syslog server.</param>
    /// <param name="application">The name of the application.</param>
    /// <param name="facility">The syslog facility.</param>
    /// <param name="batchSizeLimit">The maximum number of events to include in a single batch.</param>
    /// <param name="period">The time to wait between checking for event batches.</param>
    /// <param name="queueLimit">Maximum number of events in the queue.</param>
    /// <param name="outputTemplate">A message template describing the output messages.See https://github.com/serilog/serilog/wiki/Formatting-Output.</param>
    /// <param name="restrictedToMinimumLevel">The minimum level for events passed through the sink.</param>
    /// <param name="hostNamePrefix">A prefix that can be attached to the hostname to enable filtering e.g. in a kubernetes cluster</param>
    public static LoggerConfiguration Syslog(this LoggerSinkConfiguration loggerSinkConfiguration, string server, int port, string application, Facility facility = Facility.User, int? batchSizeLimit = null, TimeSpan? period = null, int? queueLimit = null, string outputTemplate = null, LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose, string hostNamePrefix = null)
    {
      var messageTemplateTextFormatter = String.IsNullOrWhiteSpace(outputTemplate) ? null : new MessageTemplateTextFormatter(outputTemplate, null);
      var syslogFormatter = new SyslogFormatter(application, facility, messageTemplateTextFormatter, hostNamePrefix);
      var sink =
          queueLimit.HasValue ?
          new SyslogSink(server, port, batchSizeLimit ?? SyslogSink.DefaultBatchPostingLimit, period ?? SyslogSink.DefaultPeriod, queueLimit.Value, syslogFormatter) :
          new SyslogSink(server, port, batchSizeLimit ?? SyslogSink.DefaultBatchPostingLimit, period ?? SyslogSink.DefaultPeriod, syslogFormatter);
      return loggerSinkConfiguration.Sink(sink, restrictedToMinimumLevel);
    }
  }
}