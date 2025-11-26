namespace stmx.Utils;

public class FileReader : IFileReader
{
    public string ReadAllText(string path) => File.ReadAllText(path);
}

