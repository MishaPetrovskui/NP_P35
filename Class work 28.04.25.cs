using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;


class Server
{
    static TcpListener listener;
    static int port = 5000;
    static int clients = 1;
    static readonly object lockObj = new object();
    static void sendMessage(NetworkStream stream, string message)
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
        stream.Read(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer);
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

    static List<NetworkStream> streams = new List<NetworkStream>();

    static void Broadcast(string message)
    {
        foreach (var item in streams)
        {
            sendMessage(item, message);
        }
    }

    static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        Console.WriteLine();
        Console.WriteLine("New Client");

        var endPoint = client.Client.RemoteEndPoint.ToString();

        var stream = client.GetStream();
        streams.Add(stream);
        string name = GetMessage(stream);
        Console.WriteLine($"Name {name} | {endPoint.ToString()}");


        lock (lockObj)
        {
            sendMessage(stream, Convert.ToString(clients++));
        }

        /*Broadcast(name);*/

        try
        {
            while (true) { GetMessage(stream); }
        }
        catch (Exception ex)
        {
            Broadcast($"{name} off");
        }/*
        finally
        {
            lock (clients) { clients.Remove(client); }
            client.Close(); 
        }*/

    }

}



using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

class Client
{
    static string serverIP = "127.0.0.1";
    static int port = 5000;
    static NetworkStream? stream = null;

    static void sendMessage(string message)
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
        stream.Read(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer);
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;

        Console.Write("Type your name: ");
        string a = Console.ReadLine();


        TcpClient tcpClient = new TcpClient(serverIP, port);
        Console.WriteLine("Succes!");

        stream = tcpClient.GetStream();

        sendMessage(a);
        string number = GetMessage();

        Console.WriteLine($"You are {Convert.ToInt32(number)} client");

        while (true)
        {
            Console.WriteLine(GetMessage());
        }
    }
}
