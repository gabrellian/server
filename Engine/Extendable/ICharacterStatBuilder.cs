using Data.Models;

namespace Engine.Extendable;

public interface ICharacterStatBuilder
{
    public Task Initialize(PlayerCharacter player);
}