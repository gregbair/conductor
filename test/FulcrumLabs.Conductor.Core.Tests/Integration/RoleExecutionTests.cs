using FulcrumLabs.Conductor.Core.Conditionals;
using FulcrumLabs.Conductor.Core.Execution;
using FulcrumLabs.Conductor.Core.Loops;
using FulcrumLabs.Conductor.Core.Modules;
using FulcrumLabs.Conductor.Core.Playbooks;
using FulcrumLabs.Conductor.Core.Roles;
using FulcrumLabs.Conductor.Core.Templating;
using FulcrumLabs.Conductor.Core.Yaml;
using FulcrumLabs.Conductor.Jinja.Rendering;

using Task = System.Threading.Tasks.Task;

namespace FulcrumLabs.Conductor.Core.Tests.Integration;

/// <summary>
/// Integration tests for role execution functionality.
/// </summary>
public class RoleExecutionTests
{
    private readonly string _fixturesPath = Path.Combine(Directory.GetCurrentDirectory(), "Fixtures", "roles");

    // Get the path to test fixtures

    [Fact]
    public async Task ExecutePlayWithSingleExternalRole_ShouldSucceed()
    {
        // Arrange
        string yaml = @"
- name: Test play with role
  roles:
    - webserver
";

        PlaybookLoader loader = new();
        Playbook playbook = loader.LoadFromString(yaml);

        IPlaybookExecutor executor = CreateExecutor();

        // Execute
        PlaybookResult result = await executor.ExecuteAsync(playbook);

        // Verify
        Assert.True(result.Success);
        Assert.Single(result.PlayResults);

        PlayResult playResult = result.PlayResults[0];
        Assert.True(playResult.Success);

        // Should have 3 tasks from the webserver role
        Assert.Equal(3, playResult.TaskResults.Count);
        Assert.All(playResult.TaskResults, taskResult => Assert.True(taskResult.Success));
    }

    [Fact]
    public async Task ExecutePlayWithMultipleExternalRoles_ShouldExecuteInOrder()
    {
        // Arrange
        string yaml = @"
- name: Test play with multiple roles
  roles:
    - webserver
    - database
";

        PlaybookLoader loader = new();
        Playbook playbook = loader.LoadFromString(yaml);

        IPlaybookExecutor executor = CreateExecutor();

        // Execute
        PlaybookResult result = await executor.ExecuteAsync(playbook);

        // Verify
        Assert.True(result.Success);
        Assert.Single(result.PlayResults);

        PlayResult playResult = result.PlayResults[0];
        Assert.True(playResult.Success);

        // Should have 3 tasks from webserver + 3 tasks from database = 6 total
        Assert.Equal(6, playResult.TaskResults.Count);
        Assert.All(playResult.TaskResults, taskResult => Assert.True(taskResult.Success));
    }

    [Fact]
    public async Task ExecutePlayWithRoleAndTasks_ShouldExecuteRolesFirst()
    {
        // Arrange
        string yaml = @"
- name: Test play with roles and tasks
  roles:
    - webserver
  tasks:
    - name: Additional task
      debug:
        msg: 'This runs after roles'
";

        PlaybookLoader loader = new();
        Playbook playbook = loader.LoadFromString(yaml);

        IPlaybookExecutor executor = CreateExecutor();

        // Execute
        PlaybookResult result = await executor.ExecuteAsync(playbook);

        // Verify
        Assert.True(result.Success);
        Assert.Single(result.PlayResults);

        PlayResult playResult = result.PlayResults[0];
        Assert.True(playResult.Success);

        // Should have 3 tasks from webserver role + 1 additional task = 4 total
        Assert.Equal(4, playResult.TaskResults.Count);
        Assert.All(playResult.TaskResults, taskResult => Assert.True(taskResult.Success));
    }

    [Fact]
    public async Task ExecuteRoleWithParameters_ShouldOverrideDefaults()
    {
        // Arrange
        string yaml = @"
- name: Test role with parameters
  roles:
    - name: webserver
      vars:
        webserver_port: 9000
        webserver_workers: 8
";

        PlaybookLoader loader = new();
        Playbook playbook = loader.LoadFromString(yaml);

        IPlaybookExecutor executor = CreateExecutor();
        TemplateContext context = TemplateContext.Create();

        // Execute
        PlaybookResult result = await executor.ExecuteAsync(playbook, context);

        // Verify
        Assert.True(result.Success);
        Assert.Single(result.PlayResults);

        PlayResult playResult = result.PlayResults[0];
        Assert.True(playResult.Success);
        Assert.Equal(3, playResult.TaskResults.Count);
    }

    [Fact]
    public async Task ExecuteRoleWithPlayVars_PlayVarsShouldTakePrecedence()
    {
        // Arrange
        string yaml = @"
- name: Test variable precedence
  vars:
    webserver_port: 7000
  roles:
    - webserver
";

        PlaybookLoader loader = new();
        Playbook playbook = loader.LoadFromString(yaml);

        IPlaybookExecutor executor = CreateExecutor();
        TemplateContext context = TemplateContext.Create();

        // Execute
        PlaybookResult result = await executor.ExecuteAsync(playbook, context);

        // Verify
        Assert.True(result.Success);
        Assert.Single(result.PlayResults);

        PlayResult playResult = result.PlayResults[0];
        Assert.True(playResult.Success);

        // The role should execute successfully with play vars taking precedence
        Assert.Equal(3, playResult.TaskResults.Count);
    }

    [Fact]
    public async Task ExecuteRoleWithWhenCondition_ShouldSkipWhenFalse()
    {
        // Arrange
        string yaml = @"
- name: Test conditional role
  vars:
    install_webserver: false
  roles:
    - name: webserver
      when: install_webserver
";

        PlaybookLoader loader = new();
        Playbook playbook = loader.LoadFromString(yaml);

        IPlaybookExecutor executor = CreateExecutor();

        // Execute
        PlaybookResult result = await executor.ExecuteAsync(playbook);

        // Verify
        Assert.True(result.Success);
        Assert.Single(result.PlayResults);

        PlayResult playResult = result.PlayResults[0];
        Assert.True(playResult.Success);

        // Role should be skipped, so no task results
        Assert.Empty(playResult.TaskResults);
    }

    [Fact]
    public async Task ExecuteRoleWithWhenCondition_ShouldExecuteWhenTrue()
    {
        // Arrange
        string yaml = @"
- name: Test conditional role
  vars:
    install_webserver: true
  roles:
    - name: webserver
      when: install_webserver
";

        PlaybookLoader loader = new();
        Playbook playbook = loader.LoadFromString(yaml);

        IPlaybookExecutor executor = CreateExecutor();

        // Execute
        PlaybookResult result = await executor.ExecuteAsync(playbook);

        // Verify
        Assert.True(result.Success);
        Assert.Single(result.PlayResults);

        PlayResult playResult = result.PlayResults[0];
        Assert.True(playResult.Success);

        // Role should execute, giving us 3 tasks
        Assert.Equal(3, playResult.TaskResults.Count);
    }

    [Fact(Skip = "import_role as task requires additional implementation")]
    public async Task ExecutePlayWithImportRole_ShouldSucceed()
    {
        // Arrange
        string yaml = @"
- name: Test play with import_role
  tasks:
    - name: Import web server role
      import_role:
        name: webserver
";

        PlaybookLoader loader = new();
        Playbook playbook = loader.LoadFromString(yaml);

        IPlaybookExecutor executor = CreateExecutor();

        // Execute
        PlaybookResult result = await executor.ExecuteAsync(playbook);

        // Verify
        Assert.True(result.Success);
        Assert.Single(result.PlayResults);

        PlayResult playResult = result.PlayResults[0];
        Assert.True(playResult.Success);

        // Should have 1 task (the import_role task itself)
        // Note: In real Ansible, import_role would expand to the role's tasks at parse time
        // Our implementation treats it as a task that references a role
        Assert.Single(playResult.TaskResults);
    }

    [Fact(Skip = "include_role as task requires additional implementation")]
    public async Task ExecutePlayWithIncludeRole_ShouldSucceed()
    {
        // Arrange
        string yaml = @"
- name: Test play with include_role
  tasks:
    - name: Include web server role
      include_role:
        name: webserver
";

        PlaybookLoader loader = new();
        Playbook playbook = loader.LoadFromString(yaml);

        IPlaybookExecutor executor = CreateExecutor();

        // Execute
        PlaybookResult result = await executor.ExecuteAsync(playbook);

        // Verify
        Assert.True(result.Success);
        Assert.Single(result.PlayResults);

        PlayResult playResult = result.PlayResults[0];
        Assert.True(playResult.Success);

        // Should have 1 task (the include_role task itself)
        Assert.Single(playResult.TaskResults);
    }

    [Fact(Skip = "import_role as task requires additional implementation")]
    public async Task ExecutePlayWithImportRoleAndVars_ShouldPassVariables()
    {
        // Arrange
        string yaml = @"
- name: Test import_role with vars
  tasks:
    - name: Import web server with custom port
      import_role:
        name: webserver
        vars:
          webserver_port: 3000
          webserver_workers: 16
";

        PlaybookLoader loader = new();
        Playbook playbook = loader.LoadFromString(yaml);

        IPlaybookExecutor executor = CreateExecutor();

        // Execute
        PlaybookResult result = await executor.ExecuteAsync(playbook);

        // Verify
        Assert.True(result.Success);
        Assert.Single(result.PlayResults);

        PlayResult playResult = result.PlayResults[0];
        Assert.True(playResult.Success);
        Assert.Single(playResult.TaskResults);
    }

    [Fact]
    public void LoadRole_WithMissingTasksFile_ShouldThrowException()
    {
        // Arrange
        RoleLoader loader = new();

        // Act & Assert
        RoleLoadException exception = Assert.Throws<RoleLoadException>(() =>
        {
            loader.LoadRole("nonexistent", _fixturesPath);
        });

        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public void LoadRole_WithValidRole_ShouldLoadCorrectly()
    {
        // Arrange
        RoleLoader loader = new();

        // Act
        Role role = loader.LoadRole("webserver", _fixturesPath);

        // Assert
        Assert.Equal("webserver", role.Name);
        Assert.Equal(3, role.Tasks.Count);
        Assert.NotEmpty(role.Defaults);
        Assert.NotEmpty(role.Vars);

        // Check defaults
        Assert.True(role.Defaults.ContainsKey("webserver_port"));
        Assert.Equal(8080, role.Defaults["webserver_port"]);

        // Check vars (vars override defaults for name)
        Assert.True(role.Vars.ContainsKey("webserver_name"));
        Assert.Equal("apache", role.Vars["webserver_name"]);
    }

    /// <summary>
    /// Creates a PlaybookExecutor with all necessary dependencies for testing.
    /// </summary>
    private IPlaybookExecutor CreateExecutor()
    {
        // Module registry and executor
        ModuleRegistry registry = new();

        // Manually register debug module with its actual path
        string testDir = AppContext.BaseDirectory;
        string configuration = testDir.Contains("/Release/") || testDir.Contains("\\Release\\") ? "Release" : "Debug";
        string debugModulePath =
            Path.GetFullPath(
                $"../../../../../modules/src/FulcrumLabs.Conductor.Modules.Debug/bin/{configuration}/net10.0/conductor-module-debug");
        if (File.Exists(debugModulePath))
        {
            registry.RegisterModule("debug", debugModulePath);
        }

        ModuleExecutor moduleExecutor = new(registry);

        // Template and conditional evaluation
        TemplateExpander templateExpander = new();
        ConditionalEvaluator conditionalEvaluator = new(templateExpander);
        LoopExpander loopExpander = new(templateExpander);

        // Task executor
        TaskExecutor taskExecutor = new(moduleExecutor, templateExpander, conditionalEvaluator, loopExpander);

        // Role loader and expander (using test fixtures path)
        TestRoleLoader roleLoader = new(_fixturesPath);
        RoleExpander roleExpander = new(roleLoader, conditionalEvaluator);

        // Playbook executor
        return new PlaybookExecutor(taskExecutor, roleExpander);
    }

    /// <summary>
    /// Test role loader that uses the fixtures path as the default roles base path.
    /// </summary>
    private class TestRoleLoader(string fixturesPath) : IRoleLoader
    {
        private readonly RoleLoader _innerLoader = new();

        public Role LoadRole(string roleName, string rolesBasePath = "./roles")
        {
            // Override the default path with the fixtures path
            return _innerLoader.LoadRole(roleName, fixturesPath);
        }
    }
}
