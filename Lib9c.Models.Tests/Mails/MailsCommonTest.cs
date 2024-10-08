using Lib9c.Models.Mails;

namespace Lib9c.Models.Tests.Mails;

public class MailsCommonTest
{
    [Fact]
    public void Handle_All_Relates_Types_Of_Mail()
    {
        var targetType = typeof(Nekoyume.Model.Mail.Mail);
        var targetNameTuples = targetType.Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(targetType))
            .Select(t => (t.Name, t.FullName))
            .ToArray();

        // Prevent unexpected namespace. "Nekoyume.Model.Mail.{Mail}"
        foreach (var (_, targetFullName) in targetNameTuples)
        {
            Assert.NotNull(targetFullName);
            Assert.StartsWith("Nekoyume.Model.Mail.", targetFullName);
        }

        var mailType = typeof(Mail);
        var mailNameTuples = mailType.Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(mailType))
            .Select(t => (t.Name, t.FullName))
            .ToArray();

        // Prevent unexpected namespace. "Lib9c.Models.Mails.{Mail}"
        foreach (var (_, mailFullName) in mailNameTuples)
        {
            Assert.NotNull(mailFullName);
            Assert.StartsWith("Lib9c.Models.Mails.", mailFullName);
        }

        var mailNames = mailNameTuples.Select(e => e.Name).ToArray();
        foreach (var (targetName, _) in targetNameTuples)
        {
            Assert.Contains(targetName, mailNames);
        }
    }
}
