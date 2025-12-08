using Fulcrum.Conductor.Jinja.Rendering;

namespace Fulcrum.Conductor.Jinja.Tests.Integration;

public sealed class FilterTests
{
    [Fact]
    public void Render_UpperFilter_ConvertsToUppercase()
    {
        string template = "{{ name | upper }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { name = "hello" });

        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void Render_LowerFilter_ConvertsToLowercase()
    {
        string template = "{{ name | lower }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { name = "WORLD" });

        Assert.Equal("world", result);
    }

    [Fact]
    public void Render_CapitalizeFilter_CapitalizesFirstLetter()
    {
        string template = "{{ name | capitalize }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { name = "hello world" });

        Assert.Equal("Hello world", result);
    }

    [Fact]
    public void Render_LengthFilter_ReturnsStringLength()
    {
        string template = "{{ text | length }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { text = "hello" });

        Assert.Equal("5", result);
    }

    [Fact]
    public void Render_LengthFilter_ReturnsArrayLength()
    {
        string template = "{{ items | length }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { 1, 2, 3 } });

        Assert.Equal("3", result);
    }

    [Fact]
    public void Render_FirstFilter_ReturnsFirstElement()
    {
        string template = "{{ items | first }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { "a", "b", "c" } });

        Assert.Equal("a", result);
    }

    [Fact]
    public void Render_LastFilter_ReturnsLastElement()
    {
        string template = "{{ items | last }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { "a", "b", "c" } });

        Assert.Equal("c", result);
    }

    [Fact]
    public void Render_JoinFilter_JoinsWithSeparator()
    {
        string template = "{{ items | join(', ') }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { "a", "b", "c" } });

        Assert.Equal("a, b, c", result);
    }

    [Fact]
    public void Render_JoinFilterNoArg_JoinsWithoutSeparator()
    {
        string template = "{{ items | join }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { "a", "b", "c" } });

        Assert.Equal("abc", result);
    }

    [Fact]
    public void Render_SplitFilter_SplitsString()
    {
        string template = "{% for item in text | split(',') %}{{ item }}|{% endfor %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { text = "a,b,c" });

        Assert.Equal("a|b|c|", result);
    }

    [Fact]
    public void Render_DefaultFilter_UsesDefault()
    {
        string template = "{{ missing | default('fallback') }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { defined = "value" });

        Assert.Equal("fallback", result);
    }

    [Fact]
    public void Render_DefaultFilter_DoesNotUseDefaultWhenDefined()
    {
        string template = "{{ value | default('fallback') }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { value = "actual" });

        Assert.Equal("actual", result);
    }

    [Fact]
    public void Render_ChainedFilters_AppliesInOrder()
    {
        string template = "{{ name | lower | capitalize }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { name = "HELLO WORLD" });

        Assert.Equal("Hello world", result);
    }

    [Fact]
    public void Render_ChainedFiltersMultiple_AppliesInOrder()
    {
        string template = "{{ items | join(' ') | upper }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { "hello", "world" } });

        Assert.Equal("HELLO WORLD", result);
    }

    [Fact]
    public void Render_FilterInForLoop_AppliesCorrectly()
    {
        string template = "{% for item in items %}{{ item | upper }} {% endfor %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { "a", "b", "c" } });

        Assert.Equal("A B C ", result);
    }

    [Fact]
    public void Render_FilterInIfCondition_WorksCorrectly()
    {
        string template = "{% if text | length > 5 %}long{% else %}short{% endif %}";

        Template parsed = Template.Parse(template);
        string result1 = parsed.Render(new { text = "hello world" });
        string result2 = parsed.Render(new { text = "hi" });

        Assert.Equal("long", result1);
        Assert.Equal("short", result2);
    }

    [Fact]
    public void Render_FilterWithMemberAccess_WorksCorrectly()
    {
        string template = "{{ user.name | upper }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { user = new { name = "alice" } });

        Assert.Equal("ALICE", result);
    }

    [Fact]
    public void Render_FilterWithIndexAccess_WorksCorrectly()
    {
        string template = "{{ items[0] | upper }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { "hello", "world" } });

        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void Render_MemberAccessAfterFilter_WorksCorrectly()
    {
        string template = "{{ items | first | length }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { "hello", "world" } });

        Assert.Equal("5", result);
    }
}