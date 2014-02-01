using System;

namespace Recording
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public interface IRecordable
	{
		
	}

	public class Record
	{
		public Record()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <summary>
		/// Winds the record time forwards by timespan(or backwards if timespan is negative).
		/// </summary>
		/// <param name="timeSpan"></param>
		/// <returns></returns>
		public object WindForwards( TimeSpan timeSpan);

		/// <summary>
		/// Winds the record time backwards by timespan(or forwards if timespan is positive).
		/// </summary>
		/// <param name="timeSpan">Example: </param>
		/// <returns></returns>
		public object WindBackwards( TimeSpan timeSpan);

		public object WindForwards( TimeSpan timeSpan, object external);

		public object WindBackwards( TimeSpan timeSpan, object external);

		public object GetState( object external );//RETURN the state of the internal copy that correlates with the external object
		
		public object Do();//undoes the most recent undo

		public object Undo();//undoes the most recently performed action(s), performed either by a call to any of the Do methods or an action performed on an object by the client

		public object Do( object external );//steps an internal object forward by one action, returns a copy of internal

		public object Undo( object external );//steps an internal object backwards by one action, returns a copy of internal

//Todo: add do and undo that operate on a collection of items
		
		public object DoAll();//steps all internal objects by one action
		//Todo: Return should be void or maybe some type of colletion

		public object UndoAll();//steps all internal objects backwards by one action
		//Todo: Return should be void or maybe some type of collection

		//+= operator
		//GetState and add object "+=" could be implemented with the indexer operator if appropriate.

		
		
	}
}


/*
 * 
 * Usability Goals:
 *1.Allow the client to declare an object that will maintain a record of the changes of the state of a set of objects:
 *		Record aRecord = new Record( ***TBD*** );
 * 
 *2.Allow the client to add objects to the record and have their state tracked from the point they were added forward until a point where they are removed:
 *		objectType anObject = new objectType();
 *		myRecord += anObject;//add object to Record to be recorded, a snapshot of it's current state is copied into the record and the object referenced is marked to be watched for changes
 *		anObject.DoSomething(someParams);//this call will be added to the entry in myRecord for the object referenced by anObject
 *		Implementation Issues:
 *			a. A member call that involves outparams may be difficult to record.
 *		
 *3.Allow recording to be stopped for an object:
 *		myRecord -= anObject;
 * 
 *4.Allow the client to go back to a point in a records timeline by rewinding, and later set the record at current time again.
 * 
 *		Design Issues:
 *			a. Two options:
 *				I. Set the state of the client's objects to the past state.
 *					Pro:Memory saved because we don't keep a copy of the objects in the record.
 *					Con:Any instability or bug in our class or the client's implementation of IRecordable could corrupt the client object's state.
 *				II.Set the state of a copy of the object to the past state and allow the client access to the past objects.
 *					Pro:Avoid corruption of client data.
 *					Con:The client object's state is not updated which the client may desire so that they can rewind the state of their application.
 *			The second option better because a client can use a Record for not-critical purposes and not worry about data corruption.  It also allows them to only stub out IRecordable implementations and put off full implementation if their application is not dependent on the Record, thus allowing easier incremental development.  The con of the second option may be avoidable, and will be addressed if a viable implementation is found.  The phrases internal object(s) and external object(s) refer respectively to the copy object internal to the Record and the external object of the client.
 * 
 *			
 *			
 *		
 *
 *
 *						
 * 
 * 
 * 
 * */