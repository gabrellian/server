using Data.Models;

namespace SampleServer;

public static class StatExtensions
{
    public static SampleStatBlock GetStats(this PlayerCharacter pc) => pc.Stats as SampleStatBlock;
}
public class SampleStatBlock : StatBlock
{

    [StatDescription("Maximum health points before passing out")]
    public int MaxHealth
    {
        get => HitDiceRolls.Sum() + ((Constitution - 10) * HitDiceRolls.Count());
    }
    [StatDescription(IsHidden = true)]
    public List<int> HitDiceRolls
    {
        get => Get<List<int>>(nameof(HitDiceRolls), new List<int>());
        set => Set<List<int>>(nameof(HitDiceRolls), value);
    }

    [StatDescription("Number of health points per hit die")]
    public int HitDie
    {
        get => Get<int>(nameof(HitDie));
        set => Set<int>(nameof(HitDie), value);
    }
    [StatDescription("Your ability withstand damage")]
    public int Constitution
    {
        get => Get<int>(nameof(Constitution));
        set => Set<int>(nameof(Constitution), value);
    }
    [StatDescription("Your ability to lift, push and pull along with your ability to swing weapons")]
    public int Strength
    {
        get => Get<int>(nameof(Strength));
        set => Set<int>(nameof(Strength), value);
    }
    [StatDescription("Your quickness of wit and depth of knowledge")]
    public int Intelligence
    {
        get => Get<int>(nameof(Intelligence));
        set => Set<int>(nameof(Intelligence), value);
    }
    [StatDescription("Your finess between body and eye")]
    public int Dexterity
    {
        get => Get<int>(nameof(Dexterity));
        set => Set<int>(nameof(Dexterity), value);
    }
    [StatDescription("Your power of social prowess")]
    public int Charisma
    {
        get => Get<int>(nameof(Charisma));
        set => Set<int>(nameof(Charisma), value);
    }
    [StatDescription("Your ability to for solid judgement and affinity for the divine")]
    public int Wisdom
    {
        get => Get<int>(nameof(Wisdom));
        set => Set<int>(nameof(Wisdom), value);
    }
}
