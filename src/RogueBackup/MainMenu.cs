using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace RogueBackup
{
    class MainMenu : Repl
    {
        Config _config;
        Service _service;
        Command[] _commands;
        const string Prompt = ": ";

        public MainMenu(UserIO userIO, Config config, Service service) : base(userIO)
        {
            _config = config;
            _service = service;
            _commands = new Command[]
            {
                Help_Command,
                Manual_Command,
                Exit_Command,
                Profile_Command,
                Switch_Command,
                New_Command,
                Explore_Command,
                Find_Command,
                Store_Command,
                Restore_Command,
            };
        }

        public override Repl Step()
        {
            var line = ReadLine(Prompt).Trim();
            return Execute(line);
        }

        Repl Execute(string line)
        {
            var (name, argv) = line.SplitWord();
            if (name == "")
                return this;
            var command = Find(name) ?? throw new BoringException($"Unknown command: {name}");
            if (Help_Aliases().Contains(argv))
            {
                command = Help_Command;
                argv = name;
            }
            var repl = this as Repl;
            command.Execute(argv, ref repl);
            return repl;
        }

        void Execute(Command command, string argv = "")
        {
            var name = command.GetAliases().First();
            Execute($"{name} {argv}");
        }

        public void Welcome()
        {
            WriteLine("Welcome to RogueBackup!");
            Execute(Profile_Command, "brief");
            WriteLine("Type '?' to list commands, type 'manual' for quick introduction.");
        }

        Command Find(string name)
        {
            name = name.ToLower();
            foreach (var command in _commands)
            {
                if (command.GetAliases().Contains(name))
                    return command;
            }
            return null;
        }

        Command Help_Command => new Command(Help_Aliases, Help_Description, Help_Help, Help_Execute);
        string[] Help_Aliases() => new[] { "help", "?" };
        string Help_Description() => "List all commands or show help for specific command.";
        string Help_Help() => "Type 'help [<command>]' or '<command> help' to show help for given command.";
        void Help_Execute(string argv, ref Repl repl)
        {
            var name = argv;
            var specific = null as Command;
            if (!string.IsNullOrEmpty(name))
            {
                specific = Find(name);
                if (specific == null)
                {
                    WriteLine($"Unknown command '{name}'");
                    return;
                }
            }
            if(specific != null)
            {
                var trueName = specific.GetAliases().First();
                if (trueName != name)
                    WriteLine($"'{name}' is an alias for '{trueName}'");
                WriteLine(specific.GetHelp() ?? specific.GetDescription());
            }
            else
            {
                WriteLine(Help_Help());
                WriteLine("All commands:");
                foreach (var command in _commands)
                {
                    var aliasText = string.Join(" or ", command.GetAliases().Select(a => $"'{a}'"));
                    WriteLine($"* {aliasText}: {command.GetDescription()}");
                }
            }
            return;
        }

        Command Manual_Command => new Command(Manual_Aliases, Manual_Description, Manual_Help, Manual_Execute);
        string[] Manual_Aliases() => new[] { "manual" };
        string Manual_Description() => "Show quick introduction, recommended for new users.";
        string Manual_Help() => null;
        void Manual_Execute(string argv, ref Repl repl)
        {
            // TODO consume argv
            // TODO use dedicated Repl to show long text (with prompt to show more)
            const string NN = "\n\n";
            var pieces = new string[]
            {
                NN,
                "RogueBackup is low latency backup manager, initially developed to fool videogames with permadeath.",
                "Store and restore files with few keystrokes.",
                "Our main goal is response time, so capabilities are rather barebone.",
                NN,
                "It is possible to manage single file OR entire directory.",
                "To specify which file/directory (aka target) should be archived, RogueBackup requires config file.",
                "Type 'new' to create example config and show it in file explorer.",
                "Open config file with your favorite text editor, edit options to match your case.",
                "When done, type 'profile' to check if your config seems legit.",
                NN,
                "Once configured, manage your backups with commands 'store' and 'restore' or their respective aliases 'save' and 'load'.",
                NN,
                "If you want to manage multiple configurations, path to config file may be passed as command-line argument.",
                "Alternatively, manipulating working directory works as well.",
                NN,
            };
            WriteLine(string.Join(" ", pieces));
        }

        Command Exit_Command => new Command(Exit_Aliases, Exit_Description, Exit_Help, Exit_Execute);
        string[] Exit_Aliases() => new[] { "exit" };
        string Exit_Description() => "Exit program. Closing window works as well.";
        string Exit_Help() => null;
        void Exit_Execute(string argv, ref Repl repl)
        {
            if (!string.IsNullOrEmpty(argv))
                throw new BoringException("Too many arguments.");
            Thread.Sleep(300);
            WriteLine("Stab!");
            Thread.Sleep(300);
            repl = null; // exit
        }

        Command Profile_Command => new Command(Profile_Aliases, Profile_Description, Profile_Help, Profile_Execute);
        string[] Profile_Aliases() => new[] { "profile" };
        string Profile_Description() => "Display current profile and check for errors.";
        string Profile_Help() => "'profile [brief]'\nShow current profile options and check for errors. In brief mode, only profile origin will be shown, unless there are errors.";
        void Profile_Execute(string argv, ref Repl repl)
        {
            var brief = false;
            if (argv == "brief")
                brief = true;
            else if (argv != "")
                throw new BoringException("The only argument accepted is 'brief'");

            if (!_service.ProfileExists)
            {
                WriteLine($"Profile does not exists or is not accessible. Create one to proceed.");
                WriteLine($"* Expected path is {_service.ProfilePathFull}");
                return;
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
            else if (!brief)
            {
                WriteLine("Ok! (Profile has no obvious issues.)");
            }
        }

        Command Switch_Command => new Command(Switch_Aliases, Switch_Description, Switch_Help, Switch_Execute);
        string[] Switch_Aliases() => new[] { "switch" };
        string Switch_Description() => "Switch current profile.";
        string Switch_Help() => "'switch <path>'\nSwitch current profile.";
        void Switch_Execute(string argv, ref Repl repl)
        {
            var path = argv;
            if (!File.Exists(path))
                throw new BoringException("File not found");
            _config.ProfilePath = path;
            Execute(Profile_Command, "brief");
        }

        Command New_Command => new Command(New_Aliases, New_Description, New_Help, New_Execute);
        string[] New_Aliases() => new[] { "new" };
        string New_Description() => "Create new profile with some example defaults.";
        string New_Help() => "'new [<path>]'\nCreate (and switch to) new profile, but only if it does not exists already. Profile location will be revealed in file explorer.";
        void New_Execute(string argv, ref Repl repl)
        {
            var path = argv;
            if (!string.IsNullOrEmpty(path))
                _config.ProfilePath = path;
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
            Execute(Explore_Command, "p");
        }

        Command Explore_Command => new Command(Explore_Aliases, Explore_Description, Explore_Help, Explore_Execute);
        string[] Explore_Aliases() => new[] { "explore" };
        string Explore_Description() => "Open location in file explorer.";
        string Explore_Help() => "'explore t|s|p|a|cwd'\nReveal location in file explorer. Possible locations are:\n* 't' or 'target'\n* 's' or 'storage'\n* 'p' or 'profile'\n* 'a' or 'program'\n* 'cwd' (current working directory)";
        void Explore_Execute(string argv, ref Repl repl)
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
                case "cwd":
                    path = Directory.GetCurrentDirectory();
                    break;
                case "":
                    throw new BoringException("Too few arguments");
                default:
                    throw new BoringException("Unknown option");
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
            }
        }

        Command Find_Command => new Command(Find_Aliases, Find_Description, Find_Help, Find_Execute);
        string[] Find_Aliases() => new[] { "find" };
        string Find_Description() => "Find archives matching query.";
        string Find_Help() => "'find [<suffix>]'\nLists all archives matching current profile. Specify 'suffix' to filter by name. Files sorted by date, most recent last.";
        void Find_Execute(string argv, ref Repl repl)
        {
            var suffix = argv;
            var profile = _service.LoadProfile();
            var names = _service.FindRelevantArchivesByName(profile, suffix);
            if (names.Any())
            {
                foreach (var name in names)
                    WriteLine(name);
            }
            else
                WriteLine("Nothing found");
        }

        Command Store_Command => new Command(Store_Aliases, Store_Description, Store_Help, Store_Execute);
        string[] Store_Aliases() => new[] { "store", "save" };
        string Store_Description() => "Store target into new archive.";
        string Store_Help() => "'store [<suffix>]'\nCreate new archive from target and put it into storage directory. If 'suffix' is specified, it will be added to archive name; use it as tag or commentary.";
        void Store_Execute(string argv, ref Repl repl)
        {
            var suffix = argv;
            var profile = _service.LoadProfile();
            var name = _service.GenerateNewArchiveName(profile, suffix);
            WriteLine($"Creating {name}");
            _service.Store(profile, name);
            // TODO remove old files above capacity
            WriteLine("Done");
        }

        Command Restore_Command => new Command(Restore_Aliases, Restore_Description, Restore_Help, Restore_Execute);
        string[] Restore_Aliases() => new[] { "restore", "load" };
        string Restore_Description() => "Restore target from most recent archive.";
        string Restore_Help() => "'restore [<suffix>]'\nRetore target from most recent archive. Selects from archives matching profile name, specify 'suffix' for further filtering.";
        void Restore_Execute(string argv, ref Repl repl)
        {
            var suffix = argv;
            var profile = _service.LoadProfile();
            var names = _service.FindRelevantArchivesByName(profile, suffix);
            if(!names.Any())
            {
                WriteLine("Found no matching archives!");
                return;
            }
            var name = names.Last();
            WriteLine($"Restoring {name}");
            // TODO option to clear destination
            _service.Restore(profile, name);
            WriteLine("Done");
        }
    }
}
