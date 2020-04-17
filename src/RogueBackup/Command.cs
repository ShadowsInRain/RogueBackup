using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBackup
{
    class Command
    {
        public delegate string[] AliasesGetter();
        public readonly AliasesGetter GetAliases;

        public delegate string DescriptionGetter();
        public readonly DescriptionGetter GetDescription;

        public delegate string HelpGetter();
        public readonly HelpGetter GetHelp;

        public delegate void ExecuteDelegate(string argv, ref Repl repl);
        public readonly ExecuteDelegate Execute;

        public Command(AliasesGetter aliases, DescriptionGetter description, HelpGetter help, ExecuteDelegate execute)
        {
            GetAliases = aliases;
            GetDescription = description;
            GetHelp = help;
            Execute = execute;
        }
    }
}
