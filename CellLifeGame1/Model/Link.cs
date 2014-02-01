using System;
using System.Collections.Generic;
using System.Text;

namespace CellLifeGame1.Model
{

    [Serializable]
    public class Link
    {
        public Node Destin;//Destination, where	the	link points	to

        //an optional weight for use in applications such as neural nets
        private double weight;

        public double Weight
        {
            get
            {
                return this.weight;
            }
            set
            {
                this.weight = value;
            }
        }

        public Link(Node node, double initWeight)
        {
            this.Weight = initWeight;
            this.Destin = node;
        }

        public Link(Node node) : this(node, 1) { }

    }
}
