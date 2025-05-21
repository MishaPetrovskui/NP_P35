using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.Json;
using System.Drawing;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System;

public enum MessageType
{
    Connect,
    Question,
    Answer,
    Result,
    Info,
    End
}

class Test
{
    public string Name { get; set; }
    public List<Question> Questions { get; set; }
}

public class Question
{
    public string Text { get; set; }
    public List<string> Choices { get; set; }
    public int CorrectIndex { get; set; }
}

public class Message
{
    public MessageType Type { get; set; }
    public string? Text { get; set; }
    public object? Data { get; set; }
}
class Server
{
    static TcpListener listener;
    static int port = 5000;
    static int clients = 1;
    static readonly object lockObj = new object();
    static Random rand = new Random();
    /*static List<Question> questions = new()
    {
        new Question { Text = "Скільки буде 2 + 2?", Choices = new List<string> { "3", "4", "5" }, CorrectIndex = 1 },
        new Question { Text = "Столиця Франції?", Choices = new List<string> { "Берлін", "Париж", "Рим" }, CorrectIndex = 1 },
        new Question { Text = "Країна Німеччини?", Choices = new List<string> { "Берлін", "Париж", "Рим" }, CorrectIndex = 0 },
        new Question { Text = "Скільки буде 8 + 2?", Choices = new List<string> { "6", "10", "82" }, CorrectIndex = 2 },
        new Question { Text = "Какой химический элемент имеет символ\"O\"?", Choices = new List<string> { "Озон", "Оксид", "Оксиген", "Кислород" }, CorrectIndex = 3 },
        new Question { Text = "Какая страна является родиной пиццы?", Choices = new List<string> { "Франция", "Испания", "Италия", "Греция" }, CorrectIndex = 2 },
        new Question { Text = "Сколько континентов на Земле?", Choices = new List<string> { "5", "6", "7", "8" }, CorrectIndex = 2 },
        new Question { Text = "Скільки буде 8 + 2?", Choices = new List<string> { "6", "10", "82" }, CorrectIndex = 2 },
        new Question { Text = "Скільки буде 8 + 2?", Choices = new List<string> { "6", "10", "82" }, CorrectIndex = 2 },
    };*/

    static List<Test> tests = new List<Test>()
    {
        new Test
        {
            Name = "Test1",
            Questions = new List<Question> 
            {
                new Question { Text = "Скільки буде 2 + 2?", Choices = new List<string> { "3", "4", "5" }, CorrectIndex = 1 },
                new Question { Text = "Столиця Франції?", Choices = new List<string> { "Берлін", "Париж", "Рим" }, CorrectIndex = 1 },
                new Question { Text = "Країна Німеччини?", Choices = new List<string> { "Берлін", "Париж", "Рим" }, CorrectIndex = 0 },
                new Question { Text = "Скільки буде 8 + 2?", Choices = new List<string> { "6", "10", "82" }, CorrectIndex = 2 },
                new Question { Text = "Какой химический элемент имеет символ\"O\"?", Choices = new List<string> { "Озон", "Оксид", "Оксиген", "Кислород" }, CorrectIndex = 3 },
                new Question { Text = "Какая страна является родиной пиццы?", Choices = new List<string> { "Франция", "Испания", "Италия", "Греция" }, CorrectIndex = 2 },
                new Question { Text = "Сколько континентов на Земле?", Choices = new List<string> { "5", "6", "7", "8" }, CorrectIndex = 2 },
                new Question { Text = "Скільки буде 8 + 2?", Choices = new List<string> { "6", "10", "82" }, CorrectIndex = 2 },
                new Question { Text = "Скільки буде 8 + 2?", Choices = new List<string> { "6", "10", "82" }, CorrectIndex = 2 }
            }
        }, 
        new Test
        {
            Name = "Test2",
            Questions = new List<Question>
            {
                new Question { Text = "Скільки буде 2 + 2?", Choices = new List<string> { "3", "4", "5" }, CorrectIndex = 1 },
                new Question { Text = "Столиця Франції?", Choices = new List<string> { "Берлін", "Париж", "Рим" }, CorrectIndex = 1 },
                new Question { Text = "Країна Німеччини?", Choices = new List<string> { "Берлін", "Париж", "Рим" }, CorrectIndex = 0 },
                new Question { Text = "Скільки буде 8 + 2?", Choices = new List<string> { "6", "10", "82" }, CorrectIndex = 2 },
                new Question { Text = "Какой химический элемент имеет символ\"O\"?", Choices = new List<string> { "Озон", "Оксид", "Оксиген", "Кислород" }, CorrectIndex = 3 },
                new Question { Text = "Какая страна является родиной пиццы?", Choices = new List<string> { "Франция", "Испания", "Италия", "Греция" }, CorrectIndex = 2 },
                new Question { Text = "Сколько континентов на Земле?", Choices = new List<string> { "5", "6", "7", "8" }, CorrectIndex = 2 },
                new Question { Text = "Скільки буде 8 + 2?", Choices = new List<string> { "6", "10", "82" }, CorrectIndex = 2 },
                new Question { Text = "Скільки буде 8 + 2?", Choices = new List<string> { "6", "10", "82" }, CorrectIndex = 2 }
            }
        }
    };

    static List<Question> ShuffleQuestions(List<Question> original)
    {
        var shuffled = original.OrderBy(q => rand.Next()).ToList();
        foreach (var q in shuffled)
        {
            var choices = q.Choices.Select((val, idx) => new { val, idx }).OrderBy(_ => rand.Next()).ToList();
            q.Choices = choices.Select(c => c.val).ToList();
            q.CorrectIndex = choices.FindIndex(c => c.idx == q.CorrectIndex);
        }
        return shuffled;
    }
    //static ConsoleColor Color;

    static List<ConsoleColor> colors = new List<ConsoleColor>
        {
            ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.DarkGreen, ConsoleColor.Magenta, ConsoleColor.Blue, ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.Yellow,
        };

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
        string value = Encoding.UTF8.GetString(buffer).Split(char.MinValue).First();
        Console.WriteLine($"Package got: {value}");
        return JsonSerializer.Deserialize<Message>(value
            );
    }
    static string FileName = "Test.json";
    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        Console.InputEncoding = UTF8Encoding.UTF8;

        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Console.WriteLine("Server Started!");

        /*string JsonString = JsonSerializer.Serialize(questions);
        File.WriteAllText(FileName, JsonString);*/

        questions = JsonSerializer.Deserialize<List<Question>>(File.ReadAllText(FileName));

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Thread thread = new Thread(HandleClient);
            thread.Start(client);
        }
    }

    /*static List<User> Clients = new List<User>();
    static void Broadcast(Message message)
    {
        Console.Write($"{message.user}: ");
        //Console.ForegroundColor = message.color;
        Console.WriteLine(message.text);
        //Console.ResetColor();
        foreach (var item in Clients.ToArray())
        {
            try
            {
                sendMessage(item.tcpClient.GetStream(), JsonSerializer.Serialize(message));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }*/

    static void HandleClient(object obj)
    {
        Console.Title = "SERVER";
        TcpClient client = (TcpClient)obj;
        Console.WriteLine();
        Console.WriteLine("New Client");
        var endPoint = client.Client.RemoteEndPoint.ToString();
        var stream = client.GetStream();
        SendPackage(stream, new Message { Type = MessageType.Info, Text = "Вас вітає тестування. Починаємо!" });
        int score = 0;
        try
        {
            while (true)
            {
                var shuffledQuestions = ShuffleQuestions(questions);
                foreach (var q in shuffledQuestions)
                {
                    SendPackage(stream, new Message
                    {
                        Type = MessageType.Question,
                        Data = q
                    });

                    Message? answerMsg = GetPackage(stream);
                    if (answerMsg?.Type == MessageType.Answer && answerMsg.Data is JsonElement je && je.TryGetInt32(out int index))
                    {
                        if (index == q.CorrectIndex)
                            score++;
                    }
                }

                SendPackage(stream, new Message
                {
                    Type = MessageType.Result,
                    Text = $"Тест завершено. Ваш результат: {score}/{questions.Count}"
                });
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            client.Close();
        }
    }
}
