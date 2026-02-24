using System;
using System.Collections.Generic;
using System.Linq;

namespace AliceInCradleHack.Commands
{
    public class CommandCommandManager : Command
    {
        public override string Name => "command";
        public override string Description => "Command manager command.";
        public override string Usage =>
            "command [subcommands]\n" +
            "list - List all commands\n" +
            "prompt - Set console prompt\n" +
            "help - Show this help message";
        private CommandManager CommandManager => CommandManager.Instance;
        private readonly Dictionary<string, Action<string[]>> SubCommands;
        public CommandCommandManager()
        {
            SubCommands = new Dictionary<string, Action<string[]>>
            {
                { "list", args => ListCommands() },
                { "prompt", args => SetPrompt(args) },
                { "help", args => GetHelp() }
            };
        }
        public override void Execute(string[] args)
        {
            if (args.Length == 0 || !SubCommands.ContainsKey(args[0]))
            {
                GetHelp();
                return;
            }

            SubCommands[args[0]](args.Skip(1).ToArray());
        }

        private void GetHelp()
        {
            Console.WriteLine(Usage);
        }

        private void ListCommands()
        {
            Console.WriteLine("Available Commands:");
            foreach (var command in CommandManager.Instance.GetAllCommands())
            {
                Console.WriteLine($"{command.Name} - {command.Description}");
            }
        }

        private void SetPrompt(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: command prompt [new_prompt]");
                return;
            }
            CommandManager.Prompt = string.Join(" ", args);
            Console.WriteLine($"Prompt set to: {CommandManager.Prompt}");
        }
    }
}
