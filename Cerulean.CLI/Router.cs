using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI
{
    internal class Router
    {
        private static Router? _router = null;
        private IDictionary<string, Command> _commands;

        private Router()
        {
            _commands = new Dictionary<string, Command>();
        }

        public static Router GetRouter()
        {
            if (_router is null)
                _router = new Router();
            return _router;
        }

        public Command RegisterCommand(string commandName, Action<string[]> action)
        {
            if (_commands is null)
                throw new InvalidOperationException("Object not yet initialized.");
            _commands[commandName] = new(commandName, action);
            return _commands[commandName];
        }

        public bool ExecuteCommand(string commandName, params string[] args)
        {
            if (_commands.TryGetValue(commandName, out Command? command))
            {
                if (command is null)
                    return false;
                command.DoAction(args);
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
