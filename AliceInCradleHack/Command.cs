using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AliceInCradleHack
{
    public abstract class Command
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Usage { get; }
        public abstract void Execute(string[] args);
    }

    public class CommandManager
    {
        private readonly Hashtable commands = new Hashtable();
        private bool Inited = false;
        private readonly Thread commandThread;
        public string Prompt { get; set; } = "> ";

        public static readonly CommandManager Instance = new CommandManager();


        private CommandManager() 
        {
            commandThread = new Thread(new ThreadStart(CommandLoop));
        }

        public void Initialize()
        {
            if (Inited) return;
            Console.WriteLine("Registering initial commands...");
            List<Command> initialCommands = new List<Command>
            {
                new Commands.CommandCommandManager(),
                // Add other command instances here
            };
            foreach (var command in initialCommands)
            {
                RegisterCommand(command);
            }
            Inited = true;
        }

        public void RegisterCommand(Command command) 
        {
            commands.Add(command.Name, command);
        }

        public void ExecuteCommand(string input) 
        {
            string[] parts = input.Split(' ');
            string commandName = parts[0];
            string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : new string[0];
            if (commands.ContainsKey(commandName))
            {
                Command command = (Command)commands[commandName];
                command.Execute(args);
            }
            else
            {
                Console.WriteLine($"Command '{commandName}' not found.");
            }
        }

        public IEnumerable<Command> GetAllCommands() 
        {
            foreach (DictionaryEntry entry in commands)
            {
                yield return (Command)entry.Value;
            }
        }

        public void RunCommandLoop()
        {
            commandThread.Start();
        }

        public void CommandLoop()
        {
            while (true)
            {
                Console.Write(Prompt);
                string input = Console.ReadLine();
                if (input.ToLower() == "exit")
                {
                    break;
                }
                ExecuteCommand(input);
            }
        }
    }
}