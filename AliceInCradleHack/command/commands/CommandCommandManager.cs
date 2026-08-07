using System;

namespace AliceInCradleHack.command.commands
{
    public class CommandCommandManager : SubCommandDispatcher
    {
        public override string Name => "command";
        public override string Description => "Command manager command.";
        public override string Usage =>
            "command [subcommands]\n" +
            "list - List all commands\n" +
            "prompt - Set console prompt\n" +
            "help - Show this help message";

        public CommandCommandManager()
        {
            RegisterSubCommand("list", _ => ListCommands());
            RegisterSubCommand("prompt", SetPrompt);
            RegisterSubCommand("help", _ => Console.WriteLine(Usage));
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
            CommandManager.Instance.Prompt = string.Join(" ", args);
            Console.WriteLine($"Prompt set to: {CommandManager.Instance.Prompt}");
        }
    }
}
