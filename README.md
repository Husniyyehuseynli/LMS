# LMS — Learning Management System (Courses & Quizzes)

An ASP.NET Core MVC (.NET 10) course and quiz management system. The project structure
(BaseEntity, Areas/Admin, Identity, soft-delete) follows the GameLuxBPA101 sample pattern;
the design is adapted from the eLearning HTML template (Bootstrap 5).

## Features

- Course catalog with categories, images, instructor info
- Student enrollment, quizzes with instant scoring, and a personal dashboard
- **Star ratings & written reviews** — enrolled students can rate and review a course; the average rating shows on the course page
- **Certificates** — once a student passes all quizzes in a course (60%+) **and** completes every lesson, a "Get Certificate" button unlocks a printable/PDF-savable certificate page
- **Built-in AI assistant widget** — a floating chat button (bottom-right on every page) that answers common questions about registration, courses, quizzes, certificates, reviews, teachers, and entrance exam subjects. It works out of the box with no external API key or cost, using a small built-in FAQ matcher.
- **Teachers** — dedicated profiles for Entrance Exam Preparation subject teachers (bio, subject, courses taught, star ratings & reviews from students)
- **Direct messaging (Chat)** — a WhatsApp-style inbox where logged-in students and teachers can message each other directly, with live-updating conversation threads
- **Lesson progress tracking** — enrolled students can check off each lesson as completed on the Course Details "Curriculum" tab (AJAX, no page reload); a progress bar there and on the Dashboard course cards shows the live % complete, and it now gates certificate unlocking alongside the quiz requirement. The Admin dashboard shows an "Avg. Lesson Completion" stat across all enrollments, and each course has a detailed **Progress report** (Admin → Courses → Progress) listing every enrolled student's per-lesson checkmarks and overall %. Teachers with a linked login (`Teacher.AppUserId`) get their own read-only **"My Students' Progress"** page (top-right menu) scoped to just the courses they teach.
- **In-app notifications** — a bell icon in the navbar (polls every 30s) shows unread alerts: a student is notified when they finish every lesson in a course and again when their certificate becomes available; the course's teacher (if linked) is notified when a student finishes their course. Full history, filter tabs (all/unread/read), and mark-as-read live at `/Notification`.
- **Premium visual theme** — the generic template teal has been rebranded to an indigo/gold palette (`wwwroot/css/theme-boost.css`) with consistent shadows, rounded corners, and hover lift on cards. The homepage hero now has Azerbaijani branded copy plus animated stat counters, and key sections use scroll-reveal fade-ins (`data-reveal`, vanilla JS in `main.js` — no extra libraries). AJAX actions (like completing a lesson) show a small corner toast (`window.lmsToast(...)`) instead of relying on a full page reload.
- Full Admin/Instructor panel for Category, Course, Quiz, Question, and **Teacher** management

## ⚠️ Important — new migration required

This version adds `Teachers`, `TeacherReviews`, `ChatMessages`, `LessonProgresses`, and
`Notifications` tables, plus a `TeacherId` column on `Courses` and an `IsTeacher` column
on users, so the database model changed since your last migration. Before running the
app, open the Package Manager Console and run:

```
Add-Migration AddTeachersChatProgressAndNotifications
```

Then just run the app (F5) — `Database.MigrateAsync()` in `Program.cs` will apply it
automatically, no need to run `Update-Database` by hand.

If you'd rather start clean, you can instead run `Drop-Database`, delete everything in the
`Migrations` folder, run `Add-Migration Initial` again, and then F5.

## Setup

1. Open `LMS.csproj` in Visual Studio (NuGet packages will restore automatically:
   Microsoft.AspNetCore.Identity.EntityFrameworkCore, EntityFrameworkCore,
   EntityFrameworkCore.SqlServer, EntityFrameworkCore.Tools).
2. In `appsettings.json`, check the connection string matches your environment
   (default: local SQL Server instance).
3. In the Package Manager Console, run:
   ```
   Add-Migration Initial
   ```
   You only need to do this once. From then on, the database is created and updated
   **automatically** every time you run the app (see `Program.cs` — it calls
   `Database.MigrateAsync()` on startup), so you no longer need to run `Update-Database`
   by hand.
4. Run the project (F5).

## Demo data (seeded automatically)

On first run, the app automatically creates:

- The **Admin / Instructor / Student / Teacher** roles
- Three ready-to-use demo accounts:

  | Role       | Email                | Password         |
  |------------|-----------------------|-------------------|
  | Admin      | admin@lms.com         | Admin@12345       |
  | Instructor | instructor@lms.com    | Instructor@123    |
  | Student    | student@lms.com       | Student@123       |

- **10 Teacher accounts**, one per Entrance Exam Preparation subject (all use password
  `Teacher@123`). Log in as any of them to see the Teacher inbox and reply to student
  messages:

  | Subject                | Teacher            | Email                          |
  |-------------------------|--------------------|---------------------------------|
  | Mathematics & Logic     | Huseynli Husniyya  | husniyya.huseynli@lms.com      |
  | Azerbaijani Language    | Elvin Mammadov     | azerbaijani.teacher@lms.com    |
  | English Language        | Nigar Aliyeva      | english.teacher@lms.com        |
  | Russian Language        | Anna Petrova       | russian.teacher@lms.com        |
  | Chemistry                | Farid Guliyev      | chemistry.teacher@lms.com      |
  | Physics                  | Elshan Rzayev      | physics.teacher@lms.com        |
  | Informatics               | Kamran Isayev      | informatics.teacher@lms.com    |
  | Biology                  | Aygun Hasanova     | biology.teacher@lms.com        |
  | History                  | Tural Nabiyev      | history.teacher@lms.com        |
  | Geography                | Sabina Orujova     | geography.teacher@lms.com      |

- Sample **Categories** (7): Programming, Design, Business, Marketing, DevOps & Cloud,
  Data Science, **Entrance Exam Preparation**
- Sample **Courses** (23): the original 13, plus 10 Entrance Exam Preparation courses
  (Mathematics & Logic, Azerbaijani, English, Russian, Chemistry, Physics, Informatics,
  Biology, History, Geography), each linked to its own Teacher profile
- **7 Quizzes** with multiple-choice questions, including a Mathematics & Logic
  entrance-exam quiz
- The demo Student is enrolled in 7 courses (including 2 entrance exam subjects), has
  passing quiz results on several of them, and has left star ratings/reviews on 6 courses
  **and all 10 teachers** — so the dashboard, certificates, teacher profiles, and reviews
  all have real content to show right away.

This means the site is populated and fully functional the moment you run it —
no manual data entry needed for a demo. You can log in with the Admin account above
to manage everything from `/Admin/Dashboard`, or register a new account as a regular
visitor to enroll in courses and take quizzes as a Student.

Seeding is idempotent: it only inserts demo content if the `Categories` table is
empty, so it won't duplicate data on every restart, and it won't touch anything
you've added or changed yourself.

## Project structure

```
LMS/
├── Areas/Admin/            → Admin/Instructor panel (Category, Course, Quiz, Question, Teacher CRUD)
├── Controllers/            → Public side (Home, Account, Course, Teacher, Chat, Dashboard, Quiz, Review, LessonProgress)
├── DAL/AppDbContext.cs
├── DAL/SeedData.cs         → Seeds roles, demo accounts (incl. 10 teacher accounts), and demo content
├── Models/                 → Category, Course, Lesson, Teacher, TeacherReview, ChatMessage, Quiz, Question, Enrollment, QuizResult, LessonProgress, Notification, AppUser
├── ViewModels/Account/     → Login/Register
├── Utilites/                → FileUploadExtension (image upload)
├── Views/                  → Public Razor pages (eLearning design), incl. Teacher and Chat views
└── wwwroot/                 → eLearning template's css/js/img/lib assets
```

## Main flow

- **Visitor:** Home → Courses / Teachers → Course or Teacher Details
- **Student:** Register/Login → Course Details → Enroll → Take Quiz → Result → Dashboard ("My Courses"). Also: Teachers → pick a teacher → Message (Chat) or leave a review.
- **Teacher:** Login → Messages inbox → reply to students who've messaged them.
- **Admin/Instructor:** Login → `/Admin/Dashboard` → manage Categories/Courses/Quizzes/Teachers → manage Questions for each quiz

## Notes

- Course images upload to `wwwroot/uploads/courses/` (max 2MB, image formats only).
  Seeded demo courses have no uploaded image, so they fall back to the template's
  placeholder images automatically.
- All delete operations are **soft-deletes** (`IsDeleted = true`), with a Restore action
  available in each admin list.
- Quiz results are scored as a percentage: ≥60% shows as "Passed" (green), below that
  as "Failed" (red).
