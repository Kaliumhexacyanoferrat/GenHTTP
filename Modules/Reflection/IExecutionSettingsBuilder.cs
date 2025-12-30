namespace GenHTTP.Modules.Reflection;

/// <summary>
/// A protocol for builders that will internally create a <see cref="ExecutionSettings"/>
/// instance.
/// </summary>
/// <typeparam name="T">The builder type to be returned</typeparam>
public interface IExecutionSettingsBuilder<out T>
{

    /// <summary>
    /// Sets the execution mode to be used to run functions.
    /// </summary>
    /// <param name="mode">The mode to be used for execution</param>
    T ExecutionMode(ExecutionMode mode);

}
