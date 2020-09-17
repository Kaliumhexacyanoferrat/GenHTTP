namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Functionality that wraps around a regular handler to add a
    /// concern such as response compression.
    /// </summary>
    public interface IConcern : IHandler
    {

        /// <summary>
        /// The actual handler the concern is added to.
        /// </summary>
        IHandler Content { get; }

    }

}
