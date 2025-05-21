using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.Json;
using System.Drawing;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;
public enum MessageType
{
    Connect,
    Question,
    Answer,
    Result,
    Info,
    End
}

public class Message
{
    public MessageType Type { get; set; }
    public string? Text { get; set; }
    public object? Data { get; set; }
}

public class Question
{
    public string Text { get; set; } = "";
    public List<string> Choices { get; set; } = new();
    public int CorrectIndex { get; set; }
}

class Client
{
    static string serverIP = "127.0.0.1";
    static int port = 5000;
    static NetworkStream? stream = null;
    static ConsoleColor Color;
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

    static void SendPackage(NetworkStream stream, Message package)
    {
        if (stream == null) return;
        byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(package));
        stream.Write(buffer);
    }

    static Message? GetPackage(NetworkStream stream, int bufsize = 4096)
    {
        if (stream == null) return null;
        byte[] buffer = new byte[bufsize];
        stream.Read(buffer, 0, bufsize);
        return JsonSerializer.Deserialize<Message>(
            Encoding.UTF8.GetString(buffer).Split(char.MinValue).First());
    }

    static DateTime start = DateTime.Now;
    static int ShowMenuWithTimer(string question, List<string> options, int seconds)
    {
        int index = 0;
        while ((DateTime.Now - start).TotalSeconds < seconds)
        {
            Console.Clear();
            Console.WriteLine($"{question}");
            Console.SetCursorPosition(Console.WindowWidth - 7, 0);
            Console.WriteLine($"{seconds - (int)(DateTime.Now - start).TotalSeconds} сек");
            Console.WriteLine();
            for (int i = 0; i < options.Count; i++)
            {
                Console.WriteLine((i == index ? "> " : "  ") + options[i]);
            }

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.UpArrow) index = (index - 1 + options.Count) % options.Count;
                else if (key == ConsoleKey.DownArrow) index = (index + 1) % options.Count;
                else if (key == ConsoleKey.Enter) { Console.Clear(); return index; }
            }

            Thread.Sleep(100);
        }

        return -1;
    }


    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;

        Console.Title = "CLIENT";
        TcpClient tcpClient = new TcpClient(serverIP, port);
        Console.WriteLine("Succes!");

        stream = tcpClient.GetStream();



        while (true)
        {
            Message? msg = GetPackage(stream);
            if (msg == null) break;

            switch (msg.Type)
            {
                case MessageType.Info:
                    Console.WriteLine(msg.Text);
                    Console.ReadLine();
                    Console.Clear();
                    break;

                case MessageType.Question:
                    var q = JsonSerializer.Deserialize<Question>(msg.Data?.ToString());
                    if (q != null)
                    {
                        int selected = ShowMenuWithTimer(q.Text, q.Choices, 60);
                        SendPackage(stream, new Message
                        {
                            Type = MessageType.Answer,
                            Data = selected
                        });
                    }
                    break;

                case MessageType.Result:
                    Console.WriteLine(msg.Text);
                    return;
            }
        }
    }
}
