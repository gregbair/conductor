using Fulcrum.Conductor.Jinja.Rendering;

namespace Fulcrum.Conductor.Jinja.Tests.Integration;

public sealed class AnsibleFilterTests
{
    [Fact]
    public void Render_ToJsonFilter_ConvertsToJson()
    {
        string template = "{{ data | to_json }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { data = new { name = "Alice", age = 30 } });

        Assert.Contains("\"name\"", result);
        Assert.Contains("\"Alice\"", result);
        Assert.Contains("\"age\"", result);
        Assert.Contains("30", result);
    }

    [Fact]
    public void Render_ToJsonFilter_WithArray_ConvertsToJson()
    {
        string template = "{{ items | to_json }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { items = new[] { 1, 2, 3 } });

        Assert.Equal("[1,2,3]", result);
    }

    [Fact]
    public void Render_ToJsonFilter_WithNull_ReturnsNull()
    {
        string template = "{{ missing | to_json }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { defined = "value" });

        Assert.Equal("null", result);
    }

    [Fact]
    public void Render_PathJoinFilter_JoinsPaths()
    {
        string template = "{{ parts | path_join }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { parts = new[] { "home", "user", "file.txt" } });

        string expected = Path.Combine("home", "user", "file.txt");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Render_PathJoinFilter_WithArguments_JoinsPaths()
    {
        string template = "{{ base | path_join('subdir', 'file.txt') }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { @base = "/home" });

        string expected = Path.Combine("/home", "subdir", "file.txt");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Render_B64EncodeFilter_EncodesBase64()
    {
        string template = "{{ text | b64encode }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { text = "hello" });

        Assert.Equal("aGVsbG8=", result);
    }

    [Fact]
    public void Render_B64DecodeFilter_DecodesBase64()
    {
        string template = "{{ encoded | b64decode }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { encoded = "aGVsbG8=" });

        Assert.Equal("hello", result);
    }

    [Fact]
    public void Render_B64EncodeDecodeRoundtrip_PreservesValue()
    {
        string template = "{{ text | b64encode | b64decode }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { text = "Hello World!" });

        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public void Render_ToJsonWithUpperFilter_CombinesFilters()
    {
        string template = "{{ name | upper | to_json }}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { name = "alice" });

        Assert.Equal("\"ALICE\"", result);
    }

    [Fact]
    public void Render_PathJoinInForLoop_WorksCorrectly()
    {
        string template = "{% for dir in dirs %}{{ base | path_join(dir) }}\n{% endfor %}";

        Template parsed = Template.Parse(template);
        string result = parsed.Render(new { @base = "/home", dirs = new[] { "user1", "user2" } });

        string expected1 = Path.Combine("/home", "user1");
        string expected2 = Path.Combine("/home", "user2");
        Assert.Contains(expected1, result);
        Assert.Contains(expected2, result);
    }
}