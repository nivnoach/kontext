// Decompiled with JetBrains decompiler
// Type: Kontext.Kontext
// Assembly: Kontext, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A8E5B05C-B7A7-438A-88F0-1E017A5EC409
// Assembly location: C:\Users\Niv\Desktop\Kontext.exe

using System.Collections.Generic;
using Kontext.Items;

namespace Kontext
{
    /// <summary>
    ///     This class represents a single context
    /// </summary>
    public class Kontext
    {
        #region Public Members 

        public string Name { get; set; }

        public List<KontextItem> Items { get; private set; }

        #endregion

        #region Constructor

        public Kontext(string kontextName)
        {
            Name = kontextName;
            Items = new List<KontextItem>();
        }

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