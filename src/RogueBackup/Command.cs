using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBackup
{
    struct Command
    {
        public readonly string[] Aliases;
        public string Description;
        public readonly CommandDelegate Handler;

        public Command(CommandDelegate handler, string description, params string[] aliases)
        {
            Handler = handler;
            Description = description;
            Aliases = aliases;
        }
    }
}
