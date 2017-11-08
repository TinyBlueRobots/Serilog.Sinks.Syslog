module Tests

open Expecto
open Serilog
open Serilog.Sinks.Syslog
open System
open System.Text.RegularExpressions
open System.Net

let setup =
  lazy
    let udpServer = new UdpServer.UdpServer()
    Log.Logger <-
      LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.Syslog("127.0.0.1", udpServer.Port, "test", Facility.User, Nullable 1, TimeSpan.FromMilliseconds 100. |> Nullable)
        .Enrich.WithProcessId()
        .CreateLogger()
    udpServer

let mtch input =
  let result = Regex.Match(input, """^<(?<pri>\d+)>1 (?<timestamp>[^\s]+) (?<hostname>[^\s]+) (?<application>[^\s]+) (?<processid>[^\s]+) (?<messageid>[^\s]+) \[(?<structuredData>.+)\] (?<message>.+)""")
  let group (name : string) = result.Groups.[name].Value
  group "pri", group "timestamp", group "hostname", group "application", group "processid", group "messageid", group "structuredData", group "message"

type FooBar = class end

[<Tests>]
let tests =

  testList "tests" [

    test "error" {
      let udpServer = setup.Value
      let log = Log.ForContext<FooBar>()
      log.Error("Error {$foo}", "bar")
      udpServer.Wait()
      let localHostName = Dns.GetHostName()
      let pri, timestamp, hostname, application, processId, messageId, structuredData, message = udpServer.Requests.Dequeue() |> mtch
      Expect.equal pri "11" "pri"
      Expect.isMatch timestamp """\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+\+\d{2}:\d{2}""" "timestamp"
      Expect.equal hostname localHostName "application"
      Expect.equal application "test" "application"
      Expect.isMatch processId "\\d+" "processId"
      Expect.equal messageId "Tests+FooBar" "messageId"
      let expectedStructuredData = sprintf @"structuredData@0 foo=""bar"" SourceContext=""%s"" ProcessId=""%s""" messageId processId
      Expect.equal structuredData expectedStructuredData "structuredData"
      Expect.equal message @"Error ""bar""" "message"
    }

    test "information" {
      let udpServer = setup.Value
      Log.Information("Information {$foo}", "bar")
      udpServer.Wait()
      let pri, _, _, _, _, _, _, _ = udpServer.Requests.Dequeue() |> mtch
      Expect.equal pri "14" "pri"
    }

  ]