using System;

namespace ErsatzAcumen
{

	/*
	/// <summary>
	/// UniqueID was an idea to provide IDs that were unique.
	/// But the default hashcodes of references are sufficient
	/// for most applications.
	/// </summary>
	public class UniqueID
	{
		///	<summary>
		///	A list of IDs available	previously used	IDs
		///	This is	to avoid an	object ID value	from overflowing
		///	an integer when	many objects are created and destroyed,
		///	which would	result in new objects getting higher and
		///	higher ID values.
		///	</summary>
		private	static Queue recycledIDs;

		///	<summary>
		///	The	value of the next node ID to be	issued,	if
		///	there are none available in	<c>recycledIDs</c>.
		///	</summary>
		private	static int nextID=0;

		private	static int getNextID()
		{
			
			if(recycledIDs==null)
			{
				int	useID =	this.nextID;
				++this.nextID;
				return useID;
			}
			//else there are recylcedIDs to	be used
			return (int) recycledIDs.Dequeue();
		}
		
		private readonly int id;

		public int ID
		{
			get
			{
				return id;
			}
		}
        
		public UniqueID()
		{
			this.id = getNextID();
		}

		public override int GetHashCode() 
		{
			return ID;
		}
	}*/
}
