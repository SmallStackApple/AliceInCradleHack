using AliceInCradleHack.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AliceInCradleHack
{

    public class CommandManager
    {
        private readonly Dictionary<string, Command> commands = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);
        private bool Inited = false;
        private readonly Thread commandThread;
        public string Prompt { get; set; } = "> ";

        private static readonly Lazy<CommandManager> __lazyInstance = new Lazy<CommandManager>(() => new CommandManager());
        public static CommandManager Instance { get; } = __lazyInstance.Value;

        private CommandManager() 
        {
            commandThread = new Thread(new ThreadStart(CommandLoop));
        }

        /// <summary>
        /// Initializes the command manager by registering initial commands.
        /// This method can only be called once.
        /// </summary>
        public void Initialize()
        {
            if (Inited) return;
            Console.WriteLine("Registering initial commands...");
            List<Command> initialCommands = new List<Command>
            {
                new Commands.CommandCommandManager(),
                new Commands.CommandModuleManager(),
                new Commands.CommandNotify(),
                // Add other command instances here
            };
            foreach (var command in initialCommands)
            {
                RegisterCommand(command);
            }
            Inited = true;
        }

        /// <summary>
        /// Registers a new command with the command manager.
        /// </summary>
        /// <param name="command">The command to register</param>
        public void RegisterCommand(Command command) 
        {
            commands.Add(command.Name, command);
        }

        /// <summary>
        /// Executes a command based on the input string.
        /// </summary>
        /// <param name="input">The input string containing command name and arguments</param>
        public void ExecuteCommand(string input) 
        {
            try
            {
                string[] parts = input.Split(' ');
                if (parts.Length == 0) return;
                
                string commandName = parts[0];
                string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : new string[0];
                
                if (commands.TryGetValue(commandName, out Command command))
                {
                    try
                    {
                        command.Execute(args);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error executing command '{commandName}': {ex.Message}");
                        Console.WriteLine(ex.StackTrace);
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.WriteLine($"Command '{commandName}' not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing command: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all registered commands.
        /// </summary>
        /// <returns>An enumerable collection of commands</returns>
        public IEnumerable<Command> GetAllCommands() 
        {
            foreach (var command in commands.Values)
            {
                yield return command;
            }
        }

        /// <summary>
        /// The main command loop that reads user input and executes commands.
        /// </summary>
        private void CommandLoop()
        {
            while (true)
            {
                try
                {
                    Console.Write(Prompt);
                    string input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        continue;
                    }
                    ExecuteCommand(input);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in command loop: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Starts the command loop in a new thread.
        /// </summary>
        public void RunCommandLoop()
        {
            commandThread.Start();
        }
    }
}