using AliceInCradleHack.command.commands;
using AliceInCradleHack.utils.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AliceInCradleHack.command
{
    /// <summary>
    /// Command manager (singleton). Registers commands and runs the console input loop.
    /// </summary>
    public class CommandManager
    {
        private readonly Dictionary<string, Command> _commands = new(StringComparer.OrdinalIgnoreCase);
        private readonly Thread _commandThread;
        private bool _initialized;

        private static readonly Lazy<CommandManager> _lazyInstance = new(() => new CommandManager());
        public static CommandManager Instance => _lazyInstance.Value;

        public string Prompt { get; set; } = "> ";

        private CommandManager()
        {
            _commandThread = new Thread(CommandLoop);
        }

        /// <summary>
        /// Registers the built-in commands. Only effective on the first call.
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;
            Log.Info("Registering initial commands...");
            List<Command> initialCommands = new()
            {
                new CommandCommandManager(),
                new CommandModuleManager(),
                new CommandNotify(),
                // Add other command instances here
            };
            foreach (var command in initialCommands)
            {
                RegisterCommand(command);
            }
            _initialized = true;
        }

        /// <summary>
        /// Registers a new command.
        /// </summary>
        public void RegisterCommand(Command command)
        {
            _commands.Add(command.Name, command);
        }

        /// <summary>
        /// Executes a command from a raw input line (command name followed by arguments).
        /// </summary>
        public void ExecuteCommand(string input)
        {
            try
            {
                string[] parts = input.Split(' ');
                if (parts.Length == 0) return;

                string commandName = parts[0];
                string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : Array.Empty<string>();

                if (_commands.TryGetValue(commandName, out Command command))
                {
                    try
                    {
                        command.Execute(args);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error executing command '{commandName}'", ex);
                    }
                }
                else
                {
                    Console.WriteLine($"Command '{commandName}' not found.");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error processing command", ex);
            }
        }

        /// <summary>
        /// Gets all registered commands.
        /// </summary>
        public IEnumerable<Command> GetAllCommands()
        {
            foreach (var command in _commands.Values)
            {
                yield return command;
            }
        }

        /// <summary>
        /// Reads user input and executes commands until the console closes.
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
                    Log.Error("Error in command loop", ex);
                }
            }
        }

        /// <summary>
        /// Starts the command loop on a background thread.
        /// </summary>
        public void RunCommandLoop()
        {
            _commandThread.Start();
        }
    }
}
