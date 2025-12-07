using Conductor.Jinja.Rendering;

namespace Conductor.Jinja.Tests.Integration;

public sealed class SimpleTemplateTests
{
    [Fact]
    public void Render_SimpleVariable_ReturnsValue()
    {
        string template = "Hello {{ name }}!";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { name = "World" });

        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public void Render_PlainText_ReturnsText()
    {
        string template = "Hello, this is plain text!";

        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        Assert.Equal("Hello, this is plain text!", result);
    }

    [Fact]
    public void Render_MultipleVariables_ReturnsAllValues()
    {
        string template = "{{ greeting }}, {{ name }}!";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { greeting = "Hello", name = "World" });

        Assert.Equal("Hello, World!", result);
    }

    [Fact]
    public void Render_NumberLiteral_ReturnsNumber()
    {
        string template = "The answer is {{ 42 }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        Assert.Equal("The answer is 42", result);
    }

    [Fact]
    public void Render_StringLiteral_ReturnsString()
    {
        string template = "{{ 'Hello' }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        Assert.Equal("Hello", result);
    }

    [Fact]
    public void Render_BooleanTrue_ReturnsTrue()
    {
        string template = "{{ true }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        Assert.Equal("True", result);
    }

    [Fact]
    public void Render_BooleanFalse_ReturnsFalse()
    {
        string template = "{{ false }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        Assert.Equal("False", result);
    }

    [Fact]
    public void Render_None_ReturnsEmptyString()
    {
        string template = "{{ none }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        Assert.Equal("", result);
    }

    [Fact]
    public void Render_UndefinedVariable_ReturnsEmptyString()
    {
        string template = "{{ missing }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { defined = "value" });

        Assert.Equal("", result);
    }

    [Fact]
    public void Render_MixedTextAndVariables_ReturnsCorrectOutput()
    {
        string template = "Name: {{ name }}, Age: {{ age }}, City: {{ city }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { name = "Alice", age = 30, city = "Seattle" });

        Assert.Equal("Name: Alice, Age: 30, City: Seattle", result);
    }
}