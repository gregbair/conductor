using System.Text;
using System.Text.Json;
using Fulcrum.Conductor.Core.Modules;

namespace Fulcrum.Conductor.Modules.Debug.Tests;

/// <summary>
/// Tests for the Debug module using Console I/O redirection.
/// </summary>
public class DebugModuleTests
{
    private async Task<ModuleResult> ExecuteModuleAsync(Dictionary<string, object?> vars)
    {
        var module = new DebugModule();
        var inputJson = JsonSerializer.Serialize(vars);

        var originalInput = Console.In;
        var originalOutput = Console.Out;

        try
        {
            Console.SetIn(new StringReader(inputJson));
            var outputWriter = new StringWriter();
            Console.SetOut(outputWriter);

            await module.RunAsync();

            var output = outputWriter.ToString();
            var result = JsonSerializer.Deserialize<ModuleResult>(output);
            return result ?? throw new Exception($"Failed to deserialize module result. Output: {output}");
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public async Task DebugModule_WithMsg_ReturnsSuccessWithMessage()
    {
        var vars = new Dictionary<string, object?>
        {
            ["msg"] = "Hello debug"
        };

        var result = await ExecuteModuleAsync(vars);

        Assert.True(result.Success);
        Assert.False(result.Changed);
        Assert.Equal("Hello debug", result.Message);
    }

    [Fact]
    public async Task DebugModule_WithExistingVar_ReturnsVariableValue()
    {
        var vars = new Dictionary<string, object?>
        {
            ["var"] = "my_var",
            ["my_var"] = "test value"
        };

        var result = await ExecuteModuleAsync(vars);

        Assert.True(result.Success);
        Assert.False(result.Changed);
        Assert.Equal("my_var: test value", result.Message);
    }

    [Fact]
    public async Task DebugModule_WithNonExistentVar_ReturnsNotDefined()
    {
        var vars = new Dictionary<string, object?>
        {
            ["var"] = "missing_var"
        };

        var result = await ExecuteModuleAsync(vars);

        Assert.True(result.Success);
        Assert.False(result.Changed);
        Assert.Equal("missing_var: VARIABLE IS NOT DEFINED!", result.Message);
    }

    [Fact]
    public async Task DebugModule_WithNullVar_ReturnsNull()
    {
        var vars = new Dictionary<string, object?>
        {
            ["var"] = "null_var",
            ["null_var"] = null
        };

        var result = await ExecuteModuleAsync(vars);

        Assert.True(result.Success);
        Assert.False(result.Changed);
        Assert.Equal("null_var: (null)", result.Message);
    }

    [Fact]
    public async Task DebugModule_WithoutMsgOrVar_ReturnsFailure()
    {
        var vars = new Dictionary<string, object?>();

        var result = await ExecuteModuleAsync(vars);

        Assert.False(result.Success);
        Assert.False(result.Changed);
        Assert.Contains("required", result.Message);
    }

    [Fact]
    public async Task DebugModule_WithEmptyMsg_ReturnsFailure()
    {
        var vars = new Dictionary<string, object?>
        {
            ["msg"] = "   "
        };

        var result = await ExecuteModuleAsync(vars);

        Assert.False(result.Success);
        Assert.False(result.Changed);
        Assert.Contains("required", result.Message);
    }

    [Fact]
    public async Task DebugModule_AlwaysReturnsChangedFalse()
    {
        var vars = new Dictionary<string, object?>
        {
            ["msg"] = "test"
        };

        var result = await ExecuteModuleAsync(vars);

        Assert.False(result.Changed);
    }

    [Fact]
    public async Task DebugModule_WithBothMsgAndVar_PrefersMsg()
    {
        var vars = new Dictionary<string, object?>
        {
            ["msg"] = "direct",
            ["var"] = "test_var",
            ["test_var"] = "value"
        };

        var result = await ExecuteModuleAsync(vars);

        Assert.True(result.Success);
        Assert.Equal("direct", result.Message);
    }
}
