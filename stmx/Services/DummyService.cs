namespace stmx.Services;

class DummyService()
{
    public DummyServiceOptions Options { get; } = new DummyServiceOptions();

    public Task<string> GetDummyData(string? dataInput = null)
    {
        if (dataInput == null) dataInput = Options.DefaultDataInput;
        var retValue = $"dummy data return: {dataInput}";

        return Task.FromResult(retValue);
    }
}
