namespace LMS.Utilites
{
    public static class TeacherPhotoHelper
    {
        // Cycles through 8 existing template portrait images (team-1..4, testimonial-1..4)
        // so teachers without any photo at all fall back to something instead of a broken image.
        public static string GetFallbackPhotoPath(int id)
        {
            int slot = id % 8;
            return slot < 4
                ? $"~/img/team-{slot + 1}.jpg"
                : $"~/img/testimonial-{slot - 3}.jpg";
        }

        // Resolves whatever is stored in Teacher.PhotoUrl into something an <img src> can use:
        // - empty/null            -> the alternating fallback above
        // - starts with http(s)   -> used as-is (e.g. a distinct stock portrait per teacher)
        // - anything else         -> treated as a filename under ~/uploads/teachers/
        public static string GetDisplayPhotoUrl(int teacherId, string? photoUrl)
        {
            if (string.IsNullOrWhiteSpace(photoUrl))
            {
                return GetFallbackPhotoPath(teacherId);
            }

            if (photoUrl.StartsWith("http://") || photoUrl.StartsWith("https://"))
            {
                return photoUrl;
            }

            return "~/uploads/teachers/" + photoUrl;
        }
    }
}
