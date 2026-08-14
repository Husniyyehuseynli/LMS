using LMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.DAL
{
    public static class SeedData
    {
        public static async Task SeedAsync(AppDbContext db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // 1) Ensure roles exist
            string[] roles = { "Admin", "Instructor", "Student", "Teacher" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2) Ensure default demo accounts exist (Admin, Instructor, Student)
            const string adminEmail = "admin@lms.com";
            const string adminPassword = "Admin123!";

            AppUser? adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    Name = "Admin",
                    Surname = "User",
                    IsAdmin = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            // Keep the demo password fixed, even if the account was created in a
            // previous run with an older demo password.
            await EnsureDemoPasswordAsync(userManager, adminUser, adminPassword);

            const string instructorEmail = "instructor@lms.com";
            const string instructorPassword = "Instructor@123";

            AppUser? instructorUser = await userManager.FindByEmailAsync(instructorEmail);
            if (instructorUser == null)
            {
                instructorUser = new AppUser
                {
                    UserName = "instructor",
                    Email = instructorEmail,
                    Name = "Michael",
                    Surname = "Turner",
                    IsInstructor = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(instructorUser, instructorPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(instructorUser, "Instructor");
                }
            }
            else if (!await userManager.IsInRoleAsync(instructorUser, "Instructor"))
            {
                await userManager.AddToRoleAsync(instructorUser, "Instructor");
            }

            const string studentEmail = "student@lms.com";
            const string studentPassword = "Student123!";

            AppUser? studentUser = await userManager.FindByEmailAsync(studentEmail);
            if (studentUser == null)
            {
                studentUser = new AppUser
                {
                    UserName = "student",
                    Email = studentEmail,
                    Name = "Emily",
                    Surname = "Parker",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(studentUser, studentPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, "Student");
                }
            }
            else if (!await userManager.IsInRoleAsync(studentUser, "Student"))
            {
                await userManager.AddToRoleAsync(studentUser, "Student");
            }
            await EnsureDemoPasswordAsync(userManager, studentUser, studentPassword);

            // 2b) Ensure a login account exists for every subject teacher, so each one can
            //     sign in and chat with students directly (like a messaging app).
            var teacherAccountInfo = new (string Email, string FirstName, string LastName, string Password)[]
            {
                ("teacher@lms.com", "Husniyya", "Huseynli", "Teacher123!"),
                ("azerbaijani.teacher@lms.com", "Elvin", "Mammadov", "Teacher@123"),
                ("english.teacher@lms.com", "Nigar", "Aliyeva", "Teacher@123"),
                ("russian.teacher@lms.com", "Anna", "Petrova", "Teacher@123"),
                ("chemistry.teacher@lms.com", "Farid", "Guliyev", "Teacher@123"),
                ("physics.teacher@lms.com", "Elshan", "Rzayev", "Teacher@123"),
                ("informatics.teacher@lms.com", "Kamran", "Isayev", "Teacher@123"),
                ("biology.teacher@lms.com", "Aygun", "Hasanova", "Teacher@123"),
                ("history.teacher@lms.com", "Tural", "Nabiyev", "Teacher@123"),
                ("geography.teacher@lms.com", "Sabina", "Orujova", "Teacher@123"),
            };

            var teacherAccounts = new Dictionary<string, AppUser>();
            foreach (var info in teacherAccountInfo)
            {
                AppUser? account = await userManager.FindByEmailAsync(info.Email);
                if (account == null)
                {
                    account = new AppUser
                    {
                        UserName = info.Email.Split('@')[0],
                        Email = info.Email,
                        Name = info.FirstName,
                        Surname = info.LastName,
                        IsTeacher = true,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(account, info.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(account, "Teacher");
                    }
                }
                else if (!await userManager.IsInRoleAsync(account, "Teacher"))
                {
                    await userManager.AddToRoleAsync(account, "Teacher");
                }

                teacherAccounts[info.Email] = account;

                // The teacher@lms.com account is the one shown in the presentation demo,
                // so its password must stay correct across re-runs, same as Admin/Student.
                if (info.Email == "teacher@lms.com")
                {
                    await EnsureDemoPasswordAsync(userManager, account, info.Password);
                }
            }

            // 3) Seed demo content only if the database has no categories yet
            if (await db.Categories.AnyAsync())
            {
                return;
            }

            var programming = new Category { Name = "Programming", Icon = CategoryIcon.Code };
            var design = new Category { Name = "Design", Icon = CategoryIcon.Design };
            var business = new Category { Name = "Business", Icon = CategoryIcon.Business };
            var marketing = new Category { Name = "Marketing", Icon = CategoryIcon.Marketing };
            var devOps = new Category { Name = "DevOps & Cloud", Icon = CategoryIcon.Cloud };
            var dataScience = new Category { Name = "Data Science", Icon = CategoryIcon.Chart };
            var entranceExam = new Category { Name = "Entrance Exam Preparation", Icon = CategoryIcon.Book };

            await db.Categories.AddRangeAsync(programming, design, business, marketing, devOps, dataScience, entranceExam);
            await db.SaveChangesAsync();

            var courses = new List<Course>
            {
                new Course
                {
                    Name = "ASP.NET Core MVC Fundamentals",
                    ShortDescription = "Learn to build dynamic web applications with ASP.NET Core MVC.",
                    Description = "This course covers the fundamentals of ASP.NET Core MVC, including controllers, views, routing, Entity Framework Core, and Identity-based authentication. By the end, you will be able to build and deploy a complete data-driven web application.",
                    InstructorName = "John Smith",
                    DurationHours = 12, Level = CourseLevel.Beginner, Language = CourseLanguage.English,
                    CategoryId = programming.Id,
                    VideoUrl = "https://www.youtube.com/embed/RWXKysImabs"
                },
                new Course
                {
                    Name = "JavaScript for Beginners",
                    ShortDescription = "A hands-on introduction to modern JavaScript.",
                    Description = "Start from the basics of variables, functions, and loops, and progress to DOM manipulation, events, and asynchronous programming with promises and async/await.",
                    InstructorName = "Sarah Lee",
                    DurationHours = 8, Level = CourseLevel.Intermediate, Language = CourseLanguage.English,
                    CategoryId = programming.Id,
                    VideoUrl = "https://www.youtube.com/embed/jS4aFq5-91M"
                },
                new Course
                {
                    Name = "SQL Server Database Design",
                    ShortDescription = "Design efficient, well-structured relational databases.",
                    Description = "Learn database normalization, keys, relationships, indexing, and how to write efficient T-SQL queries using Microsoft SQL Server.",
                    InstructorName = "Anna White",
                    DurationHours = 11, Level = CourseLevel.Beginner, Language = CourseLanguage.Azerbaijani,
                    CategoryId = programming.Id,
                    VideoUrl = "https://www.youtube.com/embed/HXV3zeQKqGY"
                },
                new Course
                {
                    Name = "UI/UX Design Basics",
                    ShortDescription = "Understand the principles of great user experience.",
                    Description = "This course introduces you to user-centered design, wireframing, prototyping, and usability testing so you can design interfaces people love to use.",
                    InstructorName = "Emily Clark",
                    DurationHours = 10, Level = CourseLevel.Advanced, Language = CourseLanguage.English,
                    CategoryId = design.Id,
                    VideoUrl = "https://www.youtube.com/embed/jwCmIBJ8Jtc"
                },
                new Course
                {
                    Name = "Graphic Design Fundamentals",
                    ShortDescription = "Master the building blocks of great visual design.",
                    Description = "Learn color theory, typography, layout, and composition, and how to apply them consistently to create clean, professional designs.",
                    InstructorName = "Laura Bennett",
                    DurationHours = 7, Level = CourseLevel.Intermediate, Language = CourseLanguage.Russian,
                    CategoryId = design.Id
                },
                new Course
                {
                    Name = "Business Strategy Essentials",
                    ShortDescription = "Core concepts every business professional should know.",
                    Description = "Explore strategic planning, competitive analysis, and decision-making frameworks used by successful organizations worldwide.",
                    InstructorName = "David Wilson",
                    DurationHours = 9, Level = CourseLevel.Beginner, Language = CourseLanguage.English,
                    CategoryId = business.Id
                },
                new Course
                {
                    Name = "Project Management Basics",
                    ShortDescription = "Plan, execute, and deliver projects on time and on budget.",
                    Description = "Learn the fundamentals of project scoping, scheduling, risk management, and team coordination using both traditional and Agile approaches.",
                    InstructorName = "Rachel Adams",
                    DurationHours = 8, Level = CourseLevel.Intermediate, Language = CourseLanguage.English,
                    CategoryId = business.Id
                },
                new Course
                {
                    Name = "Digital Marketing 101",
                    ShortDescription = "Build a solid foundation in online marketing.",
                    Description = "Learn the fundamentals of SEO, social media marketing, email campaigns, and analytics to grow an audience and drive engagement.",
                    InstructorName = "Michael Brown",
                    DurationHours = 6, Level = CourseLevel.Beginner, Language = CourseLanguage.Azerbaijani,
                    CategoryId = marketing.Id
                },
                new Course
                {
                    Name = "Social Media Marketing Essentials",
                    ShortDescription = "Grow and engage an audience across social platforms.",
                    Description = "Learn how to build a content strategy, schedule posts effectively, run basic ad campaigns, and measure what's actually working.",
                    InstructorName = "Olivia Martinez",
                    DurationHours = 5, Level = CourseLevel.Advanced, Language = CourseLanguage.English,
                    CategoryId = marketing.Id
                },
                new Course
                {
                    Name = "Docker & Containers for Beginners",
                    ShortDescription = "Package and run applications reliably with Docker.",
                    Description = "Understand containers vs virtual machines, build your own Docker images, manage volumes and networking, and orchestrate multi-container apps with Docker Compose.",
                    InstructorName = "Tom Richardson",
                    DurationHours = 10, Level = CourseLevel.Intermediate, Language = CourseLanguage.Russian,
                    CategoryId = devOps.Id,
                    VideoUrl = "https://www.youtube.com/embed/fqMOX6JJhGo"
                },
                new Course
                {
                    Name = "Cloud Computing Fundamentals",
                    ShortDescription = "Understand the core concepts behind modern cloud platforms.",
                    Description = "Learn the fundamentals of IaaS, PaaS, and SaaS, core services like compute, storage, and networking, and how to think about scalability and cost in the cloud.",
                    InstructorName = "Kevin Park",
                    DurationHours = 9, Level = CourseLevel.Beginner, Language = CourseLanguage.English,
                    CategoryId = devOps.Id
                },
                new Course
                {
                    Name = "Python Programming Fundamentals",
                    ShortDescription = "Learn Python from scratch, one project at a time.",
                    Description = "Cover core Python syntax, data structures, functions, and object-oriented programming, then apply it all by building small hands-on projects.",
                    InstructorName = "Daniel Kim",
                    DurationHours = 10, Level = CourseLevel.Intermediate, Language = CourseLanguage.English,
                    CategoryId = dataScience.Id,
                    VideoUrl = "https://www.youtube.com/embed/rfscVS0vtbw"
                },
                new Course
                {
                    Name = "Introduction to Machine Learning",
                    ShortDescription = "Understand how machines learn from data.",
                    Description = "Get an intuitive and practical introduction to supervised and unsupervised learning, common algorithms, and how to evaluate a model's performance.",
                    InstructorName = "Sophia Turner",
                    DurationHours = 12, Level = CourseLevel.Beginner, Language = CourseLanguage.Azerbaijani,
                    CategoryId = dataScience.Id
                }
            };

            await db.Courses.AddRangeAsync(courses);
            await db.SaveChangesAsync();

            // 3a) Give every regular-course instructor a real Teacher profile too, so they
            //     all show up (with a photo) on the "Our Teachers" page, not just the
            //     Entrance Exam Preparation subject teachers.
            var regularTeacherProfiles = new List<Teacher>
            {
                new Teacher { FirstName = "John", LastName = "Smith", Subject = "Programming",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/32.jpg",
                    Bio = "John Smith is a backend-focused .NET developer who has taught ASP.NET Core MVC to hundreds of students, with an emphasis on clean architecture and real project workflows." },
                new Teacher { FirstName = "Sarah", LastName = "Lee", Subject = "Programming",
                    PhotoUrl = "https://randomuser.me/api/portraits/women/32.jpg",
                    Bio = "Sarah Lee specializes in modern JavaScript and front-end fundamentals, known for breaking down tricky concepts like async/await into simple, practical examples." },
                new Teacher { FirstName = "Anna", LastName = "White", Subject = "Programming",
                    PhotoUrl = "https://randomuser.me/api/portraits/women/45.jpg",
                    Bio = "Anna White is a database specialist with years of experience designing efficient SQL Server schemas for production applications." },
                new Teacher { FirstName = "Emily", LastName = "Clark", Subject = "Design",
                    PhotoUrl = "https://randomuser.me/api/portraits/women/11.jpg",
                    Bio = "Emily Clark is a UX designer who has led usability research and interface design for multiple product teams, and loves teaching design thinking to beginners." },
                new Teacher { FirstName = "Laura", LastName = "Bennett", Subject = "Design",
                    PhotoUrl = "https://randomuser.me/api/portraits/women/52.jpg",
                    Bio = "Laura Bennett is a graphic designer with a strong background in branding, typography, and visual composition." },
                new Teacher { FirstName = "David", LastName = "Wilson", Subject = "Business",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/45.jpg",
                    Bio = "David Wilson has advised organizations on strategic planning and competitive analysis for over a decade." },
                new Teacher { FirstName = "Rachel", LastName = "Adams", Subject = "Business",
                    PhotoUrl = "https://randomuser.me/api/portraits/women/68.jpg",
                    Bio = "Rachel Adams is a certified project manager who has delivered projects using both traditional and Agile methodologies." },
                new Teacher { FirstName = "Michael", LastName = "Brown", Subject = "Marketing",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/11.jpg",
                    Bio = "Michael Brown has built digital marketing campaigns across SEO, email, and paid channels for growing businesses." },
                new Teacher { FirstName = "Olivia", LastName = "Martinez", Subject = "Marketing",
                    PhotoUrl = "https://randomuser.me/api/portraits/women/75.jpg",
                    Bio = "Olivia Martinez helps brands grow their audience and engagement across social media platforms." },
                new Teacher { FirstName = "Tom", LastName = "Richardson", Subject = "DevOps & Cloud",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/52.jpg",
                    Bio = "Tom Richardson is a DevOps engineer experienced in containerizing and deploying applications with Docker and Docker Compose." },
                new Teacher { FirstName = "Kevin", LastName = "Park", Subject = "DevOps & Cloud",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/68.jpg",
                    Bio = "Kevin Park works with cloud infrastructure daily and teaches the core concepts behind IaaS, PaaS, and SaaS platforms." },
                new Teacher { FirstName = "Daniel", LastName = "Kim", Subject = "Data Science",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/75.jpg",
                    Bio = "Daniel Kim is a Python developer who enjoys teaching programming fundamentals through small, hands-on projects." },
                new Teacher { FirstName = "Sophia", LastName = "Turner", Subject = "Data Science",
                    PhotoUrl = "https://randomuser.me/api/portraits/women/81.jpg",
                    Bio = "Sophia Turner has worked on machine learning projects across several industries and enjoys making ML concepts intuitive for beginners." },
            };

            await db.Teachers.AddRangeAsync(regularTeacherProfiles);
            await db.SaveChangesAsync();

            for (int i = 0; i < courses.Count && i < regularTeacherProfiles.Count; i++)
            {
                courses[i].TeacherId = regularTeacherProfiles[i].Id;
            }
            await db.SaveChangesAsync();

            // 3b) Entrance Exam Preparation: one teacher profile + one course per subject.
            var teacherProfiles = new List<Teacher>
            {
                new Teacher { FirstName = "Husniyya", LastName = "Huseynli", Subject = "Mathematics & Logic", Email = "teacher@lms.com", AppUserId = teacherAccounts["teacher@lms.com"].Id,
                    PhotoUrl = "husniyya-huseynli.jpg",
                    Bio = "Husniyya Huseynli has over 10 years of experience preparing students for university entrance exams in Mathematics and Logical Reasoning. Her lessons focus on problem-solving strategies, past-exam patterns, and building genuine confidence with numbers." },
                new Teacher { FirstName = "Elvin", LastName = "Mammadov", Subject = "Azerbaijani Language", Email = "azerbaijani.teacher@lms.com", AppUserId = teacherAccounts["azerbaijani.teacher@lms.com"].Id,
                    PhotoUrl = "https://randomuser.me/api/portraits/men/81.jpg",
                    Bio = "Elvin Mammadov specializes in grammar, literature analysis, and essay writing for the Azerbaijani Language entrance exam, with a strong track record of helping students raise their scores." },
                new Teacher { FirstName = "Nigar", LastName = "Aliyeva", Subject = "English Language", Email = "english.teacher@lms.com", AppUserId = teacherAccounts["english.teacher@lms.com"].Id,
                    PhotoUrl = "https://randomuser.me/api/portraits/women/23.jpg",
                    Bio = "Nigar Aliyeva teaches English grammar, vocabulary, and reading comprehension, tailored to the format and difficulty of university entrance exams." },
                new Teacher { FirstName = "Anna", LastName = "Petrova", Subject = "Russian Language", Email = "russian.teacher@lms.com", AppUserId = teacherAccounts["russian.teacher@lms.com"].Id,
                    PhotoUrl = "https://randomuser.me/api/portraits/women/59.jpg",
                    Bio = "Anna Petrova has taught Russian Language exam preparation for many years, focusing on grammar accuracy, vocabulary building, and confident written expression." },
                new Teacher { FirstName = "Farid", LastName = "Guliyev", Subject = "Chemistry", Email = "chemistry.teacher@lms.com", AppUserId = teacherAccounts["chemistry.teacher@lms.com"].Id,
                    PhotoUrl = "https://randomuser.me/api/portraits/men/23.jpg",
                    Bio = "Farid Guliyev makes Chemistry approachable through clear explanations of reactions, formulas, and problem-solving techniques aligned with the entrance exam syllabus." },
                new Teacher { FirstName = "Elshan", LastName = "Rzayev", Subject = "Physics", Email = "physics.teacher@lms.com", AppUserId = teacherAccounts["physics.teacher@lms.com"].Id,
                    PhotoUrl = "https://randomuser.me/api/portraits/men/59.jpg",
                    Bio = "Elshan Rzayev breaks down mechanics, electricity, and other core Physics topics into intuitive, exam-focused lessons with plenty of practice problems." },
                new Teacher { FirstName = "Kamran", LastName = "Isayev", Subject = "Informatics", Email = "informatics.teacher@lms.com", AppUserId = teacherAccounts["informatics.teacher@lms.com"].Id,
                    PhotoUrl = "https://randomuser.me/api/portraits/men/14.jpg",
                    Bio = "Kamran Isayev covers algorithms, logic, and computer science fundamentals for students preparing for Informatics-focused entrance exams." },
                new Teacher { FirstName = "Aygun", LastName = "Hasanova", Subject = "Biology", Email = "biology.teacher@lms.com", AppUserId = teacherAccounts["biology.teacher@lms.com"].Id,
                    PhotoUrl = "https://randomuser.me/api/portraits/women/14.jpg",
                    Bio = "Aygun Hasanova guides students through human anatomy, genetics, and ecology topics with a clear, exam-oriented approach." },
                new Teacher { FirstName = "Tural", LastName = "Nabiyev", Subject = "History", Email = "history.teacher@lms.com", AppUserId = teacherAccounts["history.teacher@lms.com"].Id,
                    PhotoUrl = "https://randomuser.me/api/portraits/men/37.jpg",
                    Bio = "Tural Nabiyev helps students master key historical events, dates, and cause-and-effect analysis for the History entrance exam." },
                new Teacher { FirstName = "Sabina", LastName = "Orujova", Subject = "Geography", Email = "geography.teacher@lms.com", AppUserId = teacherAccounts["geography.teacher@lms.com"].Id,
                    PhotoUrl = "https://randomuser.me/api/portraits/women/37.jpg",
                    Bio = "Sabina Orujova teaches physical and economic Geography with maps, real-world examples, and exam-style practice questions." },
            };

            await db.Teachers.AddRangeAsync(teacherProfiles);
            await db.SaveChangesAsync();

            var examCourses = new List<Course>
            {
                new Course { Name = "Mathematics for University Entrance Exams", ShortDescription = "Master problem-solving for the Mathematics entrance exam.",
                    Description = "A structured course covering algebra, geometry, and logical reasoning, built around real entrance-exam question patterns and timed practice.",
                    InstructorName = teacherProfiles[0].FullName, TeacherId = teacherProfiles[0].Id, DurationHours = 14, Level = CourseLevel.Advanced, Language = CourseLanguage.English, CategoryId = entranceExam.Id },
                new Course { Name = "Azerbaijani Language for Entrance Exams", ShortDescription = "Grammar, literature, and essay writing for the exam.",
                    Description = "Covers grammar rules, literary analysis, and structured essay writing techniques required for the Azerbaijani Language entrance exam.",
                    InstructorName = teacherProfiles[1].FullName, TeacherId = teacherProfiles[1].Id, DurationHours = 10, Level = CourseLevel.Intermediate, Language = CourseLanguage.Azerbaijani, CategoryId = entranceExam.Id },
                new Course { Name = "English Language for Entrance Exams", ShortDescription = "Grammar, vocabulary, and reading skills for the exam.",
                    Description = "Focuses on the grammar, vocabulary, and reading comprehension skills most commonly tested on university entrance exams.",
                    InstructorName = teacherProfiles[2].FullName, TeacherId = teacherProfiles[2].Id, DurationHours = 10, Level = CourseLevel.Beginner, Language = CourseLanguage.English, CategoryId = entranceExam.Id },
                new Course { Name = "Russian Language for Entrance Exams", ShortDescription = "Grammar accuracy and written expression practice.",
                    Description = "Builds grammar accuracy, vocabulary, and confident written expression for the Russian Language entrance exam.",
                    InstructorName = teacherProfiles[3].FullName, TeacherId = teacherProfiles[3].Id, DurationHours = 9, Level = CourseLevel.Intermediate, Language = CourseLanguage.Russian, CategoryId = entranceExam.Id },
                new Course { Name = "Chemistry for Entrance Exams", ShortDescription = "Reactions, formulas, and problem solving.",
                    Description = "Walks through reactions, chemical formulas, and calculation techniques aligned with the entrance exam syllabus.",
                    InstructorName = teacherProfiles[4].FullName, TeacherId = teacherProfiles[4].Id, DurationHours = 11, Level = CourseLevel.Beginner, Language = CourseLanguage.Azerbaijani, CategoryId = entranceExam.Id },
                new Course { Name = "Physics for Entrance Exams", ShortDescription = "Mechanics, electricity, and exam-style problems.",
                    Description = "Covers mechanics, electricity, and other core Physics topics through intuitive explanations and exam-style practice.",
                    InstructorName = teacherProfiles[5].FullName, TeacherId = teacherProfiles[5].Id, DurationHours = 11, Level = CourseLevel.Advanced, Language = CourseLanguage.English, CategoryId = entranceExam.Id },
                new Course { Name = "Informatics for Entrance Exams", ShortDescription = "Algorithms and computer science fundamentals.",
                    Description = "Covers algorithms, logic, and computer science fundamentals relevant to Informatics-focused entrance exams.",
                    InstructorName = teacherProfiles[6].FullName, TeacherId = teacherProfiles[6].Id, DurationHours = 9, Level = CourseLevel.Intermediate, Language = CourseLanguage.Russian, CategoryId = entranceExam.Id },
                new Course { Name = "Biology for Entrance Exams", ShortDescription = "Anatomy, genetics, and ecology essentials.",
                    Description = "Reviews human anatomy, genetics, and ecology topics with an exam-oriented, question-driven approach.",
                    InstructorName = teacherProfiles[7].FullName, TeacherId = teacherProfiles[7].Id, DurationHours = 10, Level = CourseLevel.Beginner, Language = CourseLanguage.English, CategoryId = entranceExam.Id },
                new Course { Name = "History for Entrance Exams", ShortDescription = "Key events, dates, and cause-and-effect analysis.",
                    Description = "Covers key historical events, timelines, and cause-and-effect analysis required for the History entrance exam.",
                    InstructorName = teacherProfiles[8].FullName, TeacherId = teacherProfiles[8].Id, DurationHours = 9, Level = CourseLevel.Intermediate, Language = CourseLanguage.English, CategoryId = entranceExam.Id },
                new Course { Name = "Geography for Entrance Exams", ShortDescription = "Physical and economic Geography with practice questions.",
                    Description = "Covers physical and economic Geography using maps, real-world examples, and exam-style practice questions.",
                    InstructorName = teacherProfiles[9].FullName, TeacherId = teacherProfiles[9].Id, DurationHours = 8, Level = CourseLevel.Beginner, Language = CourseLanguage.Azerbaijani, CategoryId = entranceExam.Id },
            };

            await db.Courses.AddRangeAsync(examCourses);
            await db.SaveChangesAsync();

            var teacherReviews = new List<TeacherReview>();
            for (int i = 0; i < teacherProfiles.Count; i++)
            {
                teacherReviews.Add(new TeacherReview
                {
                    TeacherId = teacherProfiles[i].Id,
                    StudentId = studentUser.Id,
                    Rating = 5,
                    Comment = $"{teacherProfiles[i].FirstName}'s {teacherProfiles[i].Subject} lessons were clear and well-paced. I felt much more prepared for the entrance exam."
                });
            }
            await db.TeacherReviews.AddRangeAsync(teacherReviews);
            await db.SaveChangesAsync();

            // Enroll the demo student in a couple of entrance exam courses too.
            await db.Enrollments.AddRangeAsync(
                new Enrollment { StudentId = studentUser.Id, CourseId = examCourses[0].Id },
                new Enrollment { StudentId = studentUser.Id, CourseId = examCourses[2].Id }
            );
            await db.SaveChangesAsync();

            // A short quiz for the Mathematics & Logic entrance exam course.
            var mathExamQuiz = new Quiz
            {
                Title = "Mathematics & Logic Entrance Exam Quiz",
                Description = "Practice questions covering algebra, geometry, and logical reasoning.",
                CourseId = examCourses[0].Id
            };
            await db.Quizzes.AddAsync(mathExamQuiz);
            await db.SaveChangesAsync();

            await db.Questions.AddRangeAsync(
                new Question { QuizId = mathExamQuiz.Id, Text = "If 3x + 5 = 20, what is the value of x?", OptionA = "3", OptionB = "5", OptionC = "10", OptionD = "15", CorrectOption = "B" },
                new Question { QuizId = mathExamQuiz.Id, Text = "What is the sum of the interior angles of a triangle?", OptionA = "90 degrees", OptionB = "180 degrees", OptionC = "270 degrees", OptionD = "360 degrees", CorrectOption = "B" },
                new Question { QuizId = mathExamQuiz.Id, Text = "All roses are flowers. Some flowers fade quickly. Which conclusion is logically valid?", OptionA = "All roses fade quickly", OptionB = "Some roses fade quickly", OptionC = "No valid conclusion can be drawn", OptionD = "All flowers are roses", CorrectOption = "C" }
            );
            await db.SaveChangesAsync();

            // 4) Seed quizzes with sample questions for several courses
            var mvcQuiz = new Quiz
            {
                Title = "ASP.NET Core MVC Basics Quiz",
                Description = "Test your understanding of the MVC pattern and routing.",
                CourseId = courses[0].Id
            };
            var jsQuiz = new Quiz
            {
                Title = "JavaScript Fundamentals Quiz",
                Description = "Check your knowledge of core JavaScript concepts.",
                CourseId = courses[1].Id
            };
            var sqlQuiz = new Quiz
            {
                Title = "SQL Server Basics Quiz",
                Description = "Review key database design and SQL concepts.",
                CourseId = courses[2].Id
            };
            var uiuxQuiz = new Quiz
            {
                Title = "UI/UX Design Basics Quiz",
                Description = "Test your understanding of core UI/UX principles.",
                CourseId = courses[3].Id
            };
            var dockerQuiz = new Quiz
            {
                Title = "Docker Basics Quiz",
                Description = "Check your knowledge of containers and Docker fundamentals.",
                CourseId = courses[9].Id
            };
            var pythonQuiz = new Quiz
            {
                Title = "Python Fundamentals Quiz",
                Description = "Test your understanding of core Python syntax and concepts.",
                CourseId = courses[11].Id
            };

            await db.Quizzes.AddRangeAsync(mvcQuiz, jsQuiz, sqlQuiz, uiuxQuiz, dockerQuiz, pythonQuiz);
            await db.SaveChangesAsync();

            var questions = new List<Question>
            {
                // MVC quiz
                new Question
                {
                    QuizId = mvcQuiz.Id,
                    Text = "In ASP.NET Core MVC, which component is responsible for handling user input and returning a response?",
                    OptionA = "Model",
                    OptionB = "View",
                    OptionC = "Controller",
                    OptionD = "Service",
                    CorrectOption = "C"
                },
                new Question
                {
                    QuizId = mvcQuiz.Id,
                    Text = "Which file is typically used to configure routing and middleware in a modern ASP.NET Core app?",
                    OptionA = "Startup.cs",
                    OptionB = "Program.cs",
                    OptionC = "appsettings.json",
                    OptionD = "web.config",
                    CorrectOption = "B"
                },
                new Question
                {
                    QuizId = mvcQuiz.Id,
                    Text = "What does EF Core use to track and apply changes to the database schema?",
                    OptionA = "Migrations",
                    OptionB = "Snapshots",
                    OptionC = "Triggers",
                    OptionD = "Views",
                    CorrectOption = "A"
                },

                // JavaScript quiz
                new Question
                {
                    QuizId = jsQuiz.Id,
                    Text = "Which keyword declares a block-scoped variable in JavaScript?",
                    OptionA = "var",
                    OptionB = "let",
                    OptionC = "static",
                    OptionD = "def",
                    CorrectOption = "B"
                },
                new Question
                {
                    QuizId = jsQuiz.Id,
                    Text = "Which method is used to select a single element by its id in the DOM?",
                    OptionA = "getElementById",
                    OptionB = "querySelectorAll",
                    OptionC = "getElementsByClassName",
                    OptionD = "selectById",
                    CorrectOption = "A"
                },
                new Question
                {
                    QuizId = jsQuiz.Id,
                    Text = "What does the 'await' keyword do inside an async function?",
                    OptionA = "Stops the whole application",
                    OptionB = "Pauses execution until the Promise resolves",
                    OptionC = "Declares a new variable",
                    OptionD = "Loops through an array",
                    CorrectOption = "B"
                },

                // SQL quiz
                new Question
                {
                    QuizId = sqlQuiz.Id,
                    Text = "Which SQL clause is used to filter rows before grouping?",
                    OptionA = "HAVING",
                    OptionB = "GROUP BY",
                    OptionC = "WHERE",
                    OptionD = "ORDER BY",
                    CorrectOption = "C"
                },
                new Question
                {
                    QuizId = sqlQuiz.Id,
                    Text = "What type of key uniquely identifies a row in another table?",
                    OptionA = "Primary key",
                    OptionB = "Foreign key",
                    OptionC = "Candidate key",
                    OptionD = "Composite key",
                    CorrectOption = "B"
                },
                new Question
                {
                    QuizId = sqlQuiz.Id,
                    Text = "Which normal form eliminates transitive dependencies?",
                    OptionA = "1NF",
                    OptionB = "2NF",
                    OptionC = "3NF",
                    OptionD = "BCNF",
                    CorrectOption = "C"
                },

                // UI/UX quiz
                new Question
                {
                    QuizId = uiuxQuiz.Id,
                    Text = "What is the main purpose of a wireframe?",
                    OptionA = "To finalize brand colors",
                    OptionB = "To outline layout and structure before visual design",
                    OptionC = "To write production code",
                    OptionD = "To test server performance",
                    CorrectOption = "B"
                },
                new Question
                {
                    QuizId = uiuxQuiz.Id,
                    Text = "In UX design, what does 'usability testing' primarily evaluate?",
                    OptionA = "How fast the server responds",
                    OptionB = "How real users interact with and understand a design",
                    OptionC = "The cost of the project",
                    OptionD = "The number of colors used",
                    CorrectOption = "B"
                },
                new Question
                {
                    QuizId = uiuxQuiz.Id,
                    Text = "Which term describes designing with the end user's needs as the central focus?",
                    OptionA = "Server-side rendering",
                    OptionB = "User-centered design",
                    OptionC = "Waterfall design",
                    OptionD = "Static design",
                    CorrectOption = "B"
                },

                // Docker quiz
                new Question
                {
                    QuizId = dockerQuiz.Id,
                    Text = "What is the main difference between a container and a virtual machine?",
                    OptionA = "Containers virtualize hardware, VMs virtualize the OS",
                    OptionB = "Containers share the host OS kernel, VMs virtualize full hardware",
                    OptionC = "There is no difference",
                    OptionD = "VMs are always faster than containers",
                    CorrectOption = "B"
                },
                new Question
                {
                    QuizId = dockerQuiz.Id,
                    Text = "Which file defines how a Docker image is built?",
                    OptionA = "docker-compose.yml",
                    OptionB = "Dockerfile",
                    OptionC = "package.json",
                    OptionD = "image.config",
                    CorrectOption = "B"
                },
                new Question
                {
                    QuizId = dockerQuiz.Id,
                    Text = "What command is used to run a container in detached (background) mode?",
                    OptionA = "docker run -d image",
                    OptionB = "docker start --background",
                    OptionC = "docker build -bg",
                    OptionD = "docker exec -detached",
                    CorrectOption = "A"
                },

                // Python quiz
                new Question
                {
                    QuizId = pythonQuiz.Id,
                    Text = "Which of these is a mutable data type in Python?",
                    OptionA = "Tuple",
                    OptionB = "String",
                    OptionC = "List",
                    OptionD = "Integer",
                    CorrectOption = "C"
                },
                new Question
                {
                    QuizId = pythonQuiz.Id,
                    Text = "What does the 'len()' function return when used on a list?",
                    OptionA = "The data type of the list",
                    OptionB = "The number of items in the list",
                    OptionC = "The largest item in the list",
                    OptionD = "The memory address of the list",
                    CorrectOption = "B"
                },
                new Question
                {
                    QuizId = pythonQuiz.Id,
                    Text = "Which keyword is used to define a function in Python?",
                    OptionA = "func",
                    OptionB = "function",
                    OptionC = "def",
                    OptionD = "lambda",
                    CorrectOption = "C"
                }
            };

            await db.Questions.AddRangeAsync(questions);
            await db.SaveChangesAsync();

            // 5) Enroll the demo student in several courses and record quiz results
            //    so the Student dashboard isn't empty either.
            await db.Enrollments.AddRangeAsync(
                new Enrollment { StudentId = studentUser.Id, CourseId = courses[0].Id },
                new Enrollment { StudentId = studentUser.Id, CourseId = courses[1].Id },
                new Enrollment { StudentId = studentUser.Id, CourseId = courses[3].Id },
                new Enrollment { StudentId = studentUser.Id, CourseId = courses[9].Id },
                new Enrollment { StudentId = studentUser.Id, CourseId = courses[11].Id }
            );

            await db.QuizResults.AddRangeAsync(
                new QuizResult { StudentId = studentUser.Id, QuizId = mvcQuiz.Id, CorrectCount = 2, TotalCount = 3 },
                new QuizResult { StudentId = studentUser.Id, QuizId = uiuxQuiz.Id, CorrectCount = 3, TotalCount = 3 },
                new QuizResult { StudentId = studentUser.Id, QuizId = dockerQuiz.Id, CorrectCount = 3, TotalCount = 3 },
                new QuizResult { StudentId = studentUser.Id, QuizId = pythonQuiz.Id, CorrectCount = 2, TotalCount = 3 }
            );

            await db.Reviews.AddRangeAsync(
                new Review { CourseId = courses[0].Id, StudentId = studentUser.Id, Rating = 5, Comment = "Clear explanations and practical examples. Helped me understand MVC much better." },
                new Review { CourseId = courses[1].Id, StudentId = studentUser.Id, Rating = 4, Comment = "Great pace for beginners, would like a bit more on async/await." },
                new Review { CourseId = courses[2].Id, StudentId = studentUser.Id, Rating = 5, Comment = "Exactly what I needed to understand database design." },
                new Review { CourseId = courses[3].Id, StudentId = studentUser.Id, Rating = 5, Comment = "Really well structured. The Figma video walkthrough made everything click." },
                new Review { CourseId = courses[9].Id, StudentId = studentUser.Id, Rating = 4, Comment = "Solid intro to Docker, would enjoy a follow-up course on Kubernetes." },
                new Review { CourseId = courses[11].Id, StudentId = studentUser.Id, Rating = 5, Comment = "Perfect first course in Python — the projects made it stick." }
            );

            await db.SaveChangesAsync();
        }

        // Forces a demo account's password back to the known demo value on every
        // startup, so the fixed presentation credentials (Admin/Student/Teacher)
        // always work even if the account was created earlier with a different
        // password. Only used for the three demo accounts, never for real users.
        private static async Task EnsureDemoPasswordAsync(UserManager<AppUser> userManager, AppUser user, string password)
        {
            if (await userManager.HasPasswordAsync(user))
            {
                await userManager.RemovePasswordAsync(user);
            }
            await userManager.AddPasswordAsync(user, password);
        }
    }
}
