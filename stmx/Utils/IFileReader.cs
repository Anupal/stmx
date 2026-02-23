namespace stmx.Utils;

public interface IFileReader
{
    Task<string> ReadAllTextAsync(string path);
}

