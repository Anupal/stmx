using System.CommandLine;
using System.CommandLine.Invocation;

using stmx.Services;

namespace stmx.Commands;

class DummyCommand : Command
{
    private readonly DummyService _dummy;

    public DummyCommand(DummyService dummy) : base("dummy", "echoes back value")
    {
        _dummy = dummy ?? throw new ArgumentNullException(nameof(dummy));

        var dataInputOption = new Option<string>("--input", ["-i"]);
        dataInputOption.Description = "value to echo back";
        dataInputOption.DefaultValueFactory = _ => _dummy.Options.DefaultDataInput;

        Add(dataInputOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var dataInputOptionValue = parseResult.GetValue(dataInputOption);
            await Execute(dataInputOptionValue!);
        });
    }

    private async Task Execute(string dataInput)
    {
        var retValue = await _dummy.GetDummyData(dataInput);
        Console.WriteLine(retValue);
    }
}
