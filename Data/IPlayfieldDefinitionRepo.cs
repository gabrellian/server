public interface IPlayfieldDefinitionRepo
{
    Task<PlayfieldDefinition> GetPlayfieldDefinition(string path);
    Task<PlayfieldDefinition> SavePlayfieldDefinition(PlayfieldDefinition def);
}