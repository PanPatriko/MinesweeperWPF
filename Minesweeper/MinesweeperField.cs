using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    internal class MinesweeperField
    {
        private bool mine;
        private bool flag;
        private bool covered;
        private int minesAround;

        public MinesweeperField(bool mine, bool flag, bool covered)
        {
            this.mine = mine;
            this.flag = flag;
            this.covered = covered;
        }

        public bool Mine
        {
            get { return mine; }
            set { mine = value; }
        }

        public bool Flag
        {
            get { return flag; }

            set { flag = value; }
        }

        public bool Covered 
        { 
            get { return covered; } 
        
            set { covered = value; }
        }

        public int MinesAround 
        { 
            get { return minesAround; }
            
            set { minesAround = value; } 
        }
    }
}
