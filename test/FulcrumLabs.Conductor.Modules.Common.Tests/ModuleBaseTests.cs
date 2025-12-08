using FulcrumLabs.Conductor.Core.Modules;

namespace FulcrumLabs.Conductor.Modules.Common.Tests;

/// <summary>
///     Tests for the ModuleBase class that all modules inherit from.
/// </summary>
public class ModuleBaseTests
{
    [Fact]
    public async Task RunAsync_WithValidJsonInput_ReturnsSuccessExitCode()
    {
        TestModule module = new() { ExecuteFunc = _ => Task.FromResult(TestModule.TestSuccess("Success")) };

        string input = """{"test": "value"}""";
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;

        try
        {
            Console.SetIn(new StringReader(input));
            StringWriter outputWriter = new();
            Console.SetOut(outputWriter);

            int exitCode = await module.RunAsync();

            Assert.Equal(0, exitCode);

            string output = outputWriter.ToString();
            Assert.Contains("\"success\":true", output);
            Assert.Contains("\"message\":\"Success\"", output);
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public async Task RunAsync_WithFailureResult_ReturnsFailureExitCode()
    {
        TestModule module = new() { ExecuteFunc = _ => Task.FromResult(TestModule.TestFailure("Something failed")) };

        string input = """{"test": "value"}""";
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;

        try
        {
            Console.SetIn(new StringReader(input));
            StringWriter outputWriter = new();
            Console.SetOut(outputWriter);

            int exitCode = await module.RunAsync();

            Assert.Equal(1, exitCode);

            string output = outputWriter.ToString();
            Assert.Contains("\"success\":false", output);
            Assert.Contains("\"message\":\"Something failed\"", output);
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public async Task RunAsync_WithInvalidJson_ReturnsErrorExitCode()
    {
        TestModule module = new();
        string input = "{invalid json";
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;
        TextWriter originalError = Console.Error;

        try
        {
            Console.SetIn(new StringReader(input));
            StringWriter outputWriter = new();
            StringWriter errorWriter = new();
            Console.SetOut(outputWriter);
            Console.SetError(errorWriter);

            int exitCode = await module.RunAsync();

            Assert.Equal(1, exitCode);

            string output = outputWriter.ToString();
            // Module should output error as JSON
            if (!string.IsNullOrEmpty(output))
            {
                Assert.Contains("\"success\":false", output);
                Assert.Contains("Invalid JSON input", output);
            }
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
            Console.SetError(originalError);
        }
    }

    [Fact]
    public async Task RunAsync_WithEmptyInput_ReturnsErrorExitCode()
    {
        TestModule module = new();
        string input = "";
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;

        try
        {
            Console.SetIn(new StringReader(input));
            StringWriter outputWriter = new();
            Console.SetOut(outputWriter);

            int exitCode = await module.RunAsync();

            Assert.Equal(1, exitCode);

            string output = outputWriter.ToString();
            Assert.Contains("\"success\":false", output);
            Assert.Contains("No input received", output);
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public async Task RunAsync_WhenExecuteThrows_ReturnsErrorExitCode()
    {
        TestModule module = new() { ExecuteFunc = _ => throw new InvalidOperationException("Test exception") };

        string input = """{"test": "value"}""";
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;

        try
        {
            Console.SetIn(new StringReader(input));
            StringWriter outputWriter = new();
            Console.SetOut(outputWriter);

            int exitCode = await module.RunAsync();

            Assert.Equal(1, exitCode);

            string output = outputWriter.ToString();
            Assert.Contains("\"success\":false", output);
            Assert.Contains("Unhandled exception", output);
            Assert.Contains("Test exception", output);
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void TryGetRequiredParameter_WithExistingParameter_ReturnsTrue()
    {
        Dictionary<string, object?> vars = new() { ["name"] = "test_value" };

        bool result = TestModule.TestTryGetRequiredParameter(vars, "name", out string value);

        Assert.True(result);
        Assert.Equal("test_value", value);
    }

    [Fact]
    public void TryGetRequiredParameter_WithMissingParameter_ReturnsFalse()
    {
        Dictionary<string, object?> vars = new();

        bool result = TestModule.TestTryGetRequiredParameter(vars, "name", out string value);

        Assert.False(result);
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void TryGetRequiredParameter_WithNullValue_ReturnsFalse()
    {
        Dictionary<string, object?> vars = new() { ["name"] = null };

        bool result = TestModule.TestTryGetRequiredParameter(vars, "name", out string value);

        Assert.False(result);
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void TryGetRequiredParameter_WithEmptyString_ReturnsFalse()
    {
        Dictionary<string, object?> vars = new() { ["name"] = "" };

        bool result = TestModule.TestTryGetRequiredParameter(vars, "name", out string value);

        Assert.False(result);
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void GetOptionalParameter_WithExistingParameter_ReturnsValue()
    {
        Dictionary<string, object?> vars = new() { ["greeting"] = "Hello" };

        string result = TestModule.TestGetOptionalParameter(vars, "greeting", "Hi");

        Assert.Equal("Hello", result);
    }

    [Fact]
    public void GetOptionalParameter_WithMissingParameter_ReturnsDefault()
    {
        Dictionary<string, object?> vars = new();

        string result = TestModule.TestGetOptionalParameter(vars, "greeting", "Hi");

        Assert.Equal("Hi", result);
    }

    [Fact]
    public void GetOptionalParameter_WithNullValue_ReturnsDefault()
    {
        Dictionary<string, object?> vars = new() { ["greeting"] = null };

        string result = TestModule.TestGetOptionalParameter(vars, "greeting", "Hi");

        Assert.Equal("Hi", result);
    }

    [Fact]
    public void Success_CreatesSuccessResult()
    {
        ModuleResult result = TestModule.TestSuccess("Operation completed", true);

        Assert.True(result.Success);
        Assert.True(result.Changed);
        Assert.Equal("Operation completed", result.Message);
        Assert.Empty(result.Facts);
    }

    [Fact]
    public void Success_WithFacts_IncludesFacts()
    {
        Dictionary<string, object?> facts = new() { ["key1"] = "value1", ["key2"] = 42 };

        ModuleResult result = TestModule.TestSuccess("Done", false, facts);

        Assert.True(result.Success);
        Assert.False(result.Changed);
        Assert.Equal("Done", result.Message);
        Assert.Equal(2, result.Facts.Count);
        Assert.Equal("value1", result.Facts["key1"]);
        Assert.Equal(42, result.Facts["key2"]);
    }

    [Fact]
    public void Failure_CreatesFailureResult()
    {
        ModuleResult result = TestModule.TestFailure("Operation failed");

        Assert.False(result.Success);
        Assert.False(result.Changed);
        Assert.Equal("Operation failed", result.Message);
        Assert.Empty(result.Facts);
    }

    [Fact]
    public void Failure_WithFacts_IncludesFacts()
    {
        Dictionary<string, object?> facts = new() { ["error_code"] = 404 };

        ModuleResult result = TestModule.TestFailure("Not found", facts);

        Assert.False(result.Success);
        Assert.False(result.Changed);
        Assert.Equal("Not found", result.Message);
        Assert.Single(result.Facts);
        Assert.Equal(404, result.Facts["error_code"]);
    }

    /// <summary>
    ///     Test module implementation for testing ModuleBase behavior.
    /// </summary>
    private class TestModule : ModuleBase
    {
        public Func<Dictionary<string, object?>, Task<ModuleResult>>? ExecuteFunc { get; init; }

        protected override Task<ModuleResult> ExecuteAsync(Dictionary<string, object?> vars)
        {
            if (ExecuteFunc != null)
            {
                return ExecuteFunc(vars);
            }

            return Task.FromResult(Success("Test executed", true));
        }

        // Expose protected methods for testing
        public static bool TestTryGetRequiredParameter(Dictionary<string, object?> vars, string paramName,
            out string value)
        {
            return TryGetRequiredParameter(vars, paramName, out value);
        }

        public static string TestGetOptionalParameter(Dictionary<string, object?> vars, string paramName,
            string defaultValue = "")
        {
            return GetOptionalParameter(vars, paramName, defaultValue);
        }

        public static ModuleResult TestSuccess(string message, bool changed = false,
            Dictionary<string, object?>? facts = null)
        {
            return Success(message, changed, facts);
        }

        public static ModuleResult TestFailure(string message, Dictionary<string, object?>? facts = null)
        {
            return Failure(message, facts);
        }
    }
}