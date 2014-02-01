//Begin file Modeling.cs
using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using CellLifeGame1.Model;


namespace CellLifeGame1
{

    

	///	<summary>
	///	Maintains a	representation of the game modeled in internal data.
	///	
	///	A Modeling object maintains the data model through its own thread.
	///	
	///	Requests are made from client objects, which are queued, and
	///	the Modeling object will process those requests in a loop.
	///	
	///	When the modeling thread is TimeStepping the data model, any
	///	changes made to the model are queued to an updates object, which
	///	the client can access and consume.
	///		
	///	When this thread initially starts, it will load	an initial default
	///	model, and then	wait for some sort of signal
	///	from the UIForm	which will prompt ModelThread to load a	model by
	///	some means,	from a file, etc., or respond to user editing of the
	///	displayed UI model by updating the internal	model.	At some	point
	///	the	user may signal	the	model to begin function	of time
	///	computations.
	///	</summary>
	public class Modeling
	{
		///	<summary>
		///	A queue	of updates available for the UI	to consume.
		///	Each item in the queue is itself a collection listing nodes	or
		///	properties of the model	that have changed during an iteration of
		///	modeling computation.
		///	
      ///   The client must take care not to modify objects in the Q, as they
		///	are references to actual objects in the data model.	
		///	</summary>
		public Queue updates;

		/// <summary>
		/// Minimum time to complete a processing loop, in milliseconds
		/// </summary>
		private int loopMin;

		private	Thread modelingThread;

		#region	Operational	State Management
		///	<summary>
		///	<c>OpStateFlag</c> defines the possible	states that the
		///	<c>ModelThread</c> can operate in.  Either <c>TimeStepping</c>
		///   when the model is being updated through each time step,
		///   Paused when the <c>ModelThread</c> is blocked awaiting commands,
		///   or <c>UpdatePaused</c> or <c>UpdateTimeStepping</c> when the
		///   model is processing a request to update the model recieved while,
		///   respectively, <c>Paused</c> or <c>TimeStepping</c>.</summary>
		[Flags()]
		public enum	OpStateFlag	
		{
         Paused = 1, TimeStepping = 2, Updating = 4, Starting = 8,
			Exiting = 16, UpdateTimeStepping = TimeStepping | Updating,
			UpdatePaused = Paused |	Updating
		};

		/// <summary>
		/// <c>opState</c> defines the current operational state.
		/// </summary>
		private OpStateFlag opState;
		//locking object since value types cannot	be locked.
		private object opStateLock;
		
		/// <summary>
		/// <c>OpState</c> indicates the current operational state that
		/// <c>Modeling</c> is in.
		/// </summary>
		public OpStateFlag OpState
		{
			get
			{
				lock(opStateLock)
				{
					return opState;
				}
			}
		}

		private void OpStateChange(OpStateFlag newOpState)
		{
			lock(opStateLock)
			{
				opState=newOpState;
			}
		}
        		
		private	Queue requests;

		/// <summary>
		/// <c>RequestTimeStepping</c> will set <c>Modeling</c> to change
		/// <c>OpState</c> at the next most convenient point.
		/// </summary>
		public void	RequestTimeStepping()
		{
			lock(requests)
			{
				requests.Enqueue(OpStateFlag.TimeStepping);
				Monitor.Pulse(requests);
			}
		}

		public void	RequestPause()
		{
			lock(requests)
			{
				requests.Enqueue(OpStateFlag.Paused);
			}
		}

		public void RequestExit()
		{
			lock(requests)
			{
				requests.Enqueue(OpStateFlag.Exiting);
			}
		}

		//sets activation state of node at position p
		public void RequestUpdate(Position p, bool a)
		{
			lock(requests)
			{
				requests.Enqueue(OpStateFlag.Updating);
				requests.Enqueue(p);
				requests.Enqueue(a);
				Monitor.Pulse(requests);
			}
			Console.WriteLine("after pulse");

		}

		private void ProcessUpdate()
		{
			Position p;
			if(requests.Peek() is Position)
			{
				p = (Position)requests.Dequeue();
			}
			else
			{
				Console.WriteLine("Error, not position");
				return;
			}

			bool a;
			if(! (requests.Peek() is bool) )
			{
				Console.WriteLine("Error, not bool");
				return;
			}
			else
			{
				a = (bool)requests.Dequeue();
			}

			int nodeIndex = nodes.BinarySearch( p );

			if(nodeIndex >=	0)//if found
			{
				Node node = (  (Node) nodes[nodeIndex]  );
				node.IsActive = a;
				updates.Enqueue(node);
				OnUpdate(EventArgs.Empty);//fire event
			}
			else
			{
				Console.WriteLine("\nError, node not found for RequestUpdate\n");
				return;
			}
		}

		private void ProcessRequests()
		{
			lock(requests)
			{
            //while OpState request has not been satisfied, it is possible
				//that while waiting in paused mode, we may be awoken to find
				//a new request or multiple requests may be made before 
				//ProcessRequests is called.

				while( requests.Count>0 )
				{
					if( requests.Peek() is OpStateFlag )
					{
						OpStateFlag request = (OpStateFlag)requests.Dequeue();
						switch (request)
						{
							case OpStateFlag.Paused:
								opState=OpStateFlag.Paused;

								//if there are still requests remaining, then
								//we should set state to pause, but continue
								//processing requests since there will likely
								//be requests for TimeStepping or Updating which
								//would normally cancel a pause.
								if( requests.Count == 0 )
								{
									Monitor.Wait(requests);
								}
								break;

							case OpStateFlag.Updating:
								opState=OpStateFlag.Updating | opState;
								ProcessUpdate();
								if(requests.Count==0)
								{
									requests.Enqueue( 
										(~OpStateFlag.Updating) & opState );
									break;
								}
								else
								{
									opState = ( (~OpStateFlag.Updating) & opState );
								}
								break;
							
							case OpStateFlag.TimeStepping:
								opState=OpStateFlag.TimeStepping;
								break;

							case OpStateFlag.Exiting:
								opState=OpStateFlag.Exiting;
								break;
							
							default:
								Console.WriteLine
									("Error, default reached in ProcessRequest");
								break;
						}
					}
					//add else if statement for other object types in queue
				}
			}
		}
		#endregion

		///	<summary>
		///   The model data as a collection of <c>Node</c> objects.
		///	</summary>
		private ArrayList nodes;

		/// <summary>
		/// List of values on which nodes should fire.
		/// </summary>
		private ArrayList activationValues;

		public ArrayList ActivationValues
		{
			get
			{
				return activationValues;
			}
		}
	
		#region Update event fields and methods.    
		///	<summary>
		///   <c>Update</c> is fired when new data has been flipped to public.
		///	</summary>
		public event EventHandler Update;

		protected virtual void OnUpdate(EventArgs e)
		{
            if (Update != null)
				Update(this,e);
		}
		#endregion

		///	<summary>
		///	Loads square grid model	into <c>nodes</c> collection.
		///	</summary>
		///	

		///	<summary>
		///   Loads a grid of nodes and builds links between adjacent and 
		///   diagonally adjacent nodes(the nearest 8).  Most applicable for 
		///   John Conway's "Game of Life."
		///	</summary>
		///	<param name="initPosition">Minimum (x,y) origin of grid 
		///	construction</param>
		///	<param name="width">Width of grid in x direction.</param>
		///   <param name="height">Height   of grid  in y direction</param>
		///	<param name="initialActivity">IsActive state to initialize each
		///	node at.</param>
		private	void LoadNodesGrid(Position initPosition, 
			int width, int height, bool initialActivity)
		{
			for( int i = initPosition.x; i<width ;	++i)
			{
				for( int j = initPosition.y; j<height ; ++j )
				{
					nodes.Add(new Node(i,j));
				}
			}

			nodes.Sort();//A lexigraphical position	based ordering
			//This call	is not necesary	here in	this special case because
			//the above	for	loops ensure that the ArrayList	is
			//already sorted in	this way, but is done here to show
			//that one should sort before doing	a BinarySearch below

			//get a list of the surounding nodes and link
			foreach	(Node node in nodes)
			{
				for( int i = -1	; i<=1 ; ++i )
				{
					for( int j = -1	; j<=1 ; ++j )
					{
						if( (i != 0) || (j != 0))
						{
							Position position = new Position();
							if( i + node.position.x == width)
							{
								position.x = 0;
							}
							else if( i + node.position.x == -1 )
							{
								position.x = width-1;
							}
							else
							{
								position.x = i + node.position.x;
							}

							if( j + node.position.y == height)
							{
								position.y = 0;
							}
							else if( j + node.position.y == -1 )
							{
								position.y = height-1;
							}
							else
							{
								position.y = j + node.position.y;
							}

							int adjacentNodeIndex = nodes.BinarySearch( position );

							if(adjacentNodeIndex >=	0)//if found
							{
								node.AddLink( new Link(
									(Node) nodes[adjacentNodeIndex],	.12D ) );
							}
							else
							{
								Console.WriteLine("Error, adjacent node not found");
							}
						}
					}
				}
				node.AddLink( new Link((Node) node, .04D) );
				node.IsActive = initialActivity;//set initial state
			}


#if DEBUG
			foreach (Node node in nodes)
			{
				foreach (Link link in node.links)
				{
					Console.WriteLine("Node at ({0},{1}) linked to ({2},{3})",
						node.position.x, node.position.y,
						link.Destin.position.x,	link.Destin.position.y);
				}
			}
#endif

		}

		///	<summary>
		///	Loads a (0,0) origin grid.
		///	</summary>
		///	<param name="width">Width of grid in x direction.</param>
		///	<param name="height">Height	of grid	in y direction</param>
		///	<param name="initialActivity">IsActive state to	initialize each
		///	node at.</param>
		private void LoadNodesGrid(int width, int height, bool initialActivity)
		{
			LoadNodesGrid(new Position(0,0), width, height, initialActivity);
		}

		///	<summary>
		///	Loads a (0,0) origin grid with random activities for cells.
		///	</summary>
		///	<param name="width">Width of grid in x direction.</param>
		///	<param name="height">Height of grid	in y direction</param>
		private	void LoadNodesGrid(int width, int height)
		{
			LoadNodesGrid(new Position(0,0), width, height, true);
		}

		//requests a new NN to simulate a CA
		public void RequestAutomaton(
			ArrayList neighborhood, ArrayList activationValues,
			double neighborWeight, double centerWeight)
		{
			this.activationValues = (ArrayList)activationValues.Clone();
			RedoLinks(neighborhood, neighborWeight, centerWeight);
		}

		/// <summary>
		/// Returns(via update Q) and sets active a list of nodes that
		/// link to the node at <c>pos</c>
		/// </summary> 
		public void RequestActivateNeighbors(Position pos)
		{
			int	nodeIndex =	nodes.BinarySearch(pos);

			if(nodeIndex >=	0)//if found
			{
				foreach(Node node in nodes)
				{
					foreach(Link link in node.links)
					{
						if( link.Destin.Equals((Node)nodes[nodeIndex]) )
						{
							node.IsActive = true;
							updates.Enqueue(node);
						}
					}
				}					

				OnUpdate(EventArgs.Empty);//fire event
			}
			else
			{
				Console.WriteLine("\nError, node not found!\n");
			} 
		}

		public void RedoLinks(ArrayList neighborhood, double neighborWeight,
			double centerWeight)
		{
			foreach(Node node in nodes)
			{
				node.links = new ArrayList();

				if(centerWeight != 0.0D)
				{
					node.AddLink( new Link((Node) node, centerWeight) );
				}

				foreach(Position pos in neighborhood)
				{
					int width = 1 + ( (Node)(nodes[nodes.Count-1]) ).position.x;
					int height = 1 + ( (Node)(nodes[nodes.Count-1]) ).position.y;

					int relX = (node.position.x - pos.x)%width;
					int relY = (node.position.y - pos.y)%height;

					if(relX < 0)
					{
						relX += width;
					}

					if(relY < 0)
					{
						relY += height;
					}

                    int	adjacentNodeIndex =	nodes.BinarySearch(
						new Position(relX, relY)
						);

					if(adjacentNodeIndex >=	0)//if found
					{
						node.AddLink( new Link(
							(Node) nodes[adjacentNodeIndex],	neighborWeight ) );
					}
					else
					{
						Console.WriteLine("\nError, adjacent node not found!\n");
					}
				}
			}
		}


		private void CalculateWeightedSum()
		{
			foreach (Node node in nodes)
			{
				if(node.IsActive)
				{
					foreach (Link link in node.links)
					{
						link.Destin.inputSum += link.Weight;
					}
				}
			}
		}


		public void RequestLoopTime(int loopTime)
		{
			loopMin = loopTime;
		}

		private void CalculateActivation()
		{
			foreach (Node node in nodes)
			{

				bool deactivate = true;
				foreach(double dble in activationValues)
				{
					//if input sum == activationvalue, with tolerance 0.0001
					if(  Math.Abs(dble - node.inputSum) <= (0.0001D))
					{						
						//for efficiency we only change node state
						//if it is not already that state
						if( !node.IsActive )
						{
							node.IsActive = true;
							updates.Enqueue(node);
						}
						deactivate = false;
						break;
					}
				}

				//if we are active, and did not find activationValue in loop
				if(node.IsActive && deactivate)
				{//then deactivate
					node.IsActive = false;
					updates.Enqueue(node);
				}									   
				
            //reset input to 0 so that totals can begin accumulating again
				node.inputSum = 0;
			}

		}

		private void TimeStep()
		{
			this.CalculateWeightedSum();
			this.CalculateActivation();
		}

		public void ProcessSave(String filename)
		{
			using(  FileStream fileStream = new FileStream(filename,
						FileMode.Create, FileAccess.Write, FileShare.None)  )
			{
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize( 
					fileStream, new SaveFile(nodes, activationValues) );
			}
		}

		public void ProcessLoad(String filename)
		{
			using(  FileStream fileStream = new FileStream(filename,
						FileMode.Open, FileAccess.Read, FileShare.Read)  )
			{
				IFormatter formatter = new BinaryFormatter();

				Object loaded = formatter.Deserialize(fileStream);
				if(loaded is ArrayList)
				{
					nodes = (ArrayList)loaded;
				}
				else
				{
					SaveFile loadedDual = (SaveFile)loaded;
					nodes = loadedDual.nodes;
					activationValues = loadedDual.activationValues;
				}
			}

			foreach(Node node in nodes)
			{
				updates.Enqueue(node);
			}
			OnUpdate(EventArgs.Empty);
		}

		public void RequestSteps(int steps)
		{
			this.RequestPause();
			//wait for pause
			while(OpState != OpStateFlag.Paused){}

			bool updatesCleared = false;
			for(int i = 0; i < steps; ++i)
			{
				TimeStep();
			
				//to prevent emory being overused, we clear updates
				if(updates.Count > 2*nodes.Count)
				{
               updatesCleared = true;
					updates.Clear();
				}
			}
            
			//if we cleared updates, then we need to add all cells to updates
			if(updatesCleared)
			{
				updates.Clear();
				foreach(Node node in nodes)
				{
					updates.Enqueue(node);
				}
			}

			OnUpdate(EventArgs.Empty);//fire event
		}

		public Modeling():this(40,40)
		{}

		public Modeling(int height, int width)
		{
			requests = new Queue();
			opStateLock = new object();
			nodes = new ArrayList();
			updates = new Queue();
			loopMin = 100;
			
			LoadNodesGrid(height,width);
	
			System.Random randGen = 
				new Random(	unchecked((int)System.DateTime.Now.Ticks) );

			foreach (Node node in nodes)
			{
				//random activity
				node.IsActive = Convert.ToBoolean(randGen.Next(2));
			}

			//activation values corresponding to Conway's Life 
			this.activationValues = 
				new ArrayList( new double[3] {0.28D, 0.36D, 0.40D} );

			foreach (Node node in nodes)
			{
				updates.Enqueue(node);
			}

			modelingThread = new Thread(
				new ThreadStart(this.ModelingThreadEntryPoint)
				);

			modelingThread.Name	= "modelingThread";

			opState = OpStateFlag.Starting;
			RequestPause();

			modelingThread.Start();
		}

		public void	ModelingThreadEntryPoint()
		{			
			DateTime timed = DateTime.Now;
			DateTime timed2 = DateTime.Now;
			OnUpdate(EventArgs.Empty);

			while(opState != OpStateFlag.Exiting)
			{
				Console.Write("while loop took: ");
				timed2 = DateTime.Now;
				Console.WriteLine("{0}",timed2-timed);
				timed = DateTime.Now;

				ProcessRequests();
#if DEBUG
				timed2 = DateTime.Now;
				Console.WriteLine("{0}",timed2-timed);
				timed = DateTime.Now;
				Console.WriteLine("Entering TimeStep");
#endif
				TimeStep();

#if DEBUG

				timed2 = DateTime.Now;
				Console.WriteLine("{0}",timed2-timed);
				timed = DateTime.Now;

				Console.WriteLine("Entering OnUpdate");
#endif
				OnUpdate(EventArgs.Empty);//fire event

#if DEBUG
				timed2 = DateTime.Now;
				Console.WriteLine("{0}",timed2-timed);
				timed = DateTime.Now;
				Console.WriteLine("Sleeping");
#endif
				//loopMin milliseconds later
				timed2 = timed.AddMilliseconds(loopMin);

				//only if less than loopMin milliseconds have passed do we sleep
				if(DateTime.Now<=timed2)
				{
					Thread.Sleep(timed2-DateTime.Now);
				}

#if DEBUG
				timed2 = DateTime.Now;
				Console.WriteLine("{0}",timed2-timed);
				timed = DateTime.Now;
#endif
				

			}
		}

		public Node GetNode(int index)
		{
			return (Node)nodes[index];
		
		}

	}//class
}//namespace
//End file Modeling.cs
