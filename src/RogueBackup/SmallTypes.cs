using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBackup
{
    delegate void WriteLineDelegate(string text = "");
    delegate Repl CommandDelegate(string line);
}
