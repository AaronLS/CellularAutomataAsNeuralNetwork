using System;
using System.Collections.Generic;
using System.Text;

namespace CellLifeGame1.Model
{
    [Serializable]
    public class Position : IComparable
    {
        //position on an X/Y plane
        public int x;
        public int y;

        public Position()
        { }

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        #region	IComparable	Members

        ///   <summary>
        ///   Performs lexicographical comparison of a position, with
        ///   Position.x being the primary comparison, and Position.y
        ///   the secondary comparison.
        ///   </summary>
        ///	<param name="obj"><c>Position</c> to compare this instance to.
        ///	</param>
        ///	<returns>Returns less than 0 if  this instance is less than
        ///	<c>obj</c>, greater than 0 for greater than, and 0 for equality.
        ///	</returns>
        public int CompareTo(object obj)
        {
            if (obj is Position)
            {
                Position position = (Position)obj;
                int xResult = this.x.CompareTo(position.x);
                if (xResult != 0)
                {
                    return xResult;
                }
                //else x is	equal, thus	we use y comparison
                return this.y.CompareTo(position.y);
            }
            throw new ArgumentException("object	is not a Position");
        }

        #endregion
    }
}
