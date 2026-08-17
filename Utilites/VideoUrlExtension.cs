using System.Text.RegularExpressions;

namespace LMS.Utilites
{
    public static class VideoUrlExtension
    {
        public static string? ToEmbedUrl(this string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return url;

            url = url.Trim();

            if (url.Contains("youtube.com/embed/"))
            {
                return url;
            }

            var watchMatch = Regex.Match(url, @"[?&]v=([a-zA-Z0-9_-]{6,})");
            if (watchMatch.Success)
            {
                return $"https://www.youtube.com/embed/{watchMatch.Groups[1].Value}";
            }

            var shortMatch = Regex.Match(url, @"youtu\.be/([a-zA-Z0-9_-]{6,})");
            if (shortMatch.Success)
            {
                return $"https://www.youtube.com/embed/{shortMatch.Groups[1].Value}";
            }

            return url;
        }
    }
}
