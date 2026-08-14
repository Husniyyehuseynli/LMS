using LMS.Models;

namespace LMS.Utilites
{
    // Generates the "What You'll Learn" and "Requirements" lists shown on the
    // Course Details page. No dedicated database columns exist for these yet —
    // this keeps the generation logic in one place so it can later be swapped
    // for real admin-managed fields (e.g. Course.LearningOutcomes) without
    // touching the view.
    public static class CourseContentExtension
    {
        public static List<string> GetWhatYoullLearn(this Course course)
        {
            var fromDescription = (course.Description ?? string.Empty)
                .Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 15)
                .Take(4)
                .ToList();

            if (fromDescription.Count >= 2) return fromDescription;

            string subject = course.Category?.Name ?? "the subject";
            return new List<string>
            {
                $"Build a solid, practical understanding of {subject}",
                $"Work through {course.Level.ToString().ToLower()}-level examples step by step",
                "Learn directly from an experienced instructor",
                "Test your knowledge with the course quizzes"
            };
        }

        public static List<string> GetRequirements(this Course course)
        {
            string subject = course.Category?.Name ?? "the subject";

            return course.Level switch
            {
                CourseLevel.Beginner => new List<string>
                {
                    "No prior experience required",
                    "A computer with an internet connection",
                    "Willingness to learn and practice"
                },
                CourseLevel.Intermediate => new List<string>
                {
                    $"Basic understanding of {subject}",
                    "A computer with an internet connection",
                    "Comfortable with the fundamentals of the field"
                },
                _ => new List<string>
                {
                    $"Solid working experience in {subject}",
                    "A computer with an internet connection",
                    "Comfortable working independently on advanced tasks"
                }
            };
        }
    }
}
