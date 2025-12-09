using FulcrumLabs.Conductor.Core.Playbooks;
using FulcrumLabs.Conductor.Jinja.Rendering;

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
