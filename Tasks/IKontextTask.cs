namespace Kontext.Tasks
{
    /// <summary>
    ///     An interface for a task descriptor
    /// </summary>
    public interface IKontextTask
    {
        /// <summary>
        ///     The display title for the task
        /// </summary>
        string Title { get; }

        /// <summary>
        ///     A link to the task
        /// </summary>
        string Link { get; }
    }
}