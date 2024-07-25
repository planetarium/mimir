using Bencodex.Types;
using Lib9c.Models.Skills;

namespace Lib9c.Models.Factories;

public static class SkillFactory
{
    public static Skill Create(IValue bencoded)
    {
        return new Skill(bencoded);
    }
}
