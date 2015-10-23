namespace Kontext.Tasks
{
    public class KontextTfsTask : IKontextTask
    {
        #region Public Properties

        public string Title { get; private set; }
        public string Link { get; private set; }

        #endregion

        #region Constructor

        public KontextTfsTask(string title, string link)
        {
            Title = title;
            Link = link;
        }

        #endregion
    }
}