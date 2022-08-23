using Microsoft.Extensions.Configuration;

namespace Data.FileSystem;

public class HelpFileRepo : FileSystemRepo<string, string>, IHelpFilesRepo
{
    public HelpFileRepo(IConfiguration config) : base(config) { }
    public override string ModelStoragePath => Path.Combine(RootStoragePath, "HelpFiles");
    protected override string Deserialize(string raw) => raw;
    protected override string Serialize(string model) => model;
    protected override string GetFileExtension() => "txt";
    protected override string GetFileName(string path)
        => $"{path.Replace(" ", "/")}.{GetFileExtension()}";

    public async Task<string> GetHelpPage(string path)
        => string.Join(
            Environment.NewLine,
            (await Get(path.Replace(" ", "/")))
                .Split(new char[] { '\r', '\n' }).Skip(1));
    protected override void SetKey(string data, string key)
    {
        data = $"@page {key}{Environment.NewLine}{data}"; 
    }
    protected override string GetKey(string data)
        => data
            .Split(new char[] { '\r', '\n' })
            .First()
            .Split(" ")
            .Skip(1)
            .FirstOrDefault();
    
}
