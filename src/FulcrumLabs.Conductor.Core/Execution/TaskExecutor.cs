using FulcrumLabs.Conductor.Jinja.Rendering;

using FulcrumLabs.Conductor.Core.Conditionals;
using FulcrumLabs.Conductor.Core.Loops;
using FulcrumLabs.Conductor.Core.Modules;
using FulcrumLabs.Conductor.Core.Tasks;
using FulcrumLabs.Conductor.Core.Templating;

using Task = FulcrumLabs.Conductor.Core.Tasks.Task;
using Tasks_Task = FulcrumLabs.Conductor.Core.Tasks.Task;

namespace FulcrumLabs.Conductor.Core.Execution;

/// <summary>
///     Executes tasks by coordinating template expansion, conditional evaluation,
///     loop expansion, and module execution.
/// </summary>
public sealed class TaskExecutor(
    ModuleExecutor moduleExecutor,
    ITemplateExpander templateExpander,
    IConditionalEvaluator conditionalEvaluator,
    ILoopExpander loopExpander)
    : ITaskExecutor
{
    /// <inheritdoc />
    public async Task<TaskResult> ExecuteAsync(Tasks_Task task, TemplateContext context, CancellationToken cancellationToken)
    {
        try
        {
            if (!conditionalEvaluator.Evaluate(task.When, context))
            {
                return TaskResult.CreateSkipped(task.Name);
            }

            if (task.Loop != null)
            {
                return await ExecuteLoopAsync(task, context, cancellationToken);
            }

            return await ExecuteSingleIterationAsync(task, context, cancellationToken);
        }
        catch (TaskExecutionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return task.IgnoreErrors
                ? TaskResult.CreateFailedButIgnored(task.Name, ex.Message)
                : throw new TaskExecutionException($"Task '{task.Name}' failed: {ex.Message}", ex);
        }
    }

    private async Task<TaskResult> ExecuteLoopAsync(Tasks_Task task, TemplateContext context,
        CancellationToken cancellationToken)
    {
        IEnumerable<object?> items = loopExpander.ExpandLoop(task.Loop!, context);
        List<ModuleResult> iterationResults = [];
        bool anyChanged = false;
        bool anyFailed = false;

        foreach (object? item in items)
        {
            // Create task scope with loop variable
            TemplateContext iterationContext = context.CreateChildScope();
            iterationContext.SetVariable("item", item);

            // Execute single iteration
            try
            {
                ModuleResult result =
                    await ExecuteModuleWithErrorHandlingAsync(task, iterationContext, cancellationToken);
                iterationResults.Add(result);

                if (result.Changed)
                {
                    anyChanged = true;
                }

                if (!result.Success)
                {
                    anyFailed = true;

                    if (!task.IgnoreErrors)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!task.IgnoreErrors)
                {
                    throw new TaskExecutionException(
                        $"Task '{task.Name}' failed on loop iteration (item: {item}): {ex.Message}", ex);
                }

                // Create a failed module result for this iteration
                ModuleResult failedResult = new()
                {
                    Success = false,
                    Changed = false,
                    Message = $"Iteration failed: {ex.Message}",
                    Facts = new Dictionary<string, object?>()
                };
                iterationResults.Add(failedResult);
                anyFailed = true;
            }
        }

        // Aggregate loop results
        bool success = task.IgnoreErrors || !anyFailed;

        return new TaskResult
        {
            Success = success,
            Changed = anyChanged,
            Failed = anyFailed,
            Skipped = false,
            Message = $"Task '{task.Name}' executed {iterationResults.Count} iteration(s)",
            IterationResults = iterationResults
        };
    }

    private async Task<TaskResult> ExecuteSingleIterationAsync(
        Tasks_Task task,
        TemplateContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            ModuleResult moduleResult = await ExecuteModuleWithErrorHandlingAsync(task, context, cancellationToken);

            // Register facts if specified
            Dictionary<string, object?> registeredFacts = [];
            if (string.IsNullOrWhiteSpace(task.RegisterAs))
            {
                return new TaskResult
                {
                    Success = moduleResult.Success || task.IgnoreErrors,
                    Changed = moduleResult.Changed,
                    Failed = !moduleResult.Success,
                    Skipped = false,
                    Message = moduleResult.Message,
                    ModuleResult = moduleResult,
                    RegisteredFacts = registeredFacts
                };
            }

            Dictionary<string, object?> resultDict = ConvertModuleResultToDict(moduleResult);
            registeredFacts[task.RegisterAs] = resultDict;
            context.SetVariable(task.RegisterAs, resultDict);

            return new TaskResult
            {
                Success = moduleResult.Success || task.IgnoreErrors,
                Changed = moduleResult.Changed,
                Failed = !moduleResult.Success,
                Skipped = false,
                Message = moduleResult.Message,
                ModuleResult = moduleResult,
                RegisteredFacts = registeredFacts
            };
        }
        catch (Exception ex)
        {
            return task.IgnoreErrors
                ? TaskResult.CreateFailedButIgnored(task.Name, ex.Message)
                : throw new TaskExecutionException($"Task '{task.Name}' failed: {ex.Message}", ex);
        }
    }

    private async Task<ModuleResult> ExecuteModuleWithErrorHandlingAsync(
        Tasks_Task task,
        TemplateContext context,
        CancellationToken cancellationToken)
    {
        // Expand parameters
        Dictionary<string, object?> expandedParams = templateExpander.ExpandParameters(task.Parameters, context);

        // Execute module (note: ModuleExecutor doesn't currently support CancellationToken)
        ModuleResult moduleResult = await moduleExecutor.ExecuteAsync(task.Module, expandedParams);

        // Apply conditional overrides
        bool failed = EvaluateFailedWhen(task, moduleResult, context);
        bool changed = EvaluateChangedWhen(task, moduleResult, context);

        // Return modified result
        return new ModuleResult
        {
            Success = !failed, Changed = changed, Message = moduleResult.Message, Facts = moduleResult.Facts
        };
    }

    private bool EvaluateFailedWhen(Tasks_Task task, ModuleResult result, TemplateContext context)
    {
        if (string.IsNullOrWhiteSpace(task.FailedWhen))
        {
            return !result.Success; // Default: use module's success
        }

        // Inject result variables into context for evaluation
        TemplateContext evalContext = context.CreateChildScope();
        InjectResultVariables(evalContext, result);

        return conditionalEvaluator.Evaluate(task.FailedWhen, evalContext);
    }

    private bool EvaluateChangedWhen(Tasks_Task task, ModuleResult result, TemplateContext context)
    {
        if (string.IsNullOrWhiteSpace(task.ChangedWhen))
        {
            return result.Changed; // Default: use module's changed
        }

        // Inject result variables into context for evaluation
        TemplateContext evalContext = context.CreateChildScope();
        InjectResultVariables(evalContext, result);

        return conditionalEvaluator.Evaluate(task.ChangedWhen, evalContext);
    }

    private static void InjectResultVariables(TemplateContext context, ModuleResult result)
    {
        // Make result properties available as variables
        context.SetVariable("success", result.Success);
        context.SetVariable("changed", result.Changed);
        context.SetVariable("message", result.Message);

        // Inject all facts as variables
        foreach ((string key, object? value) in result.Facts)
        {
            context.SetVariable(key, value);
        }
    }

    private static Dictionary<string, object?> ConvertModuleResultToDict(ModuleResult result)
    {
        Dictionary<string, object?> dict = new()
        {
            ["success"] = result.Success, ["changed"] = result.Changed, ["message"] = result.Message
        };

        // Add all facts to the dictionary
        foreach ((string key, object? value) in result.Facts)
        {
            dict[key] = value;
        }

        return dict;
    }
}