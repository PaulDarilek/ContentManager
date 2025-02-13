namespace ContentManager;


public class CommandParser
{
    public TextReader StdIn { get; } = System.Console.In;
    public TextWriter StdOut { get; } = System.Console.Out;
    public TextWriter StdErr { get; } = System.Console.Error;

    public Dictionary<string, CommandType> ValidCommands { get; } 

    public List<CommandAction> Actions { get; } = [];

    public CommandParser(IEnumerable<CommandType> validCommands)
    {
        ValidCommands = validCommands.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        
        var help = new CommandType(nameof(Help), (args) => Help());
        ValidCommands.TryAdd(help.Name, help);
    }

    public void PromptForActions()
    {
        Help();
        StdOut.WriteLine("Enter Command or Blank Line when done:");

        while (true)
        {
            string? text = StdIn.ReadLine();
            if (string.IsNullOrWhiteSpace(text))
                break;
            if (ValidCommands.TryGetValue(text, out CommandType? command))
            {
                var action = new CommandAction(command);
                PromptForParameters(action);
                Actions.Add(action);
            }
            else
            {
                StdOut.WriteLine($"Invalid Command: {text}");
                Help();
            }
        }
    }

    public void PromptForParameters(CommandAction action)
    {
        while (action.Parameters.Count < action.MaxParms)
        {
            StdOut.Write($"\tParameter[{action.Parameters.Count}]: ");
            var text = StdIn.ReadLine();
            if (string.IsNullOrWhiteSpace(text))
                break;
            action.Parameters.Add(text);
        }
    }


    public void Help()
    {
        StdOut.WriteLine("Valid Commands:");
        foreach (var item in ValidCommands.Values)
        {
            string parmCount =
                item.MinParms == 0 && item.MaxParms == 0 ?
                "(No Parameters)" :
                item.MinParms == item.MaxParms ?
                $"({item.MinParms} parameters)" :
                $"({item.MinParms} to {item.MaxParms} parameters)";

            StdOut.WriteLine($"\t{item.Name}\t{parmCount}");
            if (!string.IsNullOrEmpty(item.Description))
                StdOut.WriteLine($"\t\t{item.Description}");
        }
    }

    public void Parse(string[] args)
    {

        for (int argNumber = 0; argNumber < args.Length; argNumber++)
        {
            string argument = args[argNumber];
            if (ValidCommands.TryGetValue(argument, out CommandType? commandType))
            {
                var action = new CommandAction(commandType!);
                while(action.Parameters.Count < action.MaxParms && argNumber + 1 < args.Length && !ValidCommands.ContainsKey(args[argNumber+1]) )
                {
                    argNumber++;
                    argument = args[argNumber];
                    action.Parameters.Add(argument);
                }
                Actions.Add(action);
                StdOut.WriteLine($"{action.Name}(\"{string.Join("\", \"", action.Parameters)}\")");
            }
            else
            {
                StdOut.WriteLine($"Invalid Command: {argument}");

            }
        }
    }

    public IEnumerable<CommandAction> ExecuteAll(Action<Exception>? onException = null)
    {
        foreach (CommandAction command in Actions)
        {
            yield return command;
            if (command.Execute != null)
            {
                try
                {
                    command.Execute(command.Parameters);
                }
                catch(Exception ex)
                {
                    onException?.Invoke(ex);
                }
            }

        }
    }

}
