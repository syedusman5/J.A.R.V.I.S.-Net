using System.Security.Cryptography;

Console.WriteLine("Jarvis online. Type 'help', or 'exit' to quit.");

string lastInput = string.Empty;

Console.WriteLine();

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (input is null) break;                        
    if (string.IsNullOrWhiteSpace(input)) continue;

    var text = input.Trim().ToLowerInvariant();

    if (text is "exit" or "quit" or "bye") break;

    var response = Respond(text, lastInput);

    if(text is not "again")
    {
        lastInput = text;

    }

    Console.WriteLine(response);
    Console.WriteLine();
}

Console.WriteLine("Goodbye.");

 static string Respond(string text, string lastInput = "")
{
    if (text.Contains("time"))
        return $"It's {DateTime.Now:h:mm tt}.";

    if (text.Contains("date") || text.Contains("today"))
        return $"Today is {DateTime.Now:dddd, d MMMM yyyy}.";

    if (text.Contains("hello") || text.Contains("hi"))
        return "Hello. What can I do for you?";

    if(text.Contains("flip a coin"))
    {
        int random = RandomNumberGenerator.GetInt32(1, 3);

        if (random == 1) return "Tail";

        return "Head";
    }

    if (text.Contains("roll a dice"))
    {
        int diceNum = RandomNumberGenerator.GetInt32(1, 6);

        return diceNum.ToString();
    }

    if(text.Contains("tell me a joke"))
    {
        string[] randomJokes = 
                            [
                            "Why do C# developers wear glasses? Because they can't C#.",
                            "How many programmers does it take to change a light bulb? None, that's a hardware problem.",
                            "There are 10 types of people: those who understand binary, and those who don't."
                            ];

        return randomJokes[Random.Shared.Next(randomJokes.Length)];
    }


    if (text is "help")
        return """
               I can tell you:
                 - the time      "what time is it"
                 - the date      "what's the date"
               Say "exit" to quit.
               """;

    if (text is "again" && lastInput is not "")
    {
        var result =  Respond(lastInput);

        return result;
    }
    else if (text is "again" && lastInput is "")
    {
        return "I am nodding like I know what you are talking about, but I am blank.";
    }

    return "I don't know how to do that yet. Try 'help'.";
}
