using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Minesweeper
{
    internal class MinesweeperButton : Button
    {
        private int h;
        private int w;

        public int H { get { return h; } set { h = value; } }

        public int W { get { return w; } set { w = value; } }

    }
}
