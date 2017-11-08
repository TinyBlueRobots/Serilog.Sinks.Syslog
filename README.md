# Serilog.Sinks.Syslog

[![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.SyslogServer.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.SyslogServer/)

```csharp
var log = new LoggerConfiguration()
    .WriteTo.Syslog("syslog.domain.com", 12345, "my-app")
    .CreateLogger();
```

**server** The address of the syslog server.

**port** The port number of the syslog server.

**application** The name of the application.

### Optional parameters

**facility** The syslog facility. Defaults to `User`.

**batchSizeLimit** The maximum number of events to include in a single batch.

**period** The time to wait between checking for event batches.

**queueLimit** Maximum number of events in the queue.

**outputTemplate** A message template describing the output messages. See https://github.com/serilog/serilog/wiki/Formatting-Output.

This sink endeavours to support [RFC5424](https://tools.ietf.org/html/rfc5424):

Optionally, MSGID is set to the `SourceContext`

Optionally, PROCID is set using [`Serilog.Enrichers.Process`](https://www.nuget.org/packages/Serilog.Enrichers.Process/) and `Enrich.WithProcessId()`

STRUCTURED-DATA is set using the SD-ID `structuredData@0` and contains all the event's properties