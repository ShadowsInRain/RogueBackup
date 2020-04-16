using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBackup
{
    class ConsoleUserIO : UserIO
    {
        public override void ReadAnyKey()
        {
            Console.Read();
        }

        public override string ReadLine(string prompt)
        {
            if (!string.IsNullOrEmpty(prompt))
                Console.Write(prompt);
            return Console.ReadLine();
        }

        public override void WriteLine(string line = "")
        {
            Console.WriteLine(line);
        }
    }
}
