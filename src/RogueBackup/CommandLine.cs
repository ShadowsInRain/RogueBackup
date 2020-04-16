using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBackup
{
    static class CommandLine
    {
        public static void Parse(string[] args, Config config)
        {
            if (args.Length == 0)
                return;
            if (args.Length == 1)
            {
                config.ProfilePath = args[0];
                return;
            }
            throw new BoringException("Too many command-line arguments.");
        }
    }
}
