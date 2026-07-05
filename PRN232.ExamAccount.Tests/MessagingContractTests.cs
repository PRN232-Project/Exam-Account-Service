using PRN232.ExamAccount.Application.Messaging;

namespace PRN232.ExamAccount.Tests;

public class MessagingContractTests
{
    [Fact]
    public void GradingJobMessage_CanCarryPlagiarismConfigAndSections()
    {
        var message = new GradingJobMessage
        {
            SubmissionId = Guid.NewGuid(),
            ExamConfig = new ExamConfigurationMessage
            {
                PlagiarismConfig = ["SELECT *", "DROP TABLE"],
                Sections =
                [
                    new SectionConfigurationMessage
                    {
                        Name = "CRUD",
                        Weight = 30,
                        TestFilter = "Category=CRUD"
                    }
                ]
            }
        };

        Assert.Equal(2, message.ExamConfig.PlagiarismConfig.Length);
        Assert.Single(message.ExamConfig.Sections);
        Assert.Equal("CRUD", message.ExamConfig.Sections[0].Name);
    }
}
