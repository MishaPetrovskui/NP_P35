using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.Json;
using System.Drawing;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;


using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Text.Json;
using System.Drawing;

class Message
{
    public string user { get; set; }
    public string text { get; set; }
}
class Server
{
    static TcpListener listener;
    static int port = 5000;
    static int clients = 1;
    static readonly object lockObj = new object();
    static Random rand = new Random();
    static int isReady;
    static bool isGameStarted = false;
    static int questNumber;
    static TcpClient curentPlayer = new TcpClient();

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
    static List<TcpClient> ClientsInGame = new List<TcpClient>();
    static Dictionary<TcpClient, string> Name = new Dictionary<TcpClient, string>();
    static void Broadcast(Message message)
    {
        Console.Write($"{message.user}: ");
        Console.WriteLine(message.text);
        foreach (var item in Clients.ToArray())
        {
            try
            {
                sendMessage(item.GetStream(), JsonSerializer.Serialize(message));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    static string GetName(TcpClient client)
    {
        foreach (var item in Name)
        {
            if (item.Key == client)
                return item.Key.
        }
        return "";
    }

    static void Broadcast(string message)
    {
        Console.WriteLine($"Broadcast: {message}");
        foreach (var item in Clients.ToArray())
        {
            sendMessage(item.GetStream(), message);
        }
    }
    static void Broadcast(string message, TcpClient client)
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
        Name.Add(client, name);
        Broadcast(new Message { user = "SERVER", text = $"{name} ({endPoint}) Connect to Server"});
        try
        {
            while (true)
            {
                Message? clientMessage =
                    JsonSerializer.Deserialize<Message>(GetMessage(stream));
                if (isGameStarted)
                {
                    if (ClientsInGame.Contains(client))
                    {
                        if (client != curentPlayer)
                            sendMessage(stream, JsonSerializer.Serialize(new Message { user = "SERVER", text = $"not your turn" } ));
                        else
                        {
                            int? number;
                            try
                            {
                                number = Convert.ToInt32(clientMessage.text);
                                Broadcast(new Message { user = clientMessage.user, text = clientMessage.text });
                                if (number == questNumber)
                                {
                                    Broadcast(new Message { user = "SERVER", text = $"{name} quest the number!" });
                                    isGameStarted = false;
                                    isReady = 0;
                                }
                                else if (number < questNumber)
                                {
                                    Broadcast(new Message { user = "SERVER", text = $"Corect number is biggest!" });
                                }
                                else if (number > questNumber)
                                {
                                    Broadcast(new Message { user = "SERVER", text = $"Corect number is smallest!" });
                                }
                                curentPlayer = ClientsInGame[ClientsInGame.IndexOf(curentPlayer) == ClientsInGame.Count - 1 ? 0 : ClientsInGame.IndexOf(curentPlayer) + 1];
                                Thread.Sleep(100);
                                Broadcast(new Message { user = "SERVER", text = $"{name}'s turn" });
                            }
                            catch (Exception e)
                            {
                                sendMessage(stream, JsonSerializer.Serialize(new Message { user = "SERVER", text = $"{name} ({endPoint}) No number!" }));
                            }
                        }
                    }
                    else
                    {
                        sendMessage(stream, $"Not your turn");
                    }
                }
                else if (clientMessage.text.ToLower() == "ready")
                {
                    Broadcast(new Message { user = "SERVER", text = $"{name} ready({++isReady}/{Clients.Count})" });
                    if (isReady == Clients.Count)
                    {
                        Broadcast(new Message { user = "SERVER", text = "Start game" });
                        isGameStarted = true;
                        questNumber = rand.Next(0, 101);
                        foreach (var item in Clients)
                        {
                            ClientsInGame.Add(item);
                        }
                        curentPlayer = ClientsInGame[0];
                    }
                }
                else if (clientMessage != null)
                    Broadcast(new Message { user = clientMessage.user, text = clientMessage.text});
            }

        }
        catch (Exception ex)
        {
            Broadcast($"{name} ({endPoint}) Disconected!", client);
        }
        finally
        {
            lock (client)
                Clients.Remove(client);

            client.Close();
        }
    }
}
