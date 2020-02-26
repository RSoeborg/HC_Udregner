using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HC_Lib.Win
{
    public interface IWindow
    {
        string Title { get; }
        void Hide();
        void Show();
    }
}
