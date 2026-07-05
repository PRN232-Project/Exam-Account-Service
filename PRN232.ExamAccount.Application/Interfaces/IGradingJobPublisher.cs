namespace PRN232.ExamAccount.Application.Interfaces;

public interface IGradingJobPublisher
{
    Task PublishSubmissionAsync(Guid submissionId, bool isRegrade, CancellationToken cancellationToken = default);
}
