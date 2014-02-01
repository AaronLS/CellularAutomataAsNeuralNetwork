using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CellLifeGame1
{
    [Serializable]
    class SaveFile
    {
        internal ArrayList nodes;
        internal ArrayList activationValues;

        public SaveFile(ArrayList nodes, ArrayList activationValues)
        {
            this.nodes = nodes;
            this.activationValues = activationValues;
        }

    }
}
