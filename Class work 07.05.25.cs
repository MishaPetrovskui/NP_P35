// server 


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
    public ConsoleColor Color { get; set; }
    public Player(int x, int y, ConsoleColor color)
    {
        X = x;
        Y = y;
        Color = color;
    }
    public Player() : this(0, 0, ConsoleColor.White) { }
}

class UDPServerApp
{
    static Random rnd = new Random();
    static int port = 5056;
    static Dictionary<IPEndPoint, Player> players = new Dictionary<IPEndPoint, Player>();
    static Dictionary<IPEndPoint, DateTime> lastSeen = new Dictionary<IPEndPoint, DateTime>();
    static List<ConsoleColor> colors = new List<ConsoleColor>
    {
        ConsoleColor.Red, ConsoleColor.DarkGreen, ConsoleColor.Magenta, ConsoleColor.Blue, ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.Yellow
    };
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

    static ConsoleColor GetAvailableColor()
    {
        return colors[rnd.Next(colors.Count)];
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
            if (players.ContainsKey(remoteEP)) { player.Color = players[remoteEP].Color; players[remoteEP] = player; lastSeen[remoteEP] = DateTime.Now; }
            else { player.Color = GetAvailableColor(); players.Add(remoteEP, player); players[remoteEP] = player; lastSeen[remoteEP] = DateTime.Now; Console.WriteLine($"New gamer {remoteEP}, color: {player.Color}"); }
            Player[] response = GetResponseForClient(remoteEP);
            server.Send(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)), remoteEP);
        }
    }
}


// client 


using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.Json;
using System.Drawing;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;

class Player
{
    public int X { get; set; }
    public int Y { get; set; }
    public ConsoleColor Color { get; set; }
    public Player(int x, int y)
    {
        X = x;
        Y = y;
    }
    public Player() : this(0, 0) { }
}

class Location
{
    public int X { get; set; }
    public int Y { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public List<Player> Players { get; set; } = new List<Player>();
    public ConsoleColor odno_slovo { get; set; }
    public ConsoleColor dwa_slova { get; set; }

    public Location()
    {
        (X, Y) = (0, 0);
        (width, height) = (20, 20);
        odno_slovo = ConsoleColor.Green;
        dwa_slova = ConsoleColor.Black;
    }
    public void Draw()
    {
        for (int y = 0; y < height; y++)
        {
            Console.SetCursorPosition(X, Y + y);
            Console.BackgroundColor = odno_slovo;
            /*Console.ForegroundColor = dwa_slova;*/
            for (int x = 0; x < width; x++)
            {
                Player? player = GetPlayerByPosition(x, y);
                if (player == null) { Console.Write("  "); }
                else { Console.ForegroundColor = player.Color; Console.Write("[]"); }
            }
            Console.ResetColor();
        }
    }

    public Player? GetPlayerByPosition(int x, int y)
    {
        Player[] players;
        lock (Players)
        {
            players = Players.ToArray();
        }
        foreach (Player player in players)
        {
            if (player.X == x && player.Y == y)
                return player;
        }
        return null;
    }
}

class UDPClientApp
{
    static string serverIP = "127.0.0.1";
    static int port = 5056;
    static UdpClient client = new UdpClient();
    static IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(serverIP), port);
    static Player player = new Player();
    static Location location = new Location();
    static readonly object lockObj = new object();
    static Player[] _;

    static void UpdatePlayer()
    {
        while (true)
        {
            client.Send(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(player)), serverEP);
            string response = Encoding.UTF8.GetString(client.Receive(ref serverEP));
            Player[] otherPlayers = JsonSerializer.Deserialize<Player[]>(response);
            if (otherPlayers != null)
            {
                lock (lockObj)
                {
                    location.Players.Clear();
                    location.Players.Add(player);
                    location.Players.AddRange(otherPlayers);
                }
            }
            Thread.Sleep(10);
        }
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;
        Console.CursorVisible = false;
        Thread serverOutputThread = new Thread(UpdatePlayer);
        serverOutputThread.Start();

            lock (lockObj)
            {
                location.Players.Add(player);
            }

        while (true)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.LeftArrow: player.X -= 1; break;
                    case ConsoleKey.RightArrow: player.X += 1; break;
                    case ConsoleKey.UpArrow: player.Y -= 1; break;
                    case ConsoleKey.DownArrow: player.Y += 1; break;
                }
                byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(player));
                client.Send(buffer, serverEP);
            }
            location.Draw();
        }
    }
}
    }
}
