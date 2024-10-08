using Lib9c.Models.Mails;

namespace Lib9c.Models.Tests.Mails;

public class MailsCommonTest
{
    [Fact]
    public void Handle_All_Mail_Types_Of_Lib9c()
    {
        var targetMailType = typeof(Nekoyume.Model.Mail.Mail);
        var targetMailNameTuples = targetMailType.Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(targetMailType))
            .Select(t => (t.Name, t.FullName))
            .ToArray();

        // Prevent unexpected namespace. "Nekoyume.Model.Mail.{Mail}"
        foreach (var (_, targetMailFullName) in targetMailNameTuples)
        {
            Assert.NotNull(targetMailFullName);
            Assert.StartsWith("Nekoyume.Model.Mail.", targetMailFullName);
        }

        var mailType = typeof(Mail);
        var mailFullNameTuples = mailType.Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(mailType))
            .Select(t => (t.Name, t.FullName))
            .ToArray();

        // Prevent unexpected namespace. "Lib9c.Models.Mails.{Mail}"
        foreach (var (_, mailFullName) in mailFullNameTuples)
        {
            Assert.NotNull(mailFullName);
            Assert.StartsWith("Lib9c.Models.Mails.", mailFullName);
        }

        var mailNames = mailFullNameTuples.Select(e => e.Name).ToArray();
        foreach (var (targetMailName, _) in targetMailNameTuples)
        {
            Assert.Contains(targetMailName, mailNames);
        }
    }
}
