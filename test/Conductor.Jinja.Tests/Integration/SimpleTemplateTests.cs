using Conductor.Jinja.Rendering;

namespace Conductor.Jinja.Tests.Integration;

public sealed class SimpleTemplateTests
{
    [Fact]
    public void Render_SimpleVariable_ReturnsValue()
    {
        // Arrange
        string template = "Hello {{ name }}!";

        // Act
        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { name = "World" });

        // Assert
        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public void Render_PlainText_ReturnsText()
    {
        // Arrange
        string template = "Hello, this is plain text!";

        // Act
        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        // Assert
        Assert.Equal("Hello, this is plain text!", result);
    }

    [Fact]
    public void Render_MultipleVariables_ReturnsAllValues()
    {
        // Arrange
        string template = "{{ greeting }}, {{ name }}!";

        // Act
        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { greeting = "Hello", name = "World" });

        // Assert
        Assert.Equal("Hello, World!", result);
    }

    [Fact]
    public void Render_NumberLiteral_ReturnsNumber()
    {
        // Arrange
        string template = "The answer is {{ 42 }}";

        // Act
        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        // Assert
        Assert.Equal("The answer is 42", result);
    }

    [Fact]
    public void Render_StringLiteral_ReturnsString()
    {
        // Arrange
        string template = "{{ 'Hello' }}";

        // Act
        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        // Assert
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void Render_BooleanTrue_ReturnsTrue()
    {
        // Arrange
        string template = "{{ true }}";

        // Act
        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        // Assert
        Assert.Equal("True", result);
    }

    [Fact]
    public void Render_BooleanFalse_ReturnsFalse()
    {
        // Arrange
        string template = "{{ false }}";

        // Act
        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        // Assert
        Assert.Equal("False", result);
    }

    [Fact]
    public void Render_None_ReturnsEmptyString()
    {
        // Arrange
        string template = "{{ none }}";

        // Act
        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void Render_UndefinedVariable_ReturnsEmptyString()
    {
        // Arrange
        string template = "{{ missing }}";

        // Act
        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { defined = "value" });

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void Render_MixedTextAndVariables_ReturnsCorrectOutput()
    {
        // Arrange
        string template = "Name: {{ name }}, Age: {{ age }}, City: {{ city }}";

        // Act
        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { name = "Alice", age = 30, city = "Seattle" });

        // Assert
        Assert.Equal("Name: Alice, Age: 30, City: Seattle", result);
    }
}