using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Text.Json;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

class Message
{
    public string user { get; set; }
    public string text { get; set; }
}

class User
{
    public string name { get; set; }
    public IPEndPoint ip { get; set; }
}

class Server
{
    static Random rnd = new Random();
    static int port = 5056;
    static List<User> ipEndPoints = new List<User>();

    static bool IsNew(IPEndPoint endpoint)
    {
        foreach (var item in ipEndPoints)
        {
            if (item.ip.Equals(endpoint))
                return true;
        }
        return false;
    }

    static string Name(IPEndPoint endpoint)
    {
        foreach (var item in ipEndPoints)
        {
            if (item.ip.Equals(endpoint))
                return item.name;
        }
        return "";
    }

    static void Broadcast(string message, UdpClient server, string name)
    {
        Console.WriteLine($"{name}: {message}");
        byte[] data = Encoding.UTF8.GetBytes($"{name} - {message}");
        foreach (var item in ipEndPoints)
        {
            server.Send(data, item.ip);
        }
    }

    static void Main(string[] args)
    {
        UdpClient server = new UdpClient(port);
        /*
                while (true)
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = server.Receive(ref remoteEP);

                    string message = Encoding.UTF8.GetString(data);
                    Console.WriteLine($"{remoteEP.Address.Address}: {message}");
                    if (message.ToLower() == "ping")
                    {
                        server.Send(Encoding.UTF8.GetBytes("Pong"), remoteEP);
                        Console.WriteLine("Send message: Pong");
                    }
                    else
                    {
                        server.Send(Encoding.UTF8.GetBytes("Not ping"), remoteEP);
                        Console.WriteLine("Send message: Not ping");
                    }
                }*/
        int i = 0;
        while (true)
        {

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = server.Receive(ref remoteEP);
            string text = Encoding.UTF8.GetString(data);
            User user = new User { name = text, ip = remoteEP };
            if (IsNew(remoteEP))
            {
                ipEndPoints.Add(user);
                i++;
                Console.WriteLine($"{user.name}: Added");
            }
            else
                Broadcast(text, server, Name(remoteEP));
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

class Message
{
    public string user { get; set; }
    public string text { get; set; }
}



class Client
{
    static string serverIP = "127.0.0.1";
    static int port = 5056;
    static UdpClient client;
    static IPEndPoint serverEP;
    static void ReadingFromServer()
    {
        while (true)
        {
            try
            {
                string text = Encoding.UTF8.GetString(client.Receive(ref serverEP));

                Console.WriteLine($"{text}");
            }
            catch (Exception ex) { break; }
        }
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;
        client = new UdpClient();

        serverEP = new IPEndPoint(IPAddress.Parse(serverIP), port);

        Console.Write("Name: ");
        string message = Console.ReadLine();
        byte[] data = Encoding.UTF8.GetBytes(message);
        /*
                byte[] data;
                Stopwatch a = new Stopwatch();
                if (message.ToLower() == "ping")
                {
                    a = Stopwatch.StartNew();
                    data = Encoding.UTF8.GetBytes(message);
                }
                else
                {
                    data = Encoding.UTF8.GetBytes(message);
                }
                client.Send(data, serverEP);
                Console.WriteLine("Message send");

                byte[] responce = client.Receive(ref serverEP);
                string answer = Encoding.UTF8.GetString(responce);


                if (answer == "Pong")
                {
                    a.Stop();
                    Console.WriteLine(a.Elapsed);
                    Console.WriteLine($"Catched message: {answer}");
                }
                else
                    Console.WriteLine($"Catched message: {answer}");*/
        Thread thread = new Thread(ReadingFromServer);
        thread.Start();
        while (true)
        {
            Console.Write("");
            string text = Console.ReadLine();
            if (text != null)
            {
                byte[] data1 = Encoding.UTF8.GetBytes(text);
                client.Send(data1, serverEP);
            }
        }
    }
}
