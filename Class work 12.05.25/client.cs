using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.Json;
using System.Drawing;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;

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
                var a = GetMessage();
                Console.WriteLine(a);
                Message? clientMessage =
                    JsonSerializer.Deserialize<Message>(a);
                Console.WriteLine($"{clientMessage.user}: {clientMessage.text}");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); break; }
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

        sendMessage(name);/*
        string number = GetMessage();

        Console.WriteLine($"You are {Convert.ToInt32(number)} client");*/

        Thread serverOutputThread = new Thread(ReadingFromServer);
        serverOutputThread.Start();

        Console.WriteLine("==========LOBBY==========");
        Console.WriteLine("This is a chat, say \"ready\" for start game");
        Console.WriteLine();

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
