using System;
using System.Collections;


namespace ErsatzAcumen
{

	

	/// <summary>
	/// Summary description for Model.
	/// </summary>
	public class Model
	{
		private System.Threading.Thread thread;
        
		private Queue updates;

		public object GetNextUpdate()
		{
			return updates.Dequeue();
		}

		private class Item
		{
			public UniqueID ID;
			
			

		}

		public Model()
		{
			Item item = new Item();
			//item
		}
			


		
			

			
			//
			// TODO: Add constructor logic here
			//
		
	}
}
