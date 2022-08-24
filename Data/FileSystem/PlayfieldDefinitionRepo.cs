using Microsoft.Extensions.Configuration;

namespace Data.FileSystem;

public class PlayfieldDefinitionRepo : FileSystemRepo<PlayfieldDefinition, string>, IPlayfieldDefinitionRepo
{
    public PlayfieldDefinitionRepo(IConfiguration config) : base(config) { }
    protected override string GetNextKey() => Guid.NewGuid().ToString();
    protected override string GetKey(PlayfieldDefinition data) => data.UniqueId;
    protected override void SetKey(PlayfieldDefinition pfDef, string key) { pfDef.UniqueId = key; }
    protected override string GetFileName(string path)
        => $"{path.Replace(" ", "/")}.{GetFileExtension()}";

    public async Task<PlayfieldDefinition> GetPlayfieldDefinition(string path)
        => await Get(path);
    public async Task<PlayfieldDefinition> SavePlayfieldDefinition(PlayfieldDefinition def)
        => await Save(def);
    public async Task<IEnumerable<PlayfieldDefinition>> GetPlayfieldDefinitions()
        => await GetAll();
}
