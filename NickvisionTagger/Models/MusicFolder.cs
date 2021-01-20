using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NickvisionTagger.Models
{
    public class MusicFolder
    {
        public string FolderPath { get; set; }

        public bool IncludeSubfolders { get; set; }
        public List<MusicFile> Files { get; private set; }

        public MusicFolder(string folderPath, bool includeSubfolders)
        {
            FolderPath = folderPath;
            IncludeSubfolders = includeSubfolders;
            Files = new List<MusicFile>();
        }

        public void ReloadFiles()
        {
            var extensions = new List<string>() { ".mp3", ".wav", ".wma", ".ogg", ".flac" };
            var searchOption = IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            Files.Clear();
            if (Directory.Exists(FolderPath))
            {
                foreach (var filePath in Directory.EnumerateFiles(FolderPath, "*.*", searchOption).Where(file => extensions.Contains(Path.GetExtension(file))))
                {
                    Files.Add(new MusicFile(filePath));
                }
            }
            Files.Sort((f1, f2) => string.Compare(f1.Filename, f2.Filename));
        }
    }
}
