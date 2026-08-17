namespace LMS.Utilites
{
    public static class TeacherPhotoHelper
    {
        public static string GetFallbackPhotoPath(int id)
        {
            int slot = id % 8;
            return slot < 4
                ? $"~/img/team-{slot + 1}.jpg"
                : $"~/img/testimonial-{slot - 3}.jpg";
        }

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
