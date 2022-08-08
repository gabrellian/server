using Data.Models;

namespace Data;

public interface IPlayerCharacterRepo 
{
    Task<PlayerCharacter> GetPlayer(string nickname);
    Task<PlayerCharacter> SavePlayer(PlayerCharacter player);
}
