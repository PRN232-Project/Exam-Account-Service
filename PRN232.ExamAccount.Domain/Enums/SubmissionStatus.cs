namespace PRN232.ExamAccount.Domain.Enums;

public enum SubmissionStatus
{
    Submitted = 0,
    QueuedForGrading = 1,
    RegradingRequested = 2,
    GradingInProgress = 3,
    GradedPendingPublication = 4,
    Published = 5,
    Failed = 6
}
