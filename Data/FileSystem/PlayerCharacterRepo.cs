using System.Text.Json;
using System.Text.RegularExpressions;
using Data.Models;
using Microsoft.Extensions.Configuration;

namespace Data.FileSystem;

public class PlayerCharacterRepo : FileSystemRepo<PlayerCharacter, string>, IPlayerCharacterRepo
{
    private Regex _badCharactersMatch = new Regex("[^A-Za-z0-9\x20']");
    public PlayerCharacterRepo(IConfiguration config, JsonSerializerOptions jsonOptions) : base(config, jsonOptions)
    {
        if (jsonOptions == null)
            throw new ArgumentNullException($"Player character repository requires a pre-configured set of JsonSerializerOptions!");
    }

    public Task<PlayerCharacter> GetPlayer(string nickname) => Get(p => p.Nickname.ToLower() == nickname.ToLower());

    public async Task<bool> IsValidPlayerName(string rawCommand)
    {
        if (rawCommand.Length <= 2 || 
            rawCommand.Length > 20 || 
            _badCharactersMatch.IsMatch(rawCommand) || 
            await Get(p => p.Nickname.Equals(rawCommand, StringComparison.OrdinalIgnoreCase)) != null)
        {
            return false;
        }
        return true;
    }

    public Task<PlayerCharacter> SavePlayer(PlayerCharacter player) => Save(player);

    protected override string GetNextKey() => Guid.NewGuid().ToString();
    protected override string GetKey(PlayerCharacter data) => data.Id;
    protected override void SetKey(PlayerCharacter pc, string key) { pc.Id = key; }
}
