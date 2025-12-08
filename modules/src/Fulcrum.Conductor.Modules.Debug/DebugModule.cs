using System.Collections.Generic;
using System.Threading.Tasks;

using Fulcrum.Conductor.Core.Modules;
using Fulcrum.Conductor.Modules.Common;

namespace Fulcrum.Conductor.Modules.Debug;

/// <summary>
///     Prints debug messages, useful for troubleshooting
/// </summary>
public class DebugModule : ModuleBase
{
    protected override Task<ModuleResult> ExecuteAsync(Dictionary<string, object?> vars)
    {
        // Debug module supports either 'msg' or 'var' parameter
        string message;

        if (TryGetRequiredParameter(vars, "msg", out string msg))
        {
            // Direct message
            message = msg;
        }
        else if (TryGetRequiredParameter(vars, "var", out string varName))
        {
            // Look up the variable by name
            if (vars.TryGetValue(varName, out object? varValue))
            {
                message = $"{varName}: {varValue?.ToString() ?? "(null)"}";
            }
            else
            {
                message = $"{varName}: VARIABLE IS NOT DEFINED!";
            }
        }
        else
        {
            return Task.FromResult(Failure("Either 'msg' or 'var' parameter is required"));
        }

        // Debug module never changes anything (changed=false)
        return Task.FromResult(Success(message, changed: false));
    }
}