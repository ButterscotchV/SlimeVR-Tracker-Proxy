using System.Net;
using System.Net.Sockets;
using SlimeVRTrackerProxy;

try
{
    if (args.Length <= 0 || !IPEndPoint.TryParse(args[0], out var remote))
        throw new CmdArgumentException("Remote endpoint is not valid (ex. 192.168.0.1:6969).");

    var localPort = 6969;
    if (args.Length >= 2)
    {
        if (int.TryParse(args[1], out var argPort) && argPort > 0)
            localPort = argPort;
        else
            throw new CmdArgumentException("Local port is not valid (ex. 6969).");
    }

    var cancelToken = new CancellationTokenSource();

    var localSocket = new UdpClient(localPort);
    var clientDict = new Dictionary<IPEndPoint, UdpClient>();
    var clientTasks = new List<Task>();

    async Task RemoteToClient(IPEndPoint client, UdpClient clientSocket, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var recv = await clientSocket.ReceiveAsync(ct);
            await localSocket.SendAsync(recv.Buffer, client, ct);
        }
    }

    async Task ClientToRemote(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var recv = await localSocket.ReceiveAsync(ct);

            if (!clientDict.TryGetValue(recv.RemoteEndPoint, out var clientSocket))
            {
                clientSocket = new UdpClient(0);
                clientSocket.Connect(remote);
                clientDict[recv.RemoteEndPoint] = clientSocket;

                clientTasks.Add(RemoteToClient(recv.RemoteEndPoint, clientSocket, ct));

                Console.WriteLine($"Client {recv.RemoteEndPoint} connected.");
            }

            await clientSocket.SendAsync(recv.Buffer, ct);
        }
    }

    Console.WriteLine($"Listening on local port {localPort}, forwarding to {remote}.\nPress enter at any time to stop.");
    var _ = ClientToRemote(cancelToken.Token);
    Console.ReadLine();

    cancelToken.Cancel();
    foreach (var entry in clientDict)
    {
        entry.Value.Dispose();
        clientDict.Remove(entry.Key);
    }
    localSocket.Dispose();
}
catch (Exception e)
{
    Console.Error.WriteLine(e);

    if (e is CmdArgumentException)
        Console.WriteLine("\nUsage: slimefwd.exe <Remote Endpoint (ex. 192.168.0.1:6969)> [Local Port (ex. 6969)]");

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
