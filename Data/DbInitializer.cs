using RecruitmentSystem.Models;

namespace RecruitmentSystem.Data
{
    public static class DbInitializer
    {
        public static void Seed(RecruitmentContext context)
        {
            // ===========================
            // Jobs
            // ===========================
            if (!context.Jobs.Any())
            {
                context.Jobs.Add(new Job
                {
                    JobTitle = "Software Developer",
                    JobDescription = "Graduate Software Developer",
                    ClosingDate = DateTime.Today.AddDays(30)
                });

                context.SaveChanges();
            }

            // ===========================
            // Quiz Questions will now be added by hr
            // ===========================
           /* if (!context.QuizQuestions.Any())
            {
                var questions = new List<QuizQuestion>
                {
                    new QuizQuestion
                    {
                        QuestionText = "What does CPU stand for?",
                        OptionA = "Central Processing Unit",
                        OptionB = "Computer Personal Unit",
                        OptionC = "Central Program Utility",
                        OptionD = "Computer Processing Utility",
                        CorrectAnswer = "A"
                    },

                    new QuizQuestion
                    {
                        QuestionText = "Which language is primarily used for ASP.NET Core MVC?",
                        OptionA = "Java",
                        OptionB = "Python",
                        OptionC = "C#",
                        OptionD = "PHP",
                        CorrectAnswer = "C"
                    },

                    new QuizQuestion
                    {
                        QuestionText = "What does SQL stand for?",
                        OptionA = "Structured Query Language",
                        OptionB = "Simple Question Language",
                        OptionC = "System Query Logic",
                        OptionD = "Sequential Query Language",
                        CorrectAnswer = "A"
                    },

                    new QuizQuestion
                    {
                        QuestionText = "Which SQL command retrieves data?",
                        OptionA = "INSERT",
                        OptionB = "SELECT",
                        OptionC = "DELETE",
                        OptionD = "UPDATE",
                        CorrectAnswer = "B"
                    },

                    new QuizQuestion
                    {
                        QuestionText = "Which HTML tag creates a hyperlink?",
                        OptionA = "<img>",
                        OptionB = "<a>",
                        OptionC = "<link>",
                        OptionD = "<href>",
                        CorrectAnswer = "B"
                    }
                };

                context.QuizQuestions.AddRange(questions);
                context.SaveChanges();
            }*/
        }
    }
}