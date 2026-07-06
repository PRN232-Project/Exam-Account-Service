using PRN232.ExamAccount.Application.Messaging;

namespace PRN232.ExamAccount.Tests;

public class MessagingContractTests
{
    private const string ForbiddenKeywordOne = "SELECT *";
    private const string ForbiddenKeywordTwo = "DROP TABLE";
    private const string SectionName = "CRUD";
    private const decimal SectionWeight = 30m;
    private const string SectionFilter = "Category=CRUD";

    [Fact]
    public void GradingJobMessage_CanCarryPlagiarismConfigAndSections()
    {
        var message = new GradingJobMessage
        {
            SubmissionId = Guid.NewGuid(),
            ExamConfig = new ExamConfigurationMessage
            {
                PlagiarismConfig = [ForbiddenKeywordOne, ForbiddenKeywordTwo],
                Sections =
                [
                    new SectionConfigurationMessage
                    {
                        Name = SectionName,
                        Weight = SectionWeight,
                        TestFilter = SectionFilter
                    }
                ]
            }
        };

        Assert.Equal(2, message.ExamConfig.PlagiarismConfig.Length);
        Assert.Single(message.ExamConfig.Sections);
        Assert.Equal(SectionName, message.ExamConfig.Sections[0].Name);
    }
}
