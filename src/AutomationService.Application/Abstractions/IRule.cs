namespace AutomationService.Application.Abstractions;

public interface IRule<in T>
{
    string Name { get; }
    string Description { get; }
    bool IsSatisfied(T input);
}
