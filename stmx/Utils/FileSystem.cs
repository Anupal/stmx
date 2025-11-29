namespace stmx.Utils;

public class FileSystem : IFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public string[] GetDirectories(string path) => Directory.GetDirectories(path);
}
