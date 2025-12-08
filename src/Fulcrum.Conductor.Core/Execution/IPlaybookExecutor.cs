using Fulcrum.Conductor.Core.Playbooks;
using Fulcrum.Conductor.Jinja.Rendering;

namespace Fulcrum.Conductor.Core.Execution;

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
