using System;
using System.Collections;


namespace ErsatzAcumen
{

/*
 * pseudo code:
 * 
 * public neuron collection
 * 
 * private queue of collections of updated nuerons, one collection per timestep
 * accessable through method for getting updates that prevents client from changing queue
 * 
 * time granularity
 * 
 * subscribe to each neuron's onneuronupdate event
 * 
 * onneuronupdatehandler will add senderobject to clientupdatedneurons collection
 * NNModel should wake if sleeping, or immedietely halt processing and make updates
 * determine if procesing should be backed out due to being made obselete by a user
 * update
 * 
 * 
 * 
 * history stack or deque of historycollections
 * each collection showing the pre-updated values of neurons that were
 * updated during a timestep, any user editing done between timesteps 
 * is stored as a collection between the appropriate timesteps
 * 
 * each history item is a shallow copy of the object at the beginning of the timestep
 * algorithm for adding item to history
 * 
 * 
 * class historycollection
 * {
 * timeframe
 * collection of historyitems
 * }
 * 
 * public delegate void HistoryAction(object obj);
 * class HistoryItem
 * {
 * object Do;//delegate
 * object DoParams;//collection or array of parameters, or null if none needed
 * object Undo;//if null, then use Do with UndoParams
 * object UndoParams;//if null, then use DoParams for Undo
 * }
 * 
 * objects that are historical have an event that is fired when an archivable state change
 * has occured, which passes through the event a HistoryItem that the object produces
 * the handler for the event can use the history item for stepping backwards in time
 * through the objects state and then forward again.  Undo/Do.
 * Any state defining fields should be private, and the methods/properties through which
 * they are accessed should 
 * Historyitems should be serilizable, but are not valid if the object they are for
 * no longer exists.  This shouldn't be a problem since delegates maintain a reference
 * to the underlying object
 * 
 * stepping back in history can only take you as far as when you first subscribed to the
 * state changed event
 * it is not practical to have a historyitem that indicates the creation or desctruction
 * of an object, as the object won't really be garbage collected as long as a history item
 * exists for it due to the fact that delegates maintain a reference to the object
 * 
 * collections will need historical overloads that subscribers can track their state with.
 * With many apps, you will have some collection holding objects, and when you no longer
 * need an object, then you will remove it from the collection, if that is the only
 * reference to the object, then it will get garbage collected(GCed).  Keeping a historyitem
 * for an object will prevent it from being GCed, or keeping a historyitem for that collection
 * in which a param of the history item is a reference to that object will also prevent the
 * object from being GCed.  For example, if the history item is to undo the removal of an
 * item from a collection, then the undo delegate will likely be a call to Add on the
 * collection, with the object as the param.
 * 
 *		
 * 
 * 
 
 
*/
 






}