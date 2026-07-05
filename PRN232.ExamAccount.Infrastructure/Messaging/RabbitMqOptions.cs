namespace PRN232.ExamAccount.Infrastructure.Messaging;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "grading.exchange";
    public string GradingJobsQueue { get; set; } = "grading-jobs";
    public string RegradeQueue { get; set; } = "grading-jobs.regrade";
    public string GradingResultsQueue { get; set; } = "grading-results";
    public string NewJobRoutingKey { get; set; } = "job.new";
    public string RegradeRoutingKey { get; set; } = "job.regrade";
    public string ResultRoutingKey { get; set; } = "result.done";
}
