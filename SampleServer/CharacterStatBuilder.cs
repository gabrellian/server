using Data.Models;
using Engine.Extendable;

namespace SampleServer;

public class CharacterStatBuilder : ICharacterStatBuilder
{
    private Random _random = new Random();
    public Task Initialize(PlayerCharacter player)
    {
        player.Stats = new SampleStatBlock();
        player.GetStats().HitDie = 10;
        player.GetStats().HitDiceRolls.Add(player.GetStats().HitDie);
        player.GetStats().Strength = 10;
        player.GetStats().Dexterity = 10;
        player.GetStats().Constitution = 10;
        player.GetStats().Intelligence = 10;
        player.GetStats().Wisdom = 10;
        player.GetStats().Charisma = 10;

        return Task.CompletedTask;
    }
}
