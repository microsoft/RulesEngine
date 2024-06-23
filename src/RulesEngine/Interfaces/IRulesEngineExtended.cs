namespace RulesEngine.Interfaces;

/// <summary>
///     Extended Rules Engine with IRule and IWorkflowSupport
/// </summary>
public interface IRulesEngineExtended : IRulesEngine
{
    /// <summary>
    ///     Adds new workflows to RulesEngine
    /// </summary>
    /// <param name="workflows">The workflows to add</param>
    void AddWorkflow(params IWorkflow[] workflows);


    /// <summary>
    ///     Adds or updates the workflow.
    /// </summary>
    /// <param name="workflows">The workflows.</param>
    void AddOrUpdateWorkflow(params IWorkflow[] workflows);
}
