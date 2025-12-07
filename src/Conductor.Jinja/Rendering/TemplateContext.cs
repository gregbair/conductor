using System.Reflection;

namespace Conductor.Jinja.Rendering;

/// <summary>
///     Stores variables and their values for template rendering.
/// </summary>
public sealed class TemplateContext
{
    private readonly TemplateContext? _parent;
    private readonly Dictionary<string, object?> _variables;

    private TemplateContext(TemplateContext? parent)
    {
        _variables = new Dictionary<string, object?>();
        _parent = parent;
    }

    /// <summary>
    ///     Creates a new template context.
    /// </summary>
    public static TemplateContext Create()
    {
        return new TemplateContext(null);
    }

    /// <summary>
    ///     Creates a template context from an object.
    /// </summary>
    public static TemplateContext FromObject(object? obj)
    {
        TemplateContext context = Create();

        if (obj == null)
        {
            return context;
        }

        // Handle dictionaries
        if (obj is IDictionary<string, object?> dict)
        {
            foreach (KeyValuePair<string, object?> kvp in dict)
            {
                context.SetVariable(kvp.Key, kvp.Value);
            }

            return context;
        }

        // Handle anonymous objects and POCOs via reflection
        Type type = obj.GetType();
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo property in properties)
        {
            context.SetVariable(property.Name, property.GetValue(obj));
        }

        return context;
    }

    /// <summary>
    ///     Gets a variable value.
    /// </summary>
    public object? GetVariable(string name)
    {
        if (_variables.TryGetValue(name, out object? value))
        {
            return value;
        }

        if (_parent != null)
        {
            return _parent.GetVariable(name);
        }

        return null;
    }

    /// <summary>
    ///     Sets a variable value.
    /// </summary>
    public void SetVariable(string name, object? value)
    {
        _variables[name] = value;
    }

    /// <summary>
    ///     Checks if a variable is defined.
    /// </summary>
    public bool IsDefined(string name)
    {
        if (_variables.ContainsKey(name))
        {
            return true;
        }

        if (_parent != null)
        {
            return _parent.IsDefined(name);
        }

        return false;
    }

    /// <summary>
    ///     Creates a child scope (for loops, etc.).
    /// </summary>
    public TemplateContext CreateChildScope()
    {
        return new TemplateContext(this);
    }
}