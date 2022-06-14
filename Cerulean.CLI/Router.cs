﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Cerulean.CLI.Commands;
using Cerulean.CLI.Extensions;

namespace Cerulean.CLI
{
    internal class Router
    {
        private static Router? _router = null;
        private IDictionary<string, Delegate> _commands;

        private Router()
        {
            _commands = new Dictionary<string, Delegate>();
        }

        public static Router GetRouter()
        {
            if (_router is null)
                _router = new Router();
            return _router;
        }

        public void RegisterCommand(string commandName, Action<string[]> action)
        {
            if (_commands is null)
                throw new InvalidOperationException("Object not yet initialized.");
            _commands[commandName] = action;
        }

        public void RegisterCommands()
        {
            var interfaceType = typeof(ICommand);
            var commands = new List<Type>();
            var loadable = typeof(BuildXML).IsAssignableTo(interfaceType);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetLoadableTypes())
                {
                    if (type is not null
                        && type.IsAssignableTo(interfaceType)
                        && type != typeof(object)
                        && type != interfaceType)
                        commands.Add(type);
                }
            }
            //var commands = AppDomain.CurrentDomain.GetAssemblies()
                //.SelectMany(assembly => assembly.GetLoadableTypes())
                //.Where(x => x is not null && x != interfaceType && x.IsAssignableFrom(interfaceType));
            foreach (var command in commands)
            {
                var commandName = (string?)command?.GetProperty("CommandName")?.GetValue(null);
                var action = command?.GetMethod("DoAction")?.CreateDelegate(typeof(Action<string[]>));
                if (commandName is not null && action is not null)
                {
                    _commands[commandName] = action;
                } else
                {
                    Console.WriteLine($"[Router.RegisterCommands()] Could not load command \"{command?.Name}\".");
                }
            }
        }

        public bool ExecuteCommand(string commandName, params string[] args)
        {
            if (_commands.TryGetValue(commandName, out Delegate? command))
            {
                command?.DynamicInvoke(new[]{ args });
                return true;
            }
            return false;
        }
    }
}
