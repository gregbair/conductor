using FulcrumLabs.Conductor.Jinja.Filters.Ansible;
using FulcrumLabs.Conductor.Jinja.Filters.BuiltIn;

namespace FulcrumLabs.Conductor.Jinja.Filters;

/// <summary>
///     Registry for managing and looking up filters.
/// </summary>
public sealed class FilterRegistry
{
    private readonly Dictionary<string, IFilter> _filters = new();

    /// <summary>
    ///     Registers a filter.
    /// </summary>
    public void RegisterFilter(IFilter filter)
    {
        _filters[filter.Name] = filter;
    }

    /// <summary>
    ///     Gets a filter by name.
    /// </summary>
    /// <param name="name">The filter name.</param>
    /// <returns>The filter, or null if not found.</returns>
    public IFilter? GetFilter(string name)
    {
        _filters.TryGetValue(name, out IFilter? filter);
        return filter;
    }

    /// <summary>
    ///     Checks if a filter exists.
    /// </summary>
    public bool HasFilter(string name)
    {
        return _filters.ContainsKey(name);
    }

    /// <summary>
    ///     Creates a default registry with all built-in filters.
    /// </summary>
    public static FilterRegistry CreateDefault()
    {
        FilterRegistry registry = new();

        // String filters
        registry.RegisterFilter(new UpperFilter());
        registry.RegisterFilter(new LowerFilter());
        registry.RegisterFilter(new CapitalizeFilter());

        // Collection filters
        registry.RegisterFilter(new LengthFilter());
        registry.RegisterFilter(new FirstFilter());
        registry.RegisterFilter(new LastFilter());
        registry.RegisterFilter(new JoinFilter());
        registry.RegisterFilter(new SplitFilter());

        // Utility filters
        registry.RegisterFilter(new DefaultFilter());

        // Ansible filters
        registry.RegisterFilter(new ToJsonFilter());
        registry.RegisterFilter(new FromJsonFilter());
        registry.RegisterFilter(new PathJoinFilter());
        registry.RegisterFilter(new B64EncodeFilter());
        registry.RegisterFilter(new B64DecodeFilter());

        return registry;
    }
}