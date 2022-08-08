using Data.Models;
using Microsoft.Extensions.Configuration;

namespace Data.FileSystem;

public class PlayerCharacterRepo : FileSystemRepo<PlayerCharacter, string>, IPlayerCharacterRepo 
{
    public PlayerCharacterRepo(IConfiguration config) : base(config) { }

    public Task<PlayerCharacter> GetPlayer(string nickname) => Get(p => p.Nickname.ToLower() == nickname.ToLower());
    public Task<PlayerCharacter> SavePlayer(PlayerCharacter player) => Save(player);

    protected override string GetNextKey() => Guid.NewGuid().ToString();
}
