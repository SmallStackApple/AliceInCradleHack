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
    /// <see cref="Initialize"/> both registers the built-in commands and starts the input
    /// loop; <see cref="Dispose"/> stops the loop and clears the command registry.
    /// </summary>
    public class CommandManager : IClientComponent
    {
        private readonly Dictionary<string, Command> _commands = new(StringComparer.OrdinalIgnoreCase);
        private Thread _commandThread;
        private volatile bool _stopRequested;
        private bool _initialized;
        private int _commandLoopGeneration;

        private static readonly Lazy<CommandManager> _lazyInstance = new(() => new CommandManager());
        public static CommandManager Instance => _lazyInstance.Value;

        public string Prompt { get; set; } = "> ";

        private CommandManager() { }

        /// <summary>
        /// Registers the built-in commands and starts the console input loop.
        /// Idempotent; only the first call has an effect.
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

            _stopRequested = false;
            int generation = unchecked(++_commandLoopGeneration);
            _commandThread = new Thread(() => CommandLoop(generation))
            {
                Name = "CommandLoop",
                IsBackground = true
            };
            _initialized = true;
            _commandThread.Start();
        }

        /// <summary>
        /// Registers a new command. Duplicate names are skipped with a warning.
        /// </summary>
        public void RegisterCommand(Command command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (_commands.ContainsKey(command.Name))
            {
                Log.Warn($"Command '{command.Name}' already registered, skipping.");
                return;
            }
            _commands.Add(command.Name, command);
        }

        /// <summary>
        /// Removes a previously registered command by name.
        /// </summary>
        public bool UnregisterCommand(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && _commands.Remove(name);
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
        /// Stops the command loop and clears the command registry. If the loop does not exit
        /// within a short timeout (e.g. it is blocked on <see cref="Console.ReadLine"/>), the
        /// background thread is abandoned.
        /// </summary>
        public void Dispose()
        {
            if (!_initialized && _commands.Count == 0) return;

            _stopRequested = true;
            unchecked { _commandLoopGeneration++; }

            var commandThread = _commandThread;
            _commandThread = null;
            try { commandThread?.Interrupt(); }
            catch { /* thread may not be started yet */ }

            if (commandThread != Thread.CurrentThread && commandThread?.IsAlive == true)
            {
                if (!commandThread.Join(TimeSpan.FromSeconds(2)))
                    Log.Warn("Command loop did not exit in time; abandoning thread.");
            }

            _commands.Clear();
            _initialized = false;
        }

        /// <summary>
        /// Reads user input and executes commands until <see cref="Dispose"/> is called.
        /// </summary>
        private void CommandLoop(int generation)
        {
            while (!_stopRequested && generation == _commandLoopGeneration)
            {
                try
                {
                    Console.Write(Prompt);
                    string input = Console.ReadLine();
                    if (_stopRequested || generation != _commandLoopGeneration) break;
                    if (string.IsNullOrEmpty(input))
                    {
                        continue;
                    }
                    ExecuteCommand(input);
                }
                catch (ThreadInterruptedException)
                {
                    if (_stopRequested || generation != _commandLoopGeneration) break;
                }
                catch (Exception ex)
                {
                    if (_stopRequested || generation != _commandLoopGeneration) break;
                    Log.Error("Error in command loop", ex);
                }
            }
        }
    }
}
