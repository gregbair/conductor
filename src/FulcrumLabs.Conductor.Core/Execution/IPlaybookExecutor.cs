using FulcrumLabs.Conductor.Jinja.Rendering;

using FulcrumLabs.Conductor.Core.Playbooks;

namespace FulcrumLabs.Conductor.Core.Execution;

/// <summary>
/// Executes a complete playbook.
/// </summary>
public interface IPlaybookExecutor
{
    /// <summary>
    /// Executes a playbook with an optional initial context.
    /// </summary>
    Task<PlaybookResult> ExecuteAsync(
        Playbook playbook,
        TemplateContext? initialContext = null,
        CancellationToken cancellationToken = default);
}
