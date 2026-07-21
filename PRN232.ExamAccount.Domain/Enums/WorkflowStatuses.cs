namespace PRN232.ExamAccount.Domain.Enums;

public enum ExamSessionStatus { Draft, Ready, Grading, UnderReview, Completed }
public enum GradingBatchStatus { Assigned, InProgress, SubmittedForReview, NeedsCorrection, Resubmitted, Accepted }
public enum GradingItemStatus { Assigned, LocalMatched, Grading, Graded, TechnicalError, MissingSubmission, Submitted, ReturnedForCorrection, Accepted }
