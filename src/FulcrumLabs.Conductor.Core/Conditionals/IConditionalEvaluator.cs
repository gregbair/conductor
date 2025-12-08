using FulcrumLabs.Conductor.Jinja.Rendering;

namespace FulcrumLabs.Conductor.Core.Conditionals;

/// <summary>
/// Evaluates conditional expressions (when, failed_when, changed_when).
/// </summary>
public interface IConditionalEvaluator
{
    /// <summary>
    /// Evaluates a conditional expression.
    /// Returns true if the condition passes, false otherwise.
    /// </summary>
    bool Evaluate(string? condition, TemplateContext context);
}
