// Decompiled with JetBrains decompiler
// Type: Kontext.KontextItems.VisualWindow
// Assembly: Kontext, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A8E5B05C-B7A7-438A-88F0-1E017A5EC409
// Assembly location: C:\Users\Niv\Desktop\Kontext.exe

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Kontext.WindowKontext;

namespace Kontext.Items
{
    public class VisualWindow : KontextItem
    {
        private readonly IntPtr _hWnd;
        private readonly string _title;
        private WINDOWPLACEMENT _placement = new WINDOWPLACEMENT();
        public VisualWindow(IntPtr hWnd, string title)
        {
            _hWnd = hWnd;
            _title = title;
            GetWindowPlacement(_hWnd, ref _placement);
        }

        public override string Name
        {
            get { return _title; }
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        internal struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rectangle rcNormalPosition;
        }

        const int SW_HIDE = 0;
        const int SW_SHOWNORMAL = 1;
        const int SW_NORMAL = 1;
        const int SW_SHOWMINIMIZED = 2;
        const int SW_SHOWMAXIMIZED = 3;
        const int SW_MAXIMIZE = 3;
        const int SW_SHOWNOACTIVATE = 4;
        const int SW_SHOW = 5;
        const int SW_MINIMIZE = 6;
        const int SW_SHOWMINNOACTIVE = 7;
        const int SW_SHOWNA = 8;
        const int SW_RESTORE = 9;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        public override string ToString()
        {
            return _title;
        }

        public override void Hide(HideLevel level)
        {
            if (_hWnd.Equals(IntPtr.Zero))
                return;

            // save the current state of the window
            RefreshState();

            // 
            ShowWindowAsync(_hWnd, SW_SHOWMINIMIZED);
        }

        public override void Show(bool makeUpfront)
        {
            if (_hWnd.Equals(IntPtr.Zero))
                return;

            SetWindowPlacement(_hWnd, ref _placement);

            if (makeUpfront)
                SetForegroundWindow(_hWnd);
        }

        public override VisibilityLevel GetVisibilityLevel()
        {
            RefreshState();

            switch (_placement.showCmd)
            {
                case SW_HIDE:
                    return VisibilityLevel.Invisible;
                case SW_SHOWMINIMIZED:
                case SW_SHOWNOACTIVATE:
                case SW_MINIMIZE:
                case SW_SHOWMINNOACTIVE:
                case SW_SHOWNA:
                    return VisibilityLevel.Accessible;
                case SW_SHOWNORMAL:
                case SW_MAXIMIZE:
                case SW_SHOW:
                case SW_RESTORE:
                    return VisibilityLevel.Visible;
            }
            return VisibilityLevel.Invisible;
        }

        public override IntPtr GetHandle()
        {
            return _hWnd;
        }

        public override Icon GetIcon()
        {
            return IconProvider.GetAppIcon(GetHandle());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((VisualWindow) obj);
        }

        protected bool Equals(VisualWindow other)
        {
            return _hWnd.Equals(other._hWnd);
        }

        public override int GetHashCode()
        {
            return _hWnd.GetHashCode();
        }

        public override void RefreshState()
        {
            // var invisibleWindowsStates = new List<int> {SW_MINIMIZE, SW_HIDE, SW_SHOWMINIMIZED, SW_SHOWNA};

            // Get the window's placement
            GetWindowPlacement(_hWnd, ref _placement);
       }
    }
}