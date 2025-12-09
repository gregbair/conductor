using FulcrumLabs.Conductor.Core.Conditionals;
using FulcrumLabs.Conductor.Core.Templating;
using FulcrumLabs.Conductor.Jinja.Rendering;

namespace FulcrumLabs.Conductor.Core.Tests.Conditionals;

public class ConditionalEvaluatorTests
{
    private readonly ConditionalEvaluator _evaluator;

    public ConditionalEvaluatorTests()
    {
        TemplateExpander expander = new();
        _evaluator = new ConditionalEvaluator(expander);
    }

    [Fact]
    public void Evaluate_WithNullCondition_ReturnsTrue()
    {
        TemplateContext context = TemplateContext.Create();

        bool result = _evaluator.Evaluate(null, context);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_WithEmptyCondition_ReturnsTrue()
    {
        TemplateContext context = TemplateContext.Create();

        bool result = _evaluator.Evaluate("", context);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_WithTrueVariable_ReturnsTrue()
    {
        TemplateContext context = TemplateContext.Create();
        context.SetVariable("should_run", true);

        bool result = _evaluator.Evaluate("should_run", context);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_WithFalseVariable_ReturnsFalse()
    {
        TemplateContext context = TemplateContext.Create();
        context.SetVariable("should_run", false);

        bool result = _evaluator.Evaluate("should_run", context);

        Assert.False(result);
    }

    [Fact]
    public void Evaluate_WithComparison_EvaluatesCorrectly()
    {
        TemplateContext context = TemplateContext.Create();
        context.SetVariable("count", 5);

        bool result = _evaluator.Evaluate("count > 3", context);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_WithStringEquality_EvaluatesCorrectly()
    {
        TemplateContext context = TemplateContext.Create();
        context.SetVariable("os", "linux");

        bool result = _evaluator.Evaluate("os == 'linux'", context);

        Assert.True(result);
    }
}
