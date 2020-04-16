using System;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

namespace RogueBackup
{
    class Service
    {
        Config _config;

        string ArchiveExtension => _config.ArchiveExtension;
        public string ProfilePathFull => Path.GetFullPath(_config.ProfilePath);
        public bool ProfileExists => File.Exists(ProfilePathFull);

        public Service(Config config)
        {
            _config = config;
        }

        public Profile LoadProfile()
        {
            var path = ProfilePathFull;
            if (!File.Exists(path))
                throw new BoringException("Profile does not exists");
            using var file = File.OpenRead(path);
            var profile = Profile.Deserialize(path, new StreamReader(file));
            return profile;
        }

        public void ResetProfile()
        {
            var profile = Profile.MakeExample();
            using var file = File.OpenWrite(ProfilePathFull);
            Profile.Serialize(profile, new StreamWriter(file));
        }

        public string GenerateNewArchiveName(Profile profile, string suffix)
        {
            if (!string.IsNullOrEmpty(suffix))
                suffix = " " + suffix.Trim();
            var now = DateTime.Now;
            return $"{profile.Name}-{now:yyyyMMdd}-{now:HHmmss}{suffix}.{ArchiveExtension}";
        }

        public void Store(Profile profile, string name)
        {
            var destination = Path.Join(profile.Storage, name);
            var compression = profile.Compression ? CompressionLevel.Optimal : CompressionLevel.NoCompression;
            if (File.Exists(profile.Target))
            {
                using var zip = ZipFile.Open(destination, ZipArchiveMode.Create);
                zip.CreateEntryFromFile(profile.Target, Path.GetFileName(profile.Target), compression);
            }
            else if (Directory.Exists(profile.Target))
            {
                ZipFile.CreateFromDirectory(profile.Target, destination, compression, false);
            }
            else
            {
                throw new BoringException("Backup target does not exists or is not accessible");
            }
        }

        public string FindMostRecentArchiveName(Profile profile, string suffix)
        {
            var q = new DirectoryInfo(profile.Storage).GetFiles($"{profile.Name }-*.{ArchiveExtension}") as IEnumerable<FileInfo>;
            if (!string.IsNullOrEmpty(suffix))
                q = q.Where(f => f.Name.Contains(suffix, StringComparison.OrdinalIgnoreCase));
            var last = q.OrderBy(f => f.CreationTime).LastOrDefault();
            return last?.Name;
        }

        public void Restore(Profile profile, string name)
        {
            var source = Path.Join(profile.Storage, name);
            var destination = Directory.Exists(profile.Target) ? profile.Target : Directory.GetParent(profile.Target).FullName;
            ZipFile.ExtractToDirectory(source, destination, true);
        }
    }
}
