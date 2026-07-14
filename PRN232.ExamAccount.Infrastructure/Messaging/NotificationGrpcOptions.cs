namespace PRN232.ExamAccount.Infrastructure.Messaging;

public class NotificationGrpcOptions
{
    public const string SectionName = "NotificationGrpc";

    public string Address { get; set; } = "http://localhost:5176";
}
