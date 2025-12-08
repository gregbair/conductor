using Fulcrum.Conductor.Core.Conditionals;
using Fulcrum.Conductor.Core.Execution;
using Fulcrum.Conductor.Core.Loops;
using Fulcrum.Conductor.Core.Modules;
using Fulcrum.Conductor.Core.Playbooks;
using Fulcrum.Conductor.Core.Templating;
using Fulcrum.Conductor.Core.Yaml;
using Fulcrum.Conductor.Jinja.Rendering;

namespace Fulcrum.Conductor.Core.Tests.Integration;

public class PlaybookExecutionTests
{
    [Fact]
    public async Task ExecutePlaybook_WithDebugModule_RunsSuccessfully()
    {
        // Setup
        string yaml = """
            - name: Test playbook
              vars:
                greeting: "Hello"
              tasks:
                - name: Print greeting
                  debug:
                    msg: "{{ greeting }} World"
            """;

        // Load playbook
        PlaybookLoader loader = new();
        Playbook playbook = loader.LoadFromString(yaml);

        // Setup executors
        ModuleRegistry registry = new();

        // Manually register debug module with its actual path
        string debugModulePath = Path.GetFullPath("../../../../../modules/src/Fulcrum.Conductor.Modules.Debug/bin/Debug/net10.0/conductor-module-debug");
        if (File.Exists(debugModulePath))
        {
            registry.RegisterModule("debug", debugModulePath);
        }

        ModuleExecutor moduleExecutor = new(registry);

        TemplateExpander templateExpander = new();
        ConditionalEvaluator conditionalEvaluator = new(templateExpander);
        LoopExpander loopExpander = new(templateExpander);

        TaskExecutor taskExecutor = new(moduleExecutor, templateExpander, conditionalEvaluator, loopExpander);
        PlaybookExecutor playbookExecutor = new(taskExecutor);

        // Execute
        PlaybookResult result = await playbookExecutor.ExecuteAsync(playbook);

        // Verify
        Assert.True(result.Success);
        Assert.Single(result.PlayResults);
        Assert.True(result.PlayResults[0].Success);
        Assert.Single(result.PlayResults[0].TaskResults);
        Assert.True(result.PlayResults[0].TaskResults[0].Success);
    }

    [Fact]
    public async Task ExecutePlaybook_WithLoop_ExecutesMultipleIterations()
    {
        string yaml = """
            - name: Test loop
              tasks:
                - name: Loop over items
                  debug:
                    msg: "Item: {{ item }}"
                  loop:
                    - one
                    - two
                    - three
            """;

        PlaybookLoader loader = new();
        Playbook playbook = loader.LoadFromString(yaml);

        ModuleRegistry registry = new();

        // Manually register debug module
        string debugModulePath = Path.GetFullPath("../../../../../modules/src/Fulcrum.Conductor.Modules.Debug/bin/Debug/net10.0/conductor-module-debug");
        if (File.Exists(debugModulePath))
        {
            registry.RegisterModule("debug", debugModulePath);
        }

        ModuleExecutor moduleExecutor = new(registry);

        TemplateExpander templateExpander = new();
        ConditionalEvaluator conditionalEvaluator = new(templateExpander);
        LoopExpander loopExpander = new(templateExpander);

        TaskExecutor taskExecutor = new(moduleExecutor, templateExpander, conditionalEvaluator, loopExpander);
        PlaybookExecutor playbookExecutor = new(taskExecutor);

        PlaybookResult result = await playbookExecutor.ExecuteAsync(playbook);

        Assert.True(result.Success);
        Assert.Single(result.PlayResults);
        Assert.Single(result.PlayResults[0].TaskResults);

        // Loop task should have 3 iteration results
        Assert.Equal(3, result.PlayResults[0].TaskResults[0].IterationResults.Count);
    }

    [Fact]
    public async Task ExecutePlaybook_WithConditional_SkipsWhenFalse()
    {
        string yaml = """
            - name: Test conditional
              vars:
                should_run: false
              tasks:
                - name: Skipped task
                  debug:
                    msg: "This should be skipped"
                  when: should_run
                - name: Running task
                  debug:
                    msg: "This should run"
            """;

        PlaybookLoader loader = new();
        Playbook playbook = loader.LoadFromString(yaml);

        ModuleRegistry registry = new();

        // Manually register debug module
        string debugModulePath = Path.GetFullPath("../../../../../modules/src/Fulcrum.Conductor.Modules.Debug/bin/Debug/net10.0/conductor-module-debug");
        if (File.Exists(debugModulePath))
        {
            registry.RegisterModule("debug", debugModulePath);
        }

        ModuleExecutor moduleExecutor = new(registry);

        TemplateExpander templateExpander = new();
        ConditionalEvaluator conditionalEvaluator = new(templateExpander);
        LoopExpander loopExpander = new(templateExpander);

        TaskExecutor taskExecutor = new(moduleExecutor, templateExpander, conditionalEvaluator, loopExpander);
        PlaybookExecutor playbookExecutor = new(taskExecutor);

        PlaybookResult result = await playbookExecutor.ExecuteAsync(playbook);

        Assert.True(result.Success);
        Assert.Single(result.PlayResults);
        Assert.Equal(2, result.PlayResults[0].TaskResults.Count);

        // First task should be skipped
        Assert.True(result.PlayResults[0].TaskResults[0].Skipped);

        // Second task should succeed
        Assert.True(result.PlayResults[0].TaskResults[1].Success);
        Assert.False(result.PlayResults[0].TaskResults[1].Skipped);
    }
}
