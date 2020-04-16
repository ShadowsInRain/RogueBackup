using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBackup
{
    abstract class UserIO
    {
        public abstract string ReadLine(string prompt = null);
        public abstract void ReadAnyKey();
        public abstract void WriteLine(string line = "");
    }
}
