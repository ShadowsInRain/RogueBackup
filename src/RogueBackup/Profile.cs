using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace RogueBackup
{
    struct Profile
    {
        public string Origin;
        public string Name;
        public string Target;
        public string Storage;
        public int Capacity;
        public bool Compression;

        // lower-cased property names
        struct Lower
        {
            public static readonly string Name = "name";
            public static readonly string Target = "target";
            public static readonly string Storage = "storage";
            public static readonly string Capacity = "capacity";
            public static readonly string Compression = "compression";
        }

        // see ma, no dependencies!
        public static void Serialize(Profile profile, StreamWriter writer)
        {
            WriteEntry(writer, Lower.Name, profile.Name, "Profile name is used as prefix for archives.");
            WriteEntry(writer, Lower.Target, profile.Target, "Target file or directory that needs to be saved.");
            WriteEntry(writer, Lower.Storage, profile.Storage, "Directory to store backups.");
            WriteEntry(writer, Lower.Capacity, profile.Capacity, "How many backups to keep; oldest files removed first.");
            WriteEntry(writer, Lower.Compression, profile.Compression, "Use compression? Keep enabled unless performance impact is unbearable.");
            writer.Flush();
        }

        public static Profile MakeExample()
        {
            // TODO few more examples? allow user to choose?
            return Example_Noita;
        }

        static Profile Example_Noita => new Profile
        {
            Name = "Noita",
            Target = @"C:\Users\USERNAME\AppData\LocalLow\Nolla_Games_Noita\save00\",
            Storage = @"D:\Backup\Noita",
            Capacity = 25,
            Compression = true, // it's slow either way
        };

        public static Profile Deserialize(string origin, StreamReader reader)
        {
            var profile = new Profile { Origin = origin };
            profile.Deserialize(reader);
            return profile;
        }

        public void Deserialize(StreamReader reader)
        {
            var profile = new Profile { };
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine().Trim();
                if (line == "" || line.StartsWith("#"))
                    continue;
                var (key, value) = line.SplitWord();
                key = key.ToLower();

                if (key == Lower.Name)
                {
                    Name = value;
                }
                else if (key == Lower.Target)
                {
                    Target = value;
                }
                else if (key == Lower.Storage)
                {
                    Storage = value;
                }
                else if (key == Lower.Storage)
                {
                    Storage = value;
                }
                else if (key == Lower.Capacity)
                {
                    Capacity = ParseInt(key, value);
                }
                else if (key == Lower.Compression)
                {
                    Compression = ParseBool(key, value);
                }
                else
                    throw new BoringException($"Unknown profile option <{key}>");
            }
        }

        public void Validate(WriteLineDelegate report)
        {
            if (string.IsNullOrEmpty(Name))
                report("Profile name is empty");
            else if(Name.HasIllegalFilenameChars())
                report("Profile name contains invalid charactes");
            ValidatePath(report, "target", Target);
            ValidatePath(report, "storage", Storage, forceDirectory: true);
            if(Capacity < 1)
                report("Option <maxfiles> must be greater than 0.");
        }

        static void ValidatePath(WriteLineDelegate report, string key, string path, bool forceDirectory = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                report($"Path to <{key}> is empty.");
                return;
            }
            var dirExists = Directory.Exists(path);
            var fileExists = File.Exists(path);
            if (!dirExists && !fileExists)
            {
                report($"Path to <{key}> does not exists or is not accessible.");
            }
            else if(forceDirectory && fileExists)
            {
                report($"Path to <{key}> must be directory.");
            }
        }

        static void WriteEntry(StreamWriter writer, string key, object value, string comment)
        {
            writer.WriteLine($"# {comment}");
            writer.WriteLine($"{key} {value}");
            writer.WriteLine();
        }

        static int ParseInt(string key, string value)
        {
            int parsed;
            if (!int.TryParse(value, out parsed))
                throw new BoringException($"Profile option <{key}> must be integer");
            return parsed;
        }

        static bool ParseBool(string key, string value)
        {
            value = value.ToLower();
            if (value == "false") return false;
            if(value == "true") return true;
            throw new BoringException($"Profile option <{key}> must be boolean (true/false)");
        }
    }
}
