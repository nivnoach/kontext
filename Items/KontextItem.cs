using System;
using System.Drawing;

namespace Kontext.Items
{
    /// <summary>
    ///     Represents a single kontext item
    /// </summary>
    public abstract class KontextItem
    {
        /// <summary>
        ///     Returns the name of this item
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     Hides the item from the screen
        /// </summary>
        /// <param name="level"></param>
        public abstract void Hide(HideLevel level);

        /// <summary>
        ///     Shows the item on the screen
        /// </summary>
        /// <param name="makeUpfront"></param>
        public abstract void Show(bool makeUpfront);

        /// <summary>
        ///     Allows the item to refresh its state, in case the item's state
        ///     have changed due to user interaction that was not intercepted by the item
        /// </summary>
        public virtual void RefreshState()
        {
            // Do nothing
        }

        public abstract VisibilityLevel GetVisibilityLevel();

        /// <summary>
        ///     Returns the window handle for this item
        /// </summary>
        /// <returns></returns>
        public abstract IntPtr GetHandle();

        /// <summary>
        ///     Returns the icon which represents this item
        /// </summary>
        /// <returns></returns>
        public abstract Icon GetIcon();
    }
}