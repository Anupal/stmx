namespace stmx.Utils;

public class FileReader : IFileReader
{
    public Task<string> ReadAllTextAsync(string path) => File.ReadAllTextAsync(path);
}

