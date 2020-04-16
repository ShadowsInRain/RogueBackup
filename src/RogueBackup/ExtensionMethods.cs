using System;
using System.IO;
using System.Linq;

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

        public static bool HasIllegalFilenameChars(this string name)
        {
            return name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1;
        }
    }
}
