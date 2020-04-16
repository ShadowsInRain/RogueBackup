using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace RogueBackup
{
    class MainMenu : Repl
    {
        Service _service;
        Command[] _commands;
        Command[] _hiddenCommands;
        static readonly string Prompt = ": ";

        public MainMenu(UserIO userIO, Service service) : base(userIO)
        {
            _service = service;
            _commands = new Command[]
            {
                new Command(Help_Handler, Help_Description, "help", "?"),
                new Command(Intro_Handler, Intro_Description, "manual", "rtfm"),
                new Command(Exit_Handler, Exit_Description, "exit"),
                new Command(Profile_Handler, Profile_Description, "profile"),
                new Command(New_Handler, New_Description, "new"),
                new Command(Browse_Handler, Browse_Description, "browse", "explore"),
                new Command(Store_Handler, Store_Description, "store", "save"),
                new Command(Restore_Handler, Restore_Description, "restore", "load"),
            };
            _hiddenCommands = new Command[]
            {
                new Command(Fight_Handler, Fight_Description, "fight", "attack", "stab"),
            };
        }

        public override Repl Step()
        {
            var line = ReadLine(Prompt).Trim();
            var (name, argv) = line.SplitWord();
            if (name == "")
                return this;
            var command = Find(name) ?? throw new BoringException($"Unknown command: {name}");
            return command.Handler(argv);
        }

        public void Welcome()
        {
            WriteLine("Welcome to RogueBackup!");
            Profile_Handler("brief");
            WriteLine("Type '?' to list commands, type 'manual' for quick introduction.");
        }

        Command? Find(string name)
        {
            name = name.ToLower();
            foreach (var commands in new[] { _commands, _hiddenCommands })
            {
                foreach (var command in commands)
                {
                    if (command.Aliases.Contains(name))
                        return command;
                }
            }
            return null;
        }

        static readonly string Help_Description = "Show this help.";
        Repl Help_Handler(string argv)
        {
            // TODO consume argv
            WriteLine("Commands:");
            foreach (var command in _commands)
            {
                var aliasText = string.Join(", ", command.Aliases);
                WriteLine($"* {aliasText}: {command.Description}");
            }
            return this;
        }

        static readonly string Intro_Description = "Quick introduction to program, recommended for new users.";
        Repl Intro_Handler(string argv)
        {
            // TODO consume argv
            // TODO use dedicated Repl to show long text (with prompt to show more)
            const string nn = "\n\n";
            var pieces = new string[]
            {
                nn,
                "RogueBackup is low latency backup manager, initially developed to fool videogames with permadeath.",
                "Store and restore files with few keystrokes.",
                "Our main goal is response time, so capabilities are rather barebone.",
                nn,
                "It is possible to manage single file OR entire directory.",
                "To specify which file/directory (aka target) should be archived, RogueBackup requires config file.",
                "Type 'new' to create example config and show it in file explorer.",
                "Open config file with your favorite text editor, edit options to match your case.",
                "When done, type 'profile' to check if your config seems legit.",
                nn,
                "Once configured, manage your backups with commands 'store' and 'restore' or their respective aliases 'save' and 'load'.",
                nn,
                "If you want to manage multiple configurations, path to config file may be passed as command-line argument.",
                "Alternatively, manipulating working directory works as well.",
                nn,
            };
            WriteLine(string.Join(" ", pieces));
            return this;
        }

        static readonly string Exit_Description = "Exit program. Closing window works as well.";
        Repl Exit_Handler(string argv)
        {
            // TODO consume argv
            Thread.Sleep(300);
            WriteLine("Stab!");
            Thread.Sleep(300);
            return null; // exit
        }

        static readonly string Fight_Description = "Attack nearest character.";
        Repl Fight_Handler(string argv)
        {
            var sequence = new string[]
            {
                "You attack the shopkeeper.",
                "You miss!",
                "Rogue stabs you!",
                "Rogue stabs you!",
                "You are bleeding.",
                "Rogue stabs you!",
                "You are bleeding.",
                "You miss!",
                "Rogue slams door in your face.",
                "You are bleeding.",
                "You are bleeding.",
                "You are bleeding.",
                "",
                "You died.",
            };
            foreach (var line in sequence)
            {
                Thread.Sleep(1200);
                WriteLine(line);
            }
            Thread.Sleep(2000);
            return null; // exit
        }

        static readonly string Profile_Description = "Display current profile and check for errors.";
        Repl Profile_Handler(string argv)
        {
            var brief = false;
            if (argv == "brief")
                brief = true;
            else if (argv != "")
                throw new BoringException("The only option available is 'brief'");

            // TODO consume argv
            if (!_service.ProfileExists)
            {
                WriteLine($"Profile does not exists or is not accessible. Create one to proceed.");
                WriteLine($"* Expected path is {_service.ProfilePathFull}");
                return this;
            }
            var profile = _service.LoadProfile();
            var issues = new List<string>();
            profile.Validate(issues.Add);
            var hasIssues = issues.Any();

            WriteLine($"Profile '{profile.Name}' @ {profile.Origin}");
            if (!brief || hasIssues)
            {
                WriteLine($"* Target: {profile.Target}");
                WriteLine($"* Storage: {profile.Storage}");
                WriteLine($"* Capacity: {profile.Capacity}");
                WriteLine($"* Compression: {profile.Compression}");
            }

            if (hasIssues)
            {
                WriteLine("Issues detected:");
                WriteLine(string.Join("\n", issues.Select(i => $"* {i}")));
                WriteLine("Please fix listed issues before proceeding.");
            }
            else if(!brief)
            {
                WriteLine("Ok! (Profile has no obvious issues.)");
            }
            return this;
        }

        static readonly string New_Description = "Create new profile with some example defaults, if it does not exists yet.";
        Repl New_Handler(string argv)
        {
            // TODO consume argv
            if (_service.ProfileExists)
            {
                WriteLine("Profile exists already.");
                WriteLine("Please delete file manually if you want to reset existing profile.");
            }
            else
            {
                _service.ResetProfile();
                WriteLine("Profile created.");
                WriteLine("Please type 'manual' if you need further instructions.");
            }
            Browse_Handler("profile");
            return this;
        }

        static readonly string Browse_Description = "Open location in file explorer.";
        Repl Browse_Handler(string argv)
        {
            string path;
            switch (argv.ToLower())
            {
                case "target":
                case "t":
                    path = _service.LoadProfile().Target;
                    break;
                case "storage":
                case "s":
                    path = _service.LoadProfile().Storage;
                    break;
                case "profile":
                case "p":
                    path = _service.ProfilePathFull;
                    break;
                case "program":
                case "a":
                    path = AppDomain.CurrentDomain.BaseDirectory;
                    break;
                case "working":
                case "w":
                    path = Directory.GetCurrentDirectory();
                    break;
                default:
                    WriteLine("Possible options are: target (t), storage (s), profile (p), program (a), working (w)");
                    return this;
            }

            if (File.Exists(path))
            {
                System.Diagnostics.Process.Start("explorer", $"/select,{path}");
            }
            else if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start("explorer", $"{path}");
            }
            else
            {
                WriteLine($"Path does not exists: {path}");
                return this;
            }
            return this;
        }

        static readonly string Store_Description = "Store target into new archive.";
        Repl Store_Handler(string argv)
        {
            // TODO consume argv
            var profile = _service.LoadProfile();
            var id = _service.GenerateArchiveId(profile);
            WriteLine($"New id is '{id}', storing...");
            _service.Store(profile, id);
            // TODO remove old files above capacity
            WriteLine("Done");
            return this;
        }

        static readonly string Restore_Description = "Restore target from most recent archive.";
        Repl Restore_Handler(string argv)
        {
            // TODO consume argv
            var profile = _service.LoadProfile();
            var id = _service.FindMostRecentId(profile);
            if(id == null)
            {
                WriteLine("Found no matching archives!");
                return this;
            }
            WriteLine($"Latest id is '{id}', restoring...");
            // TODO option to clear destination
            _service.Restore(profile, id);
            WriteLine("Done");
            return this;
        }
    }
}
