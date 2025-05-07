using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Text.Json;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

class Player
{
    public int X { get; set; }
    public int Y { get; set; }
    public Player(int x, int y)
    {
        X = x;
        Y = y;
    }
    public Player() : this(0, 0) { }
}

class UDPServerApp
{
    static Random rnd = new Random();
    static int port = 5056;
    static Dictionary<IPEndPoint, Player> players = new Dictionary<IPEndPoint, Player>();
    static Dictionary<IPEndPoint, DateTime> lastSeen = new Dictionary<IPEndPoint, DateTime>();
    static Player[] GetResponseForClient(IPEndPoint client)
    {
        List<Player> response = new List<Player>();
        foreach (var player in players)
        {
            if (!player.Key.Equals(client))
            {
                response.Add(player.Value);
            }
        }
        return response.ToArray();
    }

    static void RemoveInactiveClients()
    {
        DateTime now = DateTime.Now;
        List<IPEndPoint> toRemove = new List<IPEndPoint>();
        foreach (var kvp in lastSeen)
        {
            if ((now - kvp.Value).TotalSeconds > 15)
            {
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var ep in toRemove)
        {
            players.Remove(ep);
            lastSeen.Remove(ep);
            Console.WriteLine($"Клієнт {ep} відключений через неактивність."); 
        }
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        UdpClient server = new UdpClient(port);

        while (true)
        {
            RemoveInactiveClients();

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = server.Receive(ref remoteEP);
            string text = Encoding.UTF8.GetString(data);
            Player player = JsonSerializer.Deserialize<Player>(text);
            if (player == null) { continue; }
            if (players.ContainsKey(remoteEP)) { players[remoteEP] = player; lastSeen[remoteEP] = DateTime.Now; }
            else { players.Add(remoteEP, player); }
            Player[] response = GetResponseForClient(remoteEP);
            server.Send(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)), remoteEP);
        }
    }
}
