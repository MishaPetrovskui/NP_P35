
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Text.Json;

class Message
{
    public string user { get; set; }
    public string text { get; set; }
    public ConsoleColor color { get; set; }
}
class Server
{
    static List<ConsoleColor> colors = new List<ConsoleColor> { ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.Blue, ConsoleColor.DarkGreen, ConsoleColor.DarkBlue, ConsoleColor.DarkCyan, ConsoleColor.DarkGray, ConsoleColor.DarkMagenta, ConsoleColor.DarkRed, ConsoleColor.DarkYellow, ConsoleColor.Gray, ConsoleColor.Green, ConsoleColor.White };
    static Random rnd = new Random();

    static TcpListener listener;
    static int port = 5000;
    static int clients = 1;
    static readonly object lockObj = new object();
    static void sendMessage(NetworkStream stream, string message, int buffsize = 1024)
    {
        if (stream == null)
            return;
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        stream.Write(buffer, 0, buffer.Length);
    }
    static string GetMessage(NetworkStream stream, int buffsize = 1024)
    {
        if (stream == null)
            return "";
        byte[] buffer = new byte[buffsize];
        stream.Read(buffer, 0, buffsize);
        string ret = Encoding.UTF8.GetString(buffer).Split(char.MinValue).First();

        return ret;
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;


        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Console.WriteLine("Server Started!");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Thread thread = new Thread(HandleClient);
            thread.Start(client);
        }
    }

    static List<TcpClient> Clients = new List<TcpClient>();
    static void Broadcast(Message message)
    {
        Console.WriteLine($"{message.user}: {message.text}");
        Console.ForegroundColor = message.color;
        foreach (var item in Clients.ToArray())
        {
            try
            {
                sendMessage(item.GetStream(), JsonSerializer.Serialize
                    (
                    new Message { user = message.user, text = message.text, color = message.color }
                    ));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    static void Broadcast(string message)
    {
        Console.WriteLine($"Broadcast: {message}");
        foreach (var item in Clients.ToArray())
        {
            sendMessage(item.GetStream(), message);
        }
    }
    static void BroadcastExceptSOMEONE(string message, TcpClient client)
    {
        Console.WriteLine($"Broadcast: {message}");
        foreach (var item in Clients.ToArray())
        {
            if (item != client)
                sendMessage(item.GetStream(), message);
        }
    }

    static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        Console.WriteLine();
        Console.WriteLine("New Client");

        var endPoint = client.Client.RemoteEndPoint.ToString();
        Clients.Add(client);
        var stream = client.GetStream();
        string name = GetMessage(stream);
        Console.WriteLine($"Name {name} | {endPoint.ToString()}");


        lock (lockObj)
        {
            sendMessage(stream, Convert.ToString(clients++));
        }
        Broadcast(new Message { user = "Server", text = $"{name} ({endPoint}) added", });
        try
        {
            while (true)
            {
                Message? clientMessage =
                    JsonSerializer.Deserialize<Message>(GetMessage(stream));
                if (clientMessage != null)
                {
                    Broadcast(new Message { user = clientMessage.user, text = clientMessage.text, color = clientMessage.color });
                    // Broadcast($"{clientMessage.color}{clientMessage.user}: {clientMessage.text}");
                }
            }

        }
        catch (Exception ex)
        {
            BroadcastExceptSOMEONE($"{name} ({endPoint}) Disconected!", client);
        }
        finally
        {
            lock (client)
                Clients.Remove(client);

            client.Close();
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
class Message
{
    public string user { get; set; }
    public string text { get; set; }
    public ConsoleColor color { get; set; }
}
class Client
{
    static string serverIP = "127.0.0.1";
    static int port = 5000;
    static NetworkStream? stream = null;
    static Random rand = new Random();

    static void sendMessage(string message, int buffsize = 1024)
    {
        if (stream == null)
            return;
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        stream.Write(buffer, 0, buffer.Length);
    }
    static string GetMessage(int buffsize = 1024)
    {
        if (stream == null)
            return "";
        byte[] buffer = new byte[buffsize];
        stream.Read(buffer, 0, buffsize);
        string ret = Encoding.UTF8.GetString(buffer).Split(char.MinValue).First();

        return ret;

    }

    static void ReadingFromServer()
    {
        while (true)
        {
            try
            {
                Message? clientMessage =
                    JsonSerializer.Deserialize<Message>(GetMessage());
                Console.ForegroundColor = clientMessage.color;
                Console.ResetColor();
            }
            catch (Exception ex) { break; }
        }
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;

        Console.Write("Type your name: ");
        string name = Console.ReadLine();


        TcpClient tcpClient = new TcpClient(serverIP, port);
        Console.WriteLine("Succes!");

        stream = tcpClient.GetStream();

        sendMessage(name);
        string number = GetMessage();

        Console.WriteLine($"You are {Convert.ToInt32(number)} client");

        Thread serverOutputThread = new Thread(ReadingFromServer);
        serverOutputThread.Start();

        Message a = new Message();

        while (true)
        {
            string text = Console.ReadLine();
            sendMessage(
                JsonSerializer.Serialize(
                    new Message { user = name, text = text }));
        }

        Console.WriteLine("Type Enter to exit");
        Console.ReadLine();
    }
}
