using Fulcrum.Conductor.Core.Playbooks;
using Fulcrum.Conductor.Core.Yaml;

namespace Fulcrum.Conductor.Core.Tests.Yaml;

public class PlaybookLoaderTests
{
    private readonly PlaybookLoader _loader = new();

    [Fact]
    public void LoadFromString_WithSimplePlaybook_LoadsSuccessfully()
    {
        string yaml = """
            - name: Test play
              tasks:
                - name: Print message
                  debug:
                    msg: "Hello World"
            """;

        Playbook playbook = _loader.LoadFromString(yaml);

        Assert.Single(playbook.Plays);
        Assert.Equal("Test play", playbook.Plays[0].Name);
        Assert.Single(playbook.Plays[0].Tasks);
        Assert.Equal("Print message", playbook.Plays[0].Tasks[0].Name);
        Assert.Equal("debug", playbook.Plays[0].Tasks[0].Module);
    }

    [Fact]
    public void LoadFromString_WithVariables_LoadsVariables()
    {
        string yaml = """
            - name: Test play
              vars:
                greeting: "Hello"
              tasks:
                - name: Print greeting
                  debug:
                    msg: "{{ greeting }}"
            """;

        Playbook playbook = _loader.LoadFromString(yaml);

        Assert.Single(playbook.Plays);
        Assert.Contains("greeting", playbook.Plays[0].Vars.Keys);
        Assert.Equal("Hello", playbook.Plays[0].Vars["greeting"]);
    }

    [Fact]
    public void LoadFromString_WithLoop_LoadsLoop()
    {
        string yaml = """
            - name: Test play
              tasks:
                - name: Loop task
                  debug:
                    msg: "{{ item }}"
                  loop:
                    - one
                    - two
                    - three
            """;

        Playbook playbook = _loader.LoadFromString(yaml);

        Assert.Single(playbook.Plays);
        Assert.Single(playbook.Plays[0].Tasks);
        Assert.NotNull(playbook.Plays[0].Tasks[0].Loop);
    }

    [Fact]
    public void LoadFromString_WithConditional_LoadsConditional()
    {
        string yaml = """
            - name: Test play
              tasks:
                - name: Conditional task
                  debug:
                    msg: "Running"
                  when: ansible_os_family == "Debian"
            """;

        Playbook playbook = _loader.LoadFromString(yaml);

        Assert.Single(playbook.Plays);
        Assert.Single(playbook.Plays[0].Tasks);
        Assert.Equal("ansible_os_family == \"Debian\"", playbook.Plays[0].Tasks[0].When);
    }

    [Fact]
    public void LoadFromString_WithShellModule_LoadsParameters()
    {
        string yaml = """
            - name: Test play
              tasks:
                - name: Run shell command
                  shell: echo "test"
            """;

        Playbook playbook = _loader.LoadFromString(yaml);

        Assert.Single(playbook.Plays);
        Assert.Single(playbook.Plays[0].Tasks);
        Assert.Equal("shell", playbook.Plays[0].Tasks[0].Module);
        Assert.Contains("cmd", playbook.Plays[0].Tasks[0].Parameters.Keys);
        Assert.Equal("echo \"test\"", playbook.Plays[0].Tasks[0].Parameters["cmd"]);
    }

    [Fact]
    public void LoadFromString_WithBooleanVariable_LoadsAsBoolean()
    {
        string yaml = """
            - name: Test play
              vars:
                should_run: false
                other_flag: true
              tasks:
                - name: Test task
                  debug:
                    msg: "test"
            """;

        Playbook playbook = _loader.LoadFromString(yaml);

        Assert.Single(playbook.Plays);
        Assert.Contains("should_run", playbook.Plays[0].Vars.Keys);
        Assert.Contains("other_flag", playbook.Plays[0].Vars.Keys);

        // Verify the types are actual booleans, not strings
        Assert.IsType<bool>(playbook.Plays[0].Vars["should_run"]);
        Assert.IsType<bool>(playbook.Plays[0].Vars["other_flag"]);

        Assert.False((bool)playbook.Plays[0].Vars["should_run"]!);
        Assert.True((bool)playbook.Plays[0].Vars["other_flag"]!);
    }
}
