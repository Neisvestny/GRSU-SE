using System.Text;

struct Sequence
{
    public string proteinName;
    public string organismName;
    public string proteinSequence;
}

struct Command
{
    public string commandName;
    public string[] commandParameters;
}

static class Program
{
    public static string contentPath = "";
    public static int filesID = 0;
    public static int commandID = 1;
    public static StringBuilder fileOutput = new StringBuilder();

    static (string sequencesFilePath, string commandsFilePath) GetFilesDirectories()
    {
        contentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\content");
        string[] sequenceFiles = Directory.GetFiles(contentPath, "sequences*.txt");

        if (sequenceFiles.Length == 0)
        {
            Console.WriteLine("Not found any sequences files");
            return (string.Empty, string.Empty);
        }

        Console.WriteLine("Choose sequences file:");
        for (int i = 0; i < sequenceFiles.Length; i++)
            Console.WriteLine($"{i + 1}) {Path.GetFileNameWithoutExtension(sequenceFiles[i])}");

        while (true)
        {
            Console.Write(">>> ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out filesID) &&
                filesID >= 1 &&
                filesID <= sequenceFiles.Length)
            {
                filesID--;
                break;
            }

            Console.WriteLine("Wrong input!");
        }

        string sequencesFilePath = sequenceFiles[filesID];
        string commandsFilePath = Path.Combine(contentPath, $"commands.{filesID}.txt");

        if (!File.Exists(commandsFilePath))
        {
            Console.WriteLine("No commands file for this sequences file!");
            commandsFilePath = string.Empty;
        }

        return (sequencesFilePath, commandsFilePath);
    }

    static string DecodeRLESequence(string proteinSequence)
    {
        if (string.IsNullOrEmpty(proteinSequence))
            return string.Empty;

        var result = new StringBuilder();
        int i = 0;

        while (i < proteinSequence.Length)
        {
            if (char.IsDigit(proteinSequence[i]))
            {
                int start = i;
                while (i < proteinSequence.Length && char.IsDigit(proteinSequence[i]))
                    i++;

                if (i >= proteinSequence.Length)
                    throw new FormatException("RLE string ends with number but no character follows.");

                int num = int.Parse(proteinSequence[start..i]);

                result.Append(proteinSequence[i], num);
                i++;
            }
            else
            {
                result.Append(proteinSequence[i]);
                i++;
            }
        }

        return result.ToString();
    }

    static (List<Sequence> sequences, List<Command> commands) GetFilesContent(
        (string sequencesFilePath, string commandsFilePath) filesPaths)
    {
        var sequences = new List<Sequence>();
        var commands = new List<Command>();

        if (!File.Exists(filesPaths.sequencesFilePath))
        {
            Console.WriteLine($"Sequences file not found: {filesPaths.sequencesFilePath}");
            return (sequences, commands);
        }
        if (!File.Exists(filesPaths.commandsFilePath))
        {
            Console.WriteLine($"Commands file not found: {filesPaths.commandsFilePath}");
            return (sequences, commands);
        }

        foreach (var line in File.ReadLines(filesPaths.sequencesFilePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var tokens = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 3)
            {
                Console.WriteLine($"Invalid sequence line skipped: {line}");
                continue;
            }

            sequences.Add(new Sequence
            {
                proteinName = tokens[0],
                organismName = tokens[1],
                proteinSequence = DecodeRLESequence(tokens[2])
            });
        }

        foreach (var line in File.ReadLines(filesPaths.commandsFilePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var tokens = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                Console.WriteLine($"Invalid command line skipped: {line}");
                continue;
            }

            var cmd = new Command
            {
                commandName = tokens[0],
                commandParameters = tokens.Skip(1).ToArray()
            };

            if (cmd.commandName.Equals("search", StringComparison.OrdinalIgnoreCase) &&
                cmd.commandParameters.Length > 0)
            {
                cmd.commandParameters[0] = DecodeRLESequence(cmd.commandParameters[0]);
            }

            commands.Add(cmd);
        }

        return (sequences, commands);
    }

    static void AddCommandID(Command cmd, int paramCount)
    {
        var parameters = string.Join("   ", cmd.commandParameters);
        fileOutput.Append($"{commandID:D3}   {cmd.commandName}   {parameters}\n");
        commandID++;
    }

    static void CommandSearch(List<Sequence> sequences, Command cmd)
    {
        AddCommandID(cmd, 1);
        var matches = sequences.Where(s => s.proteinSequence.Contains(cmd.commandParameters[0]));
        fileOutput.Append("organism\t\t\t\t\tprotein\n");
        if (matches.Any())
            foreach (var s in matches)
                fileOutput.Append($"{s.organismName}\t\t{s.proteinName}\n");
        else
            fileOutput.Append("NOT FOUND\n");
    }

    static void CommandDiff(List<Sequence> sequences, Command command)
    {
        AddCommandID(command, 2);
        fileOutput.Append("amino-acids difference:\n");

        var matchForFirstProteinName = sequences
            .FirstOrDefault(sequence => sequence.proteinName.Contains(command.commandParameters[0]));

        var matchForSecondProteinName = sequences
            .FirstOrDefault(sequence => sequence.proteinName.Contains(command.commandParameters[1]));

        if (matchForFirstProteinName.proteinName != null && matchForSecondProteinName.proteinName != null)
        {
            var longerSequence = matchForFirstProteinName;
            var shorterSequence = matchForSecondProteinName;
            if (longerSequence.proteinSequence.Length < matchForSecondProteinName.proteinSequence.Length)
            {
                longerSequence = matchForSecondProteinName;
                shorterSequence = matchForFirstProteinName;
            }

            int replaceAmount = 0;
            for (int i = 0; i < shorterSequence.proteinSequence.Length; i++)
            {
                if (shorterSequence.proteinSequence[i] != longerSequence.proteinSequence[i])
                {
                    replaceAmount++;
                }
            }

            replaceAmount += (longerSequence.proteinSequence.Length - shorterSequence.proteinSequence.Length);

            fileOutput.Append($"{replaceAmount}\n");
        }
        else
        {
            if (matchForFirstProteinName.proteinName == null)
            {
                fileOutput.Append($"MISSING:\t{matchForFirstProteinName.proteinName}\n");
            }
            else if (matchForSecondProteinName.proteinName == null)
            {
                fileOutput.Append($"MISSING:\t{matchForSecondProteinName.proteinName}\n");
            }
            else
            {
                fileOutput.Append($"MISSING:\t{matchForFirstProteinName.proteinName}\t{matchForSecondProteinName.proteinName}\n");
            }
        }
    }

    static void CommandMode(List<Sequence> sequences, Command cmd)
    {
        AddCommandID(cmd, 1);
        fileOutput.Append("amino-acid occurs:\n");

        var s = sequences.FirstOrDefault(x => x.proteinName.Contains(cmd.commandParameters[0]));
        if (s.proteinName != null)
        {
            var res = s.proteinSequence
                .GroupBy(c => c)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .OrderByDescending(p => p.Count)
                .ThenBy(p => p.Key)
                .First();
            fileOutput.Append($"{res.Key}          {res.Count}\n");
        }
        else
            fileOutput.Append($"MISSING: {cmd.commandParameters[0]}\n");
    }

    static void ExecutionOfCommands((List<Sequence> sequences, List<Command> commands) data)
    {
        foreach (var cmd in data.commands)
        {
            switch (cmd.commandName.ToLowerInvariant())
            {
                case "search":
                    CommandSearch(data.sequences, cmd);
                    break;

                case "diff":
                    CommandDiff(data.sequences, cmd);
                    break;

                case "mode":
                    CommandMode(data.sequences, cmd);
                    break;

                default:
                    fileOutput.Append($"Unknown command: {cmd.commandName}\n");
                    break;
            }

            fileOutput.Append('-', 74).Append('\n');
        }
    }

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var filesPaths = GetFilesDirectories();
        var pair = GetFilesContent(filesPaths);

        fileOutput.Append("Kishkunou Ruslan\nGenetic Searching\n" + new string('-', 74) + "\n");

        ExecutionOfCommands(pair);

        File.WriteAllText(Path.Combine(contentPath, $"genedata.{filesID}.txt"), fileOutput.ToString());
        Console.WriteLine("File Saved!");
    }
}
