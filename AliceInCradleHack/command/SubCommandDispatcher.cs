using System;
using System.Collections.Generic;
using System.Linq;

namespace AliceInCradleHack.command
{
    /// <summary>
    /// Base class for commands that dispatch to named sub-commands.
    /// </summary>
    public abstract class SubCommandDispatcher : Command
    {
        private readonly Dictionary<string, Action<string[]>> _subCommands = new(StringComparer.OrdinalIgnoreCase);

        protected void RegisterSubCommand(string name, Action<string[]> handler)
        {
            _subCommands[name] = handler;
        }

        public override void Execute(string[] args)
        {
            if (args.Length == 0 || !_subCommands.TryGetValue(args[0], out var handler))
            {
                Console.WriteLine(Usage);
                return;
            }
            handler(args.Skip(1).ToArray());
        }
    }
}
