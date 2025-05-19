using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.Json;
using System.Drawing;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;
enum MessageType
{
    SELECT,
    NAME,
    QUESTION,
    QUESTIONanswer,
    CHOSEN,
    FINISH
}

enum questionType
{
    ANSWER,
    OPEN
}

class test
{
    public string name { get; set; }
    public List<questions> questions { get; set; }
    public test(string name, List<questions> questions)
    {
        this.name = name;
        this.questions = questions;
    }
}

class questions
{
    public string text { get; set; }
    public List<string> answers { get; set; }
    public questionType type { set; get; }
    string corect { get; set; }
    public questions(string text, List<string> answers, string corect, questionType questionType)
    {
        this.text = text;
        this.answers = answers;
        this.corect = corect;
        this.type = questionType;
    }
    public bool IsThisCorectAnswer(string answer)
    {
        if (answer.ToLower().Equals(corect.ToLower()))
            return true;
        return false;
    }
}

class User
{
    public string name { get; set; }
    public IPEndPoint IPPoint { get; set; }
    public test test { get; set; }
    public TcpClient tcpClient { get; set; }
}

class Message
{
    public string user { get; set; }
    public string text { get; set; }
    public MessageType type { get; set; }
    public byte[] data { get; set; }
    //public ConsoleColor color { get; set; }
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

    static void ReadingFromServer()
    {
        while (true)
        {
            try
            {
                Message? clientMessage =
                    JsonSerializer.Deserialize<Message>(GetMessage());
                /*Console.ForegroundColor = clientMessage.color;*/
                Console.WriteLine($"{clientMessage.user}: {clientMessage.text}");
                Console.ResetColor();
            }
            catch (Exception ex) { break; }
        }
    }

    public static uint Menu(IEnumerable<string> Action)
    {
        uint active = 0;
        while (true)
        {
            Console.SetCursorPosition(0, 1);
            for (int i = 0; i < Action.Count(); i++)
            {

                if (i == active)
                    Console.WriteLine($" > {Action.ElementAt(i)}");
                else
                    Console.WriteLine($"   {Action.ElementAt(i)}");
            }

            if (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                if (active > 0 && (key == ConsoleKey.UpArrow || key == ConsoleKey.W))
                    active--;
                else if ((key == ConsoleKey.DownArrow || key == ConsoleKey.S) && active < Action.Count() - 1)
                    active++;
                else if (key == ConsoleKey.Enter)
                {
                    //Console.Clear();
                    return active;
                }
            }
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

        SendPackage(stream, new Message { type = MessageType.NAME, data = Encoding.UTF8.GetBytes(name) });

        while (true)
        {
            int action = 0;
            Message message = GetPackage(stream);
            if (message.type == MessageType.SELECT)
            {
                action = (int)Menu(JsonSerializer.Deserialize<List<string>>(Encoding.UTF8.GetString(message.data)));

                SendPackage(stream, new Message { type = MessageType.CHOSEN, data = Encoding.UTF8.GetBytes($"{action}") });
            }
        }

        Console.WriteLine("Type Enter to exit");
        Console.ReadLine();
    }
}
