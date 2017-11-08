namespace Serilog.Sinks.Syslog
{
  class SyslogEvent
  {
    public int Version = 1;
    public string IsoTimeStamp { get; set; }
    public string HostName { get; set; }
    public string Application { get; set; }
    public string ProcessId { get; set; }
    public string MessageId { get; set; }
    public int Priority { get; set; }
    public string StructuredData { get; set; }
    public string Message { get; set; }
  }
}