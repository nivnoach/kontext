using System.Collections.Generic;
using Kontext.Items;

namespace Kontext
{
    /// <summary>
    ///     This class represents a single context
    /// </summary>
    public class Kontext
    {
        #region Constructor

        public Kontext(string kontextName)
        {
            Name = kontextName;
            Items = new List<KontextItem>();
        }

        #endregion

        #region Public Members 

        public string Name { get; set; }

        public List<KontextItem> Items { get; }

        #endregion

        #region Public Methods

        public void ShowAll()
        {
            Items.ForEach(item => item.Show(false));
        }

        public void HideAll()
        {
            Items.ForEach(item => item.Hide(HideLevel.Hidden));
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}