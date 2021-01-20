using System;

namespace NickvisionTagger.Extensions
{
    public static class MediaExtensions
    {
        public static string DurationToString(this TimeSpan duration)
        {
            var seconds = (uint)duration.TotalSeconds;
            var minutes = seconds / 60;
            var hours = minutes / 60;
            return $"{hours:00}:{(minutes % 60):00}:{(seconds % 60):00}";
        }

        public static string FileSizeToString(this long fileSize)
        {
            var length = fileSize;
            string[] fileSizes = { "B", "KB", "MB", "GB", "TB" };
            var index = 0;
            while (length >= 1024 && index < 4)
            {
                index++;
                length /= 1024;
            }
            return String.Format("{0:0.##} {1}", length, fileSizes[index]);
        }
    }
}
