using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBackup
{
    abstract class Repl
    {
        readonly UserIO _userIO;

        public Repl(UserIO userIO)
        {
            _userIO = userIO;
        }

        protected string ReadLine(string prompt = null) => _userIO.ReadLine(prompt);
        protected void ReadAnyKey() => _userIO.ReadAnyKey();
        protected void WriteLine(string line = "") => _userIO.WriteLine(line);

        public abstract Repl Step();
    }
}
