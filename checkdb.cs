using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PRN232.ExamAccount.Infrastructure.Persistence;

namespace CheckDB {
    class Program {
        static void Main() {
            var options = new DbContextOptionsBuilder<ExamAccountDbContext>()
                .UseNpgsql("Host=localhost;Database=exam_account_db;Username=postgres;Password=postgres")
                .Options;
            using var db = new ExamAccountDbContext(options);
            var attempt = db.GradingAttempts.OrderByDescending(x => x.CompletedAtUtc).FirstOrDefault();
            if (attempt != null) {
                Console.WriteLine("Score: " + attempt.TotalScore);
                Console.WriteLine("Error: " + attempt.ErrorMessage);
                Console.WriteLine("Report: " + attempt.RawJsonReport);
            } else {
                Console.WriteLine("No attempts found.");
            }
        }
    }
}
