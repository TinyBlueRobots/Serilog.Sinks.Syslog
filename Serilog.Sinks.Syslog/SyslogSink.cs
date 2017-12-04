using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Syslog
{
  class SyslogSink : PeriodicBatchingSink
  {
    public const int DefaultBatchPostingLimit = 10;
    public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);
    UdpClient _udpClient;
    readonly SyslogFormatter _syslogFormatter;

    void createUdpClient(string server, int port)
    {
      _udpClient = new UdpClient { ExclusiveAddressUse = false };
      _udpClient.Connect(server, port);
    }

    public SyslogSink(string server, int port, int batchSizeLimit, TimeSpan period, int queueLimit, SyslogFormatter syslogFormatter) : base(batchSizeLimit, period, queueLimit)
    {
      createUdpClient(server, port);
      _syslogFormatter = syslogFormatter;
    }

    public SyslogSink(string server, int port, int batchSizeLimit, TimeSpan period, SyslogFormatter syslogFormatter) : base(batchSizeLimit, period)
    {
      createUdpClient(server, port);
      _syslogFormatter = syslogFormatter;
    }

    protected override Task EmitBatchAsync(IEnumerable<LogEvent> events)
    {
      var tasks =
        events
        .Select(_syslogFormatter.Format)
        .Select(bytes => _udpClient.SendAsync(bytes, bytes.Length));
      return Task.WhenAll(tasks);
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      _udpClient.Dispose();
    }
  }
}