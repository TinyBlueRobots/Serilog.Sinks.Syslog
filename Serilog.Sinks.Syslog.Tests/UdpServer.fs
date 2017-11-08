module UdpServer

open System.Net.Sockets
open System.Net
open System.Text
open System.Threading
open System
open System.Collections.Generic

type UdpServer() =
  let port =
    let listener = TcpListener(IPAddress.Loopback, 0)
    listener.Start()
    let port = (listener.LocalEndpoint :?> IPEndPoint).Port
    listener.Stop()
    port
  let client = new UdpClient(IPEndPoint(IPAddress.Any, port))
  let requests = Queue<string>()
  let autoResetEvent = new AutoResetEvent(false)

  let rec receive() =
    async {
      let! result = client.ReceiveAsync() |> Async.AwaitTask
      let message = Encoding.ASCII.GetString result.Buffer
      requests.Enqueue message
      autoResetEvent.Set() |> ignore
      return! receive()
    }

  do
    receive() |> Async.Start

  member __.Port = port
  member __.Wait() = autoResetEvent.WaitOne() |> ignore
  member __.Requests = requests

  interface IDisposable with
    member __.Dispose() = client.Dispose()