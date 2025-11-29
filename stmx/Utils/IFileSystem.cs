namespace stmx.Utils;

public interface IFileSystem
{
    bool DirectoryExists(string path);
    string[] GetDirectories(string path);
}

