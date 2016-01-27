using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auth0.Windows
{
    /// <summary>
    /// This class is a wrapper to use in WPF applications like this <code>new WindowWrapper(new WindowInteropHelper(this).Handle)</code>
    /// </summary>
    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        public WindowWrapper(IntPtr handle)
        {
            _hwnd = handle;
        }

        public IntPtr Handle
        {
            get { return _hwnd; }
        }

        private IntPtr _hwnd;
    }
}
