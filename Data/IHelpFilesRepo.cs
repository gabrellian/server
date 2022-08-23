public interface IHelpFilesRepo
{
    Task<string> GetHelpPage(string path);
}
