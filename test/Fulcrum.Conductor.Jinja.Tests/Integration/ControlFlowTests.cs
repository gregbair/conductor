using Fulcrum.Conductor.Jinja.Rendering;

namespace Fulcrum.Conductor.Jinja.Tests.Integration;

public sealed class ControlFlowTests
{
    [Fact]
    public void Render_ForLoop_IteratesOverList()
    {
        string template = "{% for item in items %}{{ item }} {% endfor %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { "a", "b", "c" } });

        Assert.Equal("a b c ", result);
    }

    [Fact]
    public void Render_ForLoopWithNumbers_IteratesCorrectly()
    {
        string template = "{% for num in numbers %}{{ num }},{% endfor %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { numbers = new[] { 1, 2, 3, 4, 5 } });

        Assert.Equal("1,2,3,4,5,", result);
    }

    [Fact]
    public void Render_ForLoopWithText_CombinesTextAndVariables()
    {
        string template = "Names: {% for name in names %}{{ name }}, {% endfor %}done";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { names = new[] { "Alice", "Bob", "Charlie" } });

        Assert.Equal("Names: Alice, Bob, Charlie, done", result);
    }

    [Fact]
    public void Render_IfTrue_RendersBody()
    {
        string template = "{% if show %}visible{% endif %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { show = true });

        Assert.Equal("visible", result);
    }

    [Fact]
    public void Render_IfFalse_RendersNothing()
    {
        string template = "{% if show %}visible{% endif %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { show = false });

        Assert.Equal("", result);
    }

    [Fact]
    public void Render_IfWithComparison_EvaluatesCorrectly()
    {
        string template = "{% if age > 18 %}adult{% endif %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { age = 25 });

        Assert.Equal("adult", result);
    }

    [Fact]
    public void Render_IfElse_RendersElseWhenFalse()
    {
        string template = "{% if show %}yes{% else %}no{% endif %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { show = false });

        Assert.Equal("no", result);
    }

    [Fact]
    public void Render_IfElifElse_RendersCorrectBranch()
    {
        string template = "{% if score >= 90 %}A{% elif score >= 80 %}B{% elif score >= 70 %}C{% else %}F{% endif %}";

        Template parsed = Template.Parse(template);
        string result1 = parsed.Render(new { score = 95 });
        string result2 = parsed.Render(new { score = 85 });
        string result3 = parsed.Render(new { score = 75 });
        string result4 = parsed.Render(new { score = 65 });

        Assert.Equal("A", result1);
        Assert.Equal("B", result2);
        Assert.Equal("C", result3);
        Assert.Equal("F", result4);
    }

    [Fact]
    public void Render_ComparisonOperators_WorkCorrectly()
    {
        string template = @"
{% if 5 == 5 %}eq{% endif %}
{% if 5 != 3 %}neq{% endif %}
{% if 3 < 5 %}lt{% endif %}
{% if 5 <= 5 %}lte{% endif %}
{% if 7 > 5 %}gt{% endif %}
{% if 5 >= 5 %}gte{% endif %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        Assert.Contains("eq", result);
        Assert.Contains("neq", result);
        Assert.Contains("lt", result);
        Assert.Contains("lte", result);
        Assert.Contains("gt", result);
        Assert.Contains("gte", result);
    }

    [Fact]
    public void Render_LogicalAnd_WorksCorrectly()
    {
        string template = "{% if a and b %}both{% endif %}";

        Template parsed = Template.Parse(template);
        string result1 = parsed.Render(new { a = true, b = true });
        string result2 = parsed.Render(new { a = true, b = false });

        Assert.Equal("both", result1);
        Assert.Equal("", result2);
    }

    [Fact]
    public void Render_LogicalOr_WorksCorrectly()
    {
        string template = "{% if a or b %}either{% endif %}";

        Template parsed = Template.Parse(template);
        string result1 = parsed.Render(new { a = false, b = true });
        string result2 = parsed.Render(new { a = false, b = false });

        Assert.Equal("either", result1);
        Assert.Equal("", result2);
    }

    [Fact]
    public void Render_ArithmeticOperators_WorkCorrectly()
    {
        string template = "{{ 5 + 3 }} {{ 10 - 4 }} {{ 3 * 4 }} {{ 15 / 3 }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        Assert.Equal("8 6 12 5", result);
    }

    [Fact]
    public void Render_NestedForLoops_WorkCorrectly()
    {
        string template = "{% for i in outer %}{% for j in inner %}{{ i }}{{ j }} {% endfor %}{% endfor %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { outer = new[] { "A", "B" }, inner = new[] { 1, 2 } });

        Assert.Equal("A1 A2 B1 B2 ", result);
    }

    [Fact]
    public void Render_ForLoopWithIfInside_WorksCorrectly()
    {
        string template = "{% for num in numbers %}{% if num > 2 %}{{ num }} {% endif %}{% endfor %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { numbers = new[] { 1, 2, 3, 4, 5 } });

        Assert.Equal("3 4 5 ", result);
    }
}