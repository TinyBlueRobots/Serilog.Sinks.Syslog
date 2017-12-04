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
    SyslogFormatter _syslogFormatter;

    void Init(string server, int port, SyslogFormatter syslogFormatter)
    {
      _syslogFormatter = syslogFormatter;
      _udpClient = new UdpClient { ExclusiveAddressUse = false };
      _udpClient.Connect(server, port);
    }

    public SyslogSink(string server, int port, int batchSizeLimit, TimeSpan period, int queueLimit, SyslogFormatter syslogFormatter) : base(batchSizeLimit, period, queueLimit)
    {
      Init(server, port, syslogFormatter);
    }

    public SyslogSink(string server, int port, int batchSizeLimit, TimeSpan period, SyslogFormatter syslogFormatter) : base(batchSizeLimit, period)
    {
      Init(server, port, syslogFormatter);
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