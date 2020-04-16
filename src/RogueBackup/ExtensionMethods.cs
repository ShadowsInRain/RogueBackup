using System;
using System.Linq;
using System.Collections.Generic;

namespace RogueBackup
{
    static class ExtensionMethods
    {
        public static (string, string) SplitWord(this string source)
        {
            var head = string.Concat(source.TakeWhile(c => !Char.IsWhiteSpace(c))).Trim();
            var rest = string.Concat(source.Skip(head.Length)).Trim();
            return (head, rest);
        }
    }
}
