using Microsoft.Extensions.Configuration;

namespace Data.FileSystem;

public class HelpFilesRepo : FileSystemRepo<string, string>, IHelpFilesRepo
{
    public HelpFilesRepo(IConfiguration config) : base(config) { }
    public override string ModelStoragePath => Path.Combine(RootStoragePath, "HelpFiles");
    protected override string Deserialize(string raw) => raw;
    protected override string Serialize(string model) => model;
    protected override string GetFileExtension() => "txt";
    protected override string GetFileName(string path)
        => $"{path.Replace(" ","/")}.{GetFileExtension()}";

    public async Task<string> GetHelpPage(string path)
        => await Get(path.Replace(" ", "/"));
}
