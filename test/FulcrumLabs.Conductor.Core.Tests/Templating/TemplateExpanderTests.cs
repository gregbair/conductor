using FulcrumLabs.Conductor.Core.Templating;
using FulcrumLabs.Conductor.Jinja.Rendering;

namespace FulcrumLabs.Conductor.Core.Tests.Templating;

public class TemplateExpanderTests
{
    private readonly TemplateExpander _expander = new();

    [Fact]
    public void ExpandString_WithNoTemplateMarkers_ReturnsOriginalString()
    {
        TemplateContext context = TemplateContext.Create();
        context.SetVariable("name", "World");

        string result = _expander.ExpandString("Hello", context);

        Assert.Equal("Hello", result);
    }

    [Fact]
    public void ExpandString_WithSimpleVariable_ExpandsVariable()
    {
        TemplateContext context = TemplateContext.Create();
        context.SetVariable("name", "World");

        string result = _expander.ExpandString("Hello {{ name }}", context);

        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void ExpandParameters_ExpandsNestedDictionary()
    {
        TemplateContext context = TemplateContext.Create();
        context.SetVariable("greeting", "Hello");

        Dictionary<string, object?> parameters = new()
        {
            ["message"] = "{{ greeting }} World",
            ["count"] = 42
        };

        Dictionary<string, object?> expanded = _expander.ExpandParameters(parameters, context);

        Assert.Equal("Hello World", expanded["message"]);
        Assert.Equal(42, expanded["count"]);
    }

    [Fact]
    public void EvaluateExpression_WithSimpleVariable_ReturnsValue()
    {
        TemplateContext context = TemplateContext.Create();
        context.SetVariable("items", new List<string> { "a", "b", "c" });

        object? result = _expander.EvaluateExpression("items", context);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IEnumerable<object?>>(result);
    }

    [Fact]
    public void EvaluateExpression_WithComparison_ReturnsBoolean()
    {
        TemplateContext context = TemplateContext.Create();
        context.SetVariable("count", 5);

        object? result = _expander.EvaluateExpression("count > 3", context);

        Assert.NotNull(result);
        // Result might be "True" string or bool depending on renderer output
        Assert.True(result.ToString() == "True" || result is true);
    }
}
