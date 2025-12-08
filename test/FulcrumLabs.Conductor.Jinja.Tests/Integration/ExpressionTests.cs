using FulcrumLabs.Conductor.Jinja.Rendering;

namespace FulcrumLabs.Conductor.Jinja.Tests.Integration;

public sealed class ExpressionTests
{
    [Fact]
    public void Render_MemberAccess_ReturnsPropertyValue()
    {
        string template = "{{ user.name }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { user = new { name = "Alice" } });

        Assert.Equal("Alice", result);
    }

    [Fact]
    public void Render_NestedMemberAccess_ReturnsNestedValue()
    {
        string template = "{{ user.profile.email }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { user = new { profile = new { email = "alice@example.com" } } });

        Assert.Equal("alice@example.com", result);
    }

    [Fact]
    public void Render_MemberAccessOnNull_ReturnsEmptyString()
    {
        string template = "{{ user.name }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { user = (object?)null });

        Assert.Equal("", result);
    }

    [Fact]
    public void Render_ArrayIndexAccess_ReturnsElement()
    {
        string template = "{{ items[0] }} {{ items[2] }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { "a", "b", "c" } });

        Assert.Equal("a c", result);
    }

    [Fact]
    public void Render_DictionaryIndexAccess_ReturnsValue()
    {
        string template = "{{ data['name'] }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { data = new Dictionary<string, object> { { "name", "Bob" } } });

        Assert.Equal("Bob", result);
    }

    [Fact]
    public void Render_StringIndexAccess_ReturnsCharacter()
    {
        string template = "{{ text[0] }}{{ text[4] }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { text = "Hello" });

        Assert.Equal("Ho", result);
    }

    [Fact]
    public void Render_IndexOutOfBounds_ReturnsEmptyString()
    {
        string template = "{{ items[10] }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { "a", "b" } });

        Assert.Equal("", result);
    }

    [Fact]
    public void Render_NotOperator_NegatesTruthiness()
    {
        string template = "{% if not false %}yes{% endif %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render();

        Assert.Equal("yes", result);
    }

    [Fact]
    public void Render_NotOperatorOnVariable_Works()
    {
        string template = "{% if not flag %}off{% endif %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { flag = false });

        Assert.Equal("off", result);
    }

    [Fact]
    public void Render_NegateOperator_NegatesNumber()
    {
        string template = "{{ -5 }} {{ -x }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { x = 10 });

        Assert.Equal("-5 -10", result);
    }

    [Fact]
    public void Render_ChainedMemberAndIndex_WorksCorrectly()
    {
        string template = "{{ user.roles[0] }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { user = new { roles = new[] { "admin", "user" } } });

        Assert.Equal("admin", result);
    }

    [Fact]
    public void Render_ChainedIndexAndMember_WorksCorrectly()
    {
        string template = "{{ users[0].name }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { users = new[] { new { name = "Alice" }, new { name = "Bob" } } });

        Assert.Equal("Alice", result);
    }

    [Fact]
    public void Render_ComplexChaining_WorksCorrectly()
    {
        string template = "{{ data.users[1].profile.settings['theme'] }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new
        {
            data = new
            {
                users = new[]
                {
                    new
                    {
                        profile = new
                        {
                            settings = new Dictionary<string, object> { { "theme", "light" } }
                        }
                    },
                    new
                    {
                        profile = new
                        {
                            settings = new Dictionary<string, object> { { "theme", "dark" } }
                        }
                    }
                }
            }
        });

        Assert.Equal("dark", result);
    }

    [Fact]
    public void Render_MemberAccessInForLoop_WorksCorrectly()
    {
        string template = "{% for user in users %}{{ user.name }} {% endfor %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new
        {
            users = new[] { new { name = "Alice" }, new { name = "Bob" }, new { name = "Charlie" } }
        });

        Assert.Equal("Alice Bob Charlie ", result);
    }

    [Fact]
    public void Render_IndexAccessInForLoop_WorksCorrectly()
    {
        string template = "{% for item in items %}{{ item[0] }}{% endfor %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { "apple", "banana", "cherry" } });

        Assert.Equal("abc", result);
    }

    [Fact]
    public void Render_NotInIfCondition_WorksCorrectly()
    {
        string template = "{% if not user.active %}inactive{% endif %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { user = new { active = false } });

        Assert.Equal("inactive", result);
    }

    [Fact]
    public void Render_MemberAccessWithComparison_WorksCorrectly()
    {
        string template = "{% if user.age > 18 %}adult{% endif %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { user = new { age = 25 } });

        Assert.Equal("adult", result);
    }

    [Fact]
    public void Render_DynamicIndexWithVariable_WorksCorrectly()
    {
        string template = "{{ items[index] }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { "a", "b", "c" }, index = 1 });

        Assert.Equal("b", result);
    }
}