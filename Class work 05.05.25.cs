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
    static Player[] GetResponseForClient (IPEndPoint client)
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

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        UdpClient server = new UdpClient(port);
        
        while (true)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = server.Receive(ref remoteEP);
            string text = Encoding.UTF8.GetString(data);
            Player player = JsonSerializer.Deserialize<Player>(text);
            if (player == null) { continue; }
            if (players.ContainsKey(remoteEP)) { players[remoteEP] = player; }
            else { players.Add(remoteEP, player); }
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
        (X,Y) = (0,0);
        (width,height) = (20,20);
        odno_slovo = ConsoleColor.Green;
        dwa_slova = ConsoleColor.Black;
    }
    public void Draw()
    {
        for (int y = 0; y < height; y++)
        {
            Console.SetCursorPosition(X,Y+y);
            Console.BackgroundColor = odno_slovo;
            Console.ForegroundColor = dwa_slova;
            for (int x = 0; x < width; x++)
            {
                Player? player = GetPlayerByPosition(x, y);
                if (player == null) { Console.Write("  "); }
                else { Console.Write("[]"); }
            }
            Console.ResetColor();
        }
    }

    public Player? GetPlayerByPosition(int x, int y)
    {
        foreach(Player player in Players)
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
    static UdpClient client;
    static IPEndPoint serverEP;
    static Player player = new Player();
    static Location location = new Location();
    static readonly object lockObj = new object();
    static Player[] _;

    static void ReadingFromServer()
    {
        while (true)
        {
            try
            {
                _ = JsonSerializer.Deserialize<Player[]>(Encoding.UTF8.GetString(client.Receive(ref serverEP)));
                lock (lockObj)
                {
                    location.Players = _.ToList();
                    location.Players.Add(player);
                }
            }
            catch (Exception ex) { break; }
        }
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;
        Console.CursorVisible = false;
        client = new UdpClient();
        serverEP = new IPEndPoint(IPAddress.Parse(serverIP), port);
        Thread serverOutputThread = new Thread(ReadingFromServer);
        serverOutputThread.Start();

        for (int i = 0; i < _.Length; i++)
        {
            lock (lockObj)
            {
                location.Players.Add(player);
            }
        }

        while (true)
        {
            if(Console.KeyAvailable)
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
