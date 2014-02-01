using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CellLifeGame1.Model
{
    [Serializable]
    public class Node : IComparable
    {

        public double inputSum;

        //on or	off, firing or not-firing
        private bool active;
        public bool IsActive
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
            }
        }

        //position of node on an X/Y plane
        public Position position;

        //list of nodes	to which this one connects
        public ArrayList links;

        ///	<summary>
        ///	Creates	a link from	<c>this</c>	to <c>node</c>.
        ///	</summary>
        ///	<param name="node">The <c>Node</c> to link to.</param>
        public void AddLink(Link link)
        {
            if (links == null)
            {
                links = new ArrayList();
            }

            links.Add(link);
        }

        public Node(int positionX, int positionY)
        {
            position = new Position();

            this.position.x = positionX;
            this.position.y = positionY;
            inputSum = 0;
        }

        #region	IComparable	Members
        ///	<summary>
        ///	Performs lexigraphical comparison based on <c>position</c>.
        ///	</summary>
        ///	<param name="obj"><c>Node</c> or <c>position</c>
        ///	to compare this instance to.</param>
        ///	<returns>Returns result	of <c>position</c> comparison.</returns>
        public int CompareTo(object obj)
        {

            if (obj is Node)
            {
                Node node = (Node)obj;
                return this.position.CompareTo(node.position);
            }
            else//let Position try to compare the type
            {
                try
                {
                    return this.position.CompareTo(obj);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException(e + "and object	is not a Node");
                }
            }
        }
        #endregion

    }
}
