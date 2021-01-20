using NickvisionTagger.Extensions;
using System;
using TagLib;

namespace NickvisionTagger.Models
{
    public class MusicFile
    {
        private File _musicFile;

        public string Path { get; private set; }

        public MusicFile(string path)
        {
            Path = path;
            _musicFile = File.Create(path);
        }

        public string Filename
        {
            get => System.IO.Path.GetFileName(Path);

            set
            {
                var newPath = Path.Replace(System.IO.Path.GetFileName(Path), "") + value;
                _musicFile.Dispose();
                System.IO.File.Move(Path, newPath);
                Path = newPath;
                _musicFile = File.Create(Path);
            }
        }

        public string Title
        {
            get => _musicFile.Tag.Title;

            set => _musicFile.Tag.Title = value;
        }

        public string Artist
        {
            get => _musicFile.Tag.FirstPerformer;

            set => _musicFile.Tag.Performers = new string[] { value == null ? "" : value };
        }

        public string Album
        {
            get => _musicFile.Tag.Album;

            set => _musicFile.Tag.Album = value;
        }

        public uint Year
        {
            get => _musicFile.Tag.Year;

            set => _musicFile.Tag.Year = value;
        }

        public uint Track
        {
            get => _musicFile.Tag.Track;

            set => _musicFile.Tag.Track = value;
        }

        public string AlbumArtist
        {
            get => _musicFile.Tag.FirstAlbumArtist;

            set => _musicFile.Tag.AlbumArtists = new string[] { value == null ? "" : value };
        }

        public string Genre
        {
            get => _musicFile.Tag.FirstGenre;

            set => _musicFile.Tag.Genres = new string[] { value == null ? "" : value };
        }

        public string Comment
        {
            get => _musicFile.Tag.Comment;

            set => _musicFile.Tag.Comment = value;
        }

        public TimeSpan Duration => _musicFile.Properties.Duration;

        public string DurationAsString => Duration.DurationToString();

        public long FileSize => new System.IO.FileInfo(Path).Length;

        public string FileSizeAsString => FileSize.FileSizeToString();

        public void Save() => _musicFile.Save();

        public void RemoveTag()
        {
            _musicFile.Tag.Clear();
            _musicFile.Save();
        }

        public void FilenameToTag(string formatString)
        {
            var dashIndex = Filename.IndexOf("- ");
            if (dashIndex == -1 && formatString != "%title%")
            {
                throw new ArgumentException("Filename does not follow format string");
            }
            var extenstionIndex = Filename.ToLower().IndexOf(".mp3");
            if (formatString == "%artist%- %title%")
            {
                var artist = Filename.Substring(0, dashIndex);
                var title = Filename.Substring(dashIndex + 2, extenstionIndex - (artist.Length + 2));
                Artist = artist;
                Title = title;
            }
            else if (formatString == "%title%- %artist%")
            {
                var title = Filename.Substring(0, dashIndex);
                var artist = Filename.Substring(dashIndex + 2, extenstionIndex - (title.Length + 2));
                Title = title;
                Artist = artist;
            }
            else if (formatString == "%title%")
            {
                var title = Filename.Substring(0, extenstionIndex);
                Title = title;
            }
            _musicFile.Save();
        }

        public void TagToFilename(string formatString)
        {
            if (formatString == "%artist%- %title%")
            {
                if (string.IsNullOrEmpty(Artist) || string.IsNullOrEmpty(Title))
                {
                    throw new ArgumentException("Artist and/or title fields are empty");
                }
                Filename = $"{Artist}- {Title}.mp3";
            }
            else if (formatString == "%title%- %artist%")
            {
                if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Artist))
                {
                    throw new ArgumentException("Title and/or artist fields are empty");
                }
                Filename = $"{Title}- {Artist}.mp3";
            }
            else if (formatString == "%title%")
            {
                if (string.IsNullOrEmpty(Title))
                {
                    throw new ArgumentException("Title field is empty");
                }
                Filename = $"{Title}.mp3";
            }
        }
    }
}