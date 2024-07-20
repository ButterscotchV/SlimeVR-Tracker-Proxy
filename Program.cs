using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using SlimeVRTrackerProxy;

var cfg = "slimefwd.cfg";
var defaultPort = 6969;

bool TryParseEndpoint(string addr, [NotNullWhen(true)] out IPEndPoint? endPoint)
{
    if (IPEndPoint.TryParse(addr, out endPoint))
    {
        // If the user didn't provide a port, we can just fill in the default port
        if (endPoint.Port <= 0)
        {
            endPoint.Port = defaultPort;
        }

        return true;
    }

    return false;
}

IPEndPoint ParseEndpoint(string addr)
{
    if (TryParseEndpoint(addr, out var remote))
        return remote;
    throw new CmdArgumentException("Remote endpoint is not valid (ex. 192.168.0.1:6969).");
}

try
{
    IPEndPoint remote;
    if (args.Length >= 1)
    {
        remote = ParseEndpoint(args[0]);
    }
    else
    {
        // No arguments given, let's prompt the user or load their previous response
        if (File.Exists(cfg))
        {
            remote = ParseEndpoint(File.ReadAllText(cfg));
            Console.WriteLine($"Loaded remote endpoint from \"{cfg}\" ({remote}).");
        }
        else
        {
            Console.Write(
                "No remote endpoint provided, please enter one now (ex. 192.168.0.1:6969):\n> "
            );
            remote = ParseEndpoint(Console.ReadLine() ?? "");

            File.WriteAllText(cfg, remote.ToString());
            Console.WriteLine($"Valid remote endpoint provided, saved to \"{cfg}\".");
        }
    }

    var localPort = defaultPort;
    if (args.Length >= 2)
    {
        if (int.TryParse(args[1], out var argPort) && argPort > 0)
            localPort = argPort;
        else
            throw new CmdArgumentException("Local port is not valid (ex. 6969).");
    }

    var cancelToken = new CancellationTokenSource();

    UdpClient localSocket;
    try
    {
        localSocket = new UdpClient(localPort);
    }
    catch (Exception e)
    {
        throw new CmdArgumentException(
            $"Unable to open local socket on port {localPort}. Ensure the port is not currently in use.",
            e
        );
    }

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

    Console.WriteLine(
        $"Listening on local port {localPort}, forwarding to {remote}.\nPress enter at any time to stop."
    );
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
        Console.WriteLine(
            "\nUsage: slimefwd.exe <Remote Endpoint (ex. 192.168.0.1:6969)> [Local Port (ex. 6969)]"
        );

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
