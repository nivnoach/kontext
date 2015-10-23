namespace Kontext.Items
{
    public enum VisibilityLevel
    {
        // Item is visible
        Visible,

        // Item is not directly visible, but an action is required to make it visible
        Accessible,

        // Item is not visible and cannot be made visible
        Invisible
    }
}