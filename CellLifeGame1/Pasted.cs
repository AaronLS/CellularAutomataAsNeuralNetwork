//Begin file Modeling.cs
using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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

   [Serializable]
   public class Position:IComparable
   {
      //position on an X/Y plane
      public int x;
      public int y;

      public Position()
      {}

      public Position(int  x, int y)
      {
         this.x = x;
         this.y = y;
      }

      #region  IComparable Members

      ///   <summary>
      ///   Performs lexicographical comparison of a position, with
      ///   Position.x being the primary comparison, and Position.y
      ///   the secondary comparison.
      ///   </summary>
      ///   <param name="obj"><c>Position</c> to compare this instance to.
      ///   </param>
      ///   <returns>Returns less than 0 if  this instance is less than
      ///   <c>obj</c>, greater than 0 for greater than, and 0 for equality.
      ///   </returns>
      public int CompareTo(object   obj)
      {
         if(obj is Position)  
         {
            Position position =  (Position) obj;
            int   xResult  = this.x.CompareTo(position.x);
            if(xResult!=0)
            {
               return xResult;
            }
            //else x is equal, thus we use y comparison
            return this.y.CompareTo(position.y);
         }
         throw new ArgumentException("object is not a Position");
      }

      #endregion
   }

   [Serializable]
   public class Link
   {
      public Node Destin;//Destination, where   the   link points to

      //an optional weight for use in applications such as neural nets
      private  double weight;

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

      public Link(Node node):this(node, 1)   {}

   }

   [Serializable]
   public class Node:IComparable
   {

      public double inputSum;
      
      //on or  off, firing or not-firing
      private  bool active;
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
      public Position   position;
            
      //list of nodes   to which this one connects
      public ArrayList links;
      
      ///   <summary>
      ///   Creates  a link from <c>this</c> to <c>node</c>.
      ///   </summary>
      ///   <param name="node">The <c>Node</c> to link to.</param>
      public void AddLink(Link link)
      {
         if(links == null)
         {
            links =  new   ArrayList();
         }

         links.Add(link);
      }
      
      public Node(int   positionX, int positionY)
      {
         position = new Position();

         this.position.x   = positionX;
         this.position.y   = positionY;
         inputSum = 0;
      }

      #region  IComparable Members
      ///   <summary>
      ///   Performs lexigraphical comparison based on <c>position</c>.
      ///   </summary>
      ///   <param name="obj"><c>Node</c> or <c>position</c>
      ///   to compare this instance to.</param>
      ///   <returns>Returns result of <c>position</c> comparison.</returns>
      public int CompareTo(object   obj)
      {        

         if(obj is Node)
         {
            Node node = (Node) obj;
            return this.position.CompareTo(node.position);
         }
         else//let Position try to compare the type
         {
            try
            {
               return this.position.CompareTo(obj);
            }
            catch(ArgumentException e)
            {
               throw new ArgumentException(e +  "and object is not a Node");
            }  
         }
      }
      #endregion

   }

   ///   <summary>
   ///   Maintains a representation of the game modeled in internal data.
   ///   
   ///   A Modeling object maintains the data model through its own thread.
   ///   
   ///   Requests are made from client objects, which are queued, and
   ///   the Modeling object will process those requests in a loop.
   ///   
   ///   When the modeling thread is TimeStepping the data model, any
   ///   changes made to the model are queued to an updates object, which
   ///   the client can access and consume.
   ///      
   ///   When this thread initially starts, it will load an initial default
   ///   model, and then   wait for some sort of signal
   ///   from the UIForm   which will prompt ModelThread to load a   model by
   ///   some means, from a file, etc., or respond to user editing of the
   ///   displayed UI model by updating the internal  model.   At some  point
   ///   the   user may signal   the   model to begin function of time
   ///   computations.
   ///   </summary>
   public class Modeling
   {
      ///   <summary>
      ///   A queue  of updates available for the UI  to consume.
      ///   Each item in the queue is itself a collection listing nodes or
      ///   properties of the model that have changed during an iteration of
      ///   modeling computation.
      ///   
      ///   The client must take care not to modify objects in the Q, as they
      ///   are references to actual objects in the data model.   
      ///   </summary>
      public Queue updates;

      /// <summary>
      /// Minimum time to complete a processing loop, in milliseconds
      /// </summary>
      private int loopMin;

      private  Thread modelingThread;

      #region  Operational State Management
      ///   <summary>
      ///   <c>OpStateFlag</c> defines the possible   states that the
      ///   <c>ModelThread</c> can operate in.  Either <c>TimeStepping</c>
      ///   when the model is being updated through each time step,
      ///   Paused when the <c>ModelThread</c> is blocked awaiting commands,
      ///   or <c>UpdatePaused</c> or <c>UpdateTimeStepping</c> when the
      ///   model is processing a request to update the model recieved while,
      ///   respectively, <c>Paused</c> or <c>TimeStepping</c>.</summary>
      [Flags()]
         public enum OpStateFlag 
      {
         Paused = 1, TimeStepping = 2, Updating = 4, Starting = 8,
         Exiting = 16, UpdateTimeStepping = TimeStepping | Updating,
         UpdatePaused = Paused | Updating
      };

      /// <summary>
      /// <c>opState</c> defines the current operational state.
      /// </summary>
      private OpStateFlag opState;
      //locking object since value types cannot be locked.
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
            
      private  Queue requests;

      /// <summary>
      /// <c>RequestTimeStepping</c> will set <c>Modeling</c> to change
      /// <c>OpState</c> at the next most convenient point.
      /// </summary>
      public void RequestTimeStepping()
      {
         lock(requests)
         {
            requests.Enqueue(OpStateFlag.TimeStepping);
            Monitor.Pulse(requests);
         }
      }

      public void RequestPause()
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

         if(nodeIndex >=   0)//if found
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

      ///   <summary>
      ///   The model data as a collection of <c>Node</c> objects.
      ///   </summary>
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
      ///   <summary>
      ///   <c>Update</c> is fired when new data has been flipped to public.
      ///   </summary>
      public event EventHandler Update;

      protected virtual void OnUpdate(EventArgs e)
      {
         if (Update != null)
            Update(this,e);
      }
      #endregion

      ///   <summary>
      ///   Loads square grid model into <c>nodes</c> collection.
      ///   </summary>
      ///   

      ///   <summary>
      ///   Loads a grid of nodes and builds links between adjacent and 
      ///   diagonally adjacent nodes(the nearest 8).  Most applicable for 
      ///   John Conway's "Game of Life."
      ///   </summary>
      ///   <param name="initPosition">Minimum (x,y) origin of grid 
      ///   construction</param>
      ///   <param name="width">Width of grid in x direction.</param>
      ///   <param name="height">Height   of grid  in y direction</param>
      ///   <param name="initialActivity">IsActive state to initialize each
      ///   node at.</param>
      private  void LoadNodesGrid(Position initPosition, 
         int width, int height, bool initialActivity)
      {
         for( int i = initPosition.x; i<width ; ++i)
         {
            for( int j = initPosition.y; j<height ; ++j )
            {
               nodes.Add(new Node(i,j));
            }
         }

         nodes.Sort();//A lexigraphical position   based ordering
         //This call is not necesary   here in  this special case because
         //the above for   loops ensure that the ArrayList  is
         //already sorted in  this way, but is done here to show
         //that one should sort before doing a BinarySearch below

         //get a list of the surounding nodes and link
         foreach  (Node node in nodes)
         {
            for( int i = -1   ; i<=1 ; ++i )
            {
               for( int j = -1   ; j<=1 ; ++j )
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

                     if(adjacentNodeIndex >= 0)//if found
                     {
                        node.AddLink( new Link(
                           (Node) nodes[adjacentNodeIndex], .12D ) );
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
                  link.Destin.position.x, link.Destin.position.y);
            }
         }
#endif

      }

      ///   <summary>
      ///   Loads a (0,0) origin grid.
      ///   </summary>
      ///   <param name="width">Width of grid in x direction.</param>
      ///   <param name="height">Height   of grid  in y direction</param>
      ///   <param name="initialActivity">IsActive state to initialize each
      ///   node at.</param>
      private void LoadNodesGrid(int width, int height, bool initialActivity)
      {
         LoadNodesGrid(new Position(0,0), width, height, initialActivity);
      }

      ///   <summary>
      ///   Loads a (0,0) origin grid with random activities for cells.
      ///   </summary>
      ///   <param name="width">Width of grid in x direction.</param>
      ///   <param name="height">Height of grid in y direction</param>
      private  void LoadNodesGrid(int width, int height)
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
         int   nodeIndex = nodes.BinarySearch(pos);

         if(nodeIndex >=   0)//if found
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

               int   adjacentNodeIndex =  nodes.BinarySearch(
                  new Position(relX, relY)
                  );

               if(adjacentNodeIndex >= 0)//if found
               {
                  node.AddLink( new Link(
                     (Node) nodes[adjacentNodeIndex], neighborWeight ) );
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
            new Random( unchecked((int)System.DateTime.Now.Ticks) );

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

         modelingThread.Name  = "modelingThread";

         opState = OpStateFlag.Starting;
         RequestPause();

         modelingThread.Start();
      }

      public void ModelingThreadEntryPoint()
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

//Begin file UIForm.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;


namespace CellLifeGame1
{

   /// <summary>
   /// Summary description for Form1.
   /// </summary>
   public class UIForm : System.Windows.Forms.Form
   {
      public class RectGrid : System.Windows.Forms.Panel
      {
         private void InitializeComponent()
         {}

         protected override void OnPaintBackground(PaintEventArgs pevent)
         {
            //disable background painting by not calling base
         }
      }

      /// <summary>
      /// Programmer Added Declarations
      /// </summary>
      private Modeling modeling;//computation and data model

      /// <summary>
      /// Form Designer Generated Declarations
      /// </summary>
      private RectGrid panel1;
      private StatesOutput crntStatesOutput;

      private Cell[,] cells;
      private System.Windows.Forms.Button StartStop;

      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.Container components = null;
      
      private int cellSize=8;
      private int cellsWide=64;
      private System.Windows.Forms.Button btnClear;
      private System.Windows.Forms.Button btnSave;
      private System.Windows.Forms.Button btnLoad;
      private System.Windows.Forms.Button btnDone;
      private System.Windows.Forms.Button automaton;
      private System.Windows.Forms.TrackBar sldrSpeed;
      private System.Windows.Forms.NumericUpDown numSteps;
      private System.Windows.Forms.Button btnStep;
      private System.Windows.Forms.Button btnCurrentAuto;
      private System.Windows.Forms.GroupBox groupAutomaton;
      private int cellsHigh=64;

      public UIForm()
      {
         //
         // Required for Windows Form Designer support
         //
         InitializeComponent();
         
         ProgrammaticInit();
      }

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      protected override void Dispose( bool disposing )
      {
         if( disposing )
         {
            if (components != null) 
            {
               components.Dispose();
            }
         }
         base.Dispose( disposing );
      }

      private void ProgrammaticInit()
      {
         SetStyle(ControlStyles.UserPaint, true); 
         SetStyle(ControlStyles.AllPaintingInWmPaint, true); 
         SetStyle(ControlStyles.DoubleBuffer, true); 

         crntStatesOutput = new StatesOutput();
         
         cells= new Cell[cellsWide, cellsHigh];
            
         for(int i=0;i<cellsWide;++i)
         {
            for(int j=0;j<cellsHigh;++j)
            {
               cells[i,j] = new Cell();
            }
         }

         //Create thread that maintains the model of the game.
         modeling = new Modeling(cellsWide,cellsHigh);

         //event hookup
         modeling.Update += new EventHandler(modeling_Updated);

      }


      private void modeling_Updated(object sender, EventArgs e)
      {
         Console.WriteLine("{0} updates.",modeling.updates.Count);

         while(modeling.updates.Count>0)
         {
            Node node = (Node)modeling.updates.Dequeue();

            Cell cell = cells[node.position.x, node.position.y];

            if(node.IsActive)
            {
               cell.Color = Color.Crimson;
            }
            else
            {
               cell.Color = Color.DarkBlue;
            }

            cell.X = node.position.x*cellSize;
            cell.Y = node.position.y*cellSize;
            cell.Size = cellSize;   
         }

         if(this.StartStop.Text == "Stop")
         {
            modeling.RequestPause();
         }
         this.panel1.Invalidate();
      }
      

      #region Windows Form Designer generated code
      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.panel1 = new CellLifeGame1.UIForm.RectGrid();
         this.StartStop = new System.Windows.Forms.Button();
         this.btnClear = new System.Windows.Forms.Button();
         this.btnSave = new System.Windows.Forms.Button();
         this.btnLoad = new System.Windows.Forms.Button();
         this.automaton = new System.Windows.Forms.Button();
         this.btnDone = new System.Windows.Forms.Button();
         this.sldrSpeed = new System.Windows.Forms.TrackBar();
         this.numSteps = new System.Windows.Forms.NumericUpDown();
         this.btnStep = new System.Windows.Forms.Button();
         this.btnCurrentAuto = new System.Windows.Forms.Button();
         this.groupAutomaton = new System.Windows.Forms.GroupBox();
         ((System.ComponentModel.ISupportInitialize)
            (this.sldrSpeed)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)
            (this.numSteps)).BeginInit();
         this.groupAutomaton.SuspendLayout();
         this.SuspendLayout();
         // 
         // panel1
         // 
         this.panel1.Location = new System.Drawing.Point(56, 40);
         this.panel1.Name = "panel1";
         this.panel1.Size = new System.Drawing.Size(512, 512);
         this.panel1.TabIndex = 0;
         this.panel1.MouseUp += new 
            System.Windows.Forms.MouseEventHandler(this.panel1_MouseUp);
         this.panel1.Paint += new 
            System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
         this.panel1.DragLeave += new
            System.EventHandler(this.panel1_DragLeave);
         this.panel1.MouseMove += new
            System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);
         this.panel1.MouseDown += new 
            System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
         // 
         // StartStop
         // 
         this.StartStop.Location = new System.Drawing.Point(72, 8);
         this.StartStop.Name = "StartStop";
         this.StartStop.Size = new System.Drawing.Size(40, 24);
         this.StartStop.TabIndex = 2;
         this.StartStop.Text = "Start";
         this.StartStop.Click += new
            System.EventHandler(this.StartStop_Click);
         // 
         // btnClear
         // 
         this.btnClear.Location = new System.Drawing.Point(256, 8);
         this.btnClear.Name = "btnClear";
         this.btnClear.Size = new System.Drawing.Size(40, 24);
         this.btnClear.TabIndex = 3;
         this.btnClear.Text = "Clear";
         this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
         // 
         // btnSave
         // 
         this.btnSave.Location = new System.Drawing.Point(304, 8);
         this.btnSave.Name = "btnSave";
         this.btnSave.Size = new System.Drawing.Size(40, 24);
         this.btnSave.TabIndex = 4;
         this.btnSave.Text = "Save";
         this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
         // 
         // btnLoad
         // 
         this.btnLoad.Location = new System.Drawing.Point(352, 8);
         this.btnLoad.Name = "btnLoad";
         this.btnLoad.Size = new System.Drawing.Size(40, 24);
         this.btnLoad.TabIndex = 5;
         this.btnLoad.Text = "Load";
         this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
         // 
         // automaton
         // 
         this.automaton.Location = new System.Drawing.Point(4, 16);
         this.automaton.Name = "automaton";
         this.automaton.Size = new System.Drawing.Size(36, 20);
         this.automaton.TabIndex = 6;
         this.automaton.Text = "New";
         this.automaton.Click += new
            System.EventHandler(this.automaton_Click);
         // 
         // btnDone
         // 
         this.btnDone.Enabled = false;
         this.btnDone.Location = new System.Drawing.Point(108, 16);
         this.btnDone.Name = "btnDone";
         this.btnDone.Size = new System.Drawing.Size(40, 20);
         this.btnDone.TabIndex = 7;
         this.btnDone.Text = "Done";
         this.btnDone.Click += new
            System.EventHandler(this.btnDone_Click);
         // 
         // sldrSpeed
         // 
         this.sldrSpeed.Location = new System.Drawing.Point(8, 8);
         this.sldrSpeed.Maximum = 1000;
         this.sldrSpeed.Minimum = 10;
         this.sldrSpeed.Name = "sldrSpeed";
         this.sldrSpeed.Orientation = 
            System.Windows.Forms.Orientation.Vertical;
         this.sldrSpeed.Size = new System.Drawing.Size(45, 544);
         this.sldrSpeed.TabIndex = 9;
         this.sldrSpeed.TickStyle = System.Windows.Forms.TickStyle.None;
         this.sldrSpeed.Value = 200;
         this.sldrSpeed.ValueChanged += new
            System.EventHandler(this.sldSpeed_Changed);
         // 
         // numSteps
         // 
         this.numSteps.Location = new System.Drawing.Point(168, 8);
         this.numSteps.Maximum = new System.Decimal(new int[] {
                                                                 5000,
                                                                 0,
                                                                 0,
                                                                 0});
         this.numSteps.Minimum = new System.Decimal(new int[] {
                                                                 1,
                                                                 0,
                                                                 0,
                                                                 0});
         this.numSteps.Name = "numSteps";
         this.numSteps.Size = new System.Drawing.Size(80, 20);
         this.numSteps.TabIndex = 10;
         this.numSteps.Value = new System.Decimal(new int[] {
                                                               1,
                                                               0,
                                                               0,
                                                               0});
         // 
         // btnStep
         // 
         this.btnStep.Location = new System.Drawing.Point(120, 8);
         this.btnStep.Name = "btnStep";
         this.btnStep.Size = new System.Drawing.Size(40, 24);
         this.btnStep.TabIndex = 8;
         this.btnStep.Text = "Step";
         this.btnStep.Click += new System.EventHandler(this.btnStep_Click);
         // 
         // btnCurrentAuto
         // 
         this.btnCurrentAuto.Location = new System.Drawing.Point(48, 16);
         this.btnCurrentAuto.Name = "btnCurrentAuto";
         this.btnCurrentAuto.Size = new System.Drawing.Size(52, 20);
         this.btnCurrentAuto.TabIndex = 11;
         this.btnCurrentAuto.Text = "Current";
         this.btnCurrentAuto.Click += new
            System.EventHandler(this.btnCurrentAuto_Click);
         // 
         // groupAutomaton
         // 
         this.groupAutomaton.Controls.Add(this.btnDone);
         this.groupAutomaton.Controls.Add(this.btnCurrentAuto);
         this.groupAutomaton.Controls.Add(this.automaton);
         this.groupAutomaton.Location = new System.Drawing.Point(400, 0);
         this.groupAutomaton.Name = "groupAutomaton";
         this.groupAutomaton.Size = new System.Drawing.Size(152, 40);
         this.groupAutomaton.TabIndex = 13;
         this.groupAutomaton.TabStop = false;
         this.groupAutomaton.Text = "Automaton";
         // 
         // UIForm
         // 
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.ClientSize = new System.Drawing.Size(576, 557);
         this.Controls.Add(this.groupAutomaton);
         this.Controls.Add(this.numSteps);
         this.Controls.Add(this.sldrSpeed);
         this.Controls.Add(this.btnStep);
         this.Controls.Add(this.btnLoad);
         this.Controls.Add(this.btnSave);
         this.Controls.Add(this.btnClear);
         this.Controls.Add(this.StartStop);
         this.Controls.Add(this.panel1);
         this.FormBorderStyle = 
            System.Windows.Forms.FormBorderStyle.FixedToolWindow;
         this.Name = "UIForm";
         this.RightToLeft = System.Windows.Forms.RightToLeft.No;
         this.Text = 
            "Cellular Automata Computer Aided Design and Conversion (CACADAC)";
         ((System.ComponentModel.ISupportInitialize)
            (this.sldrSpeed)).EndInit();
         ((System.ComponentModel.ISupportInitialize)
            (this.numSteps)).EndInit();
         this.groupAutomaton.ResumeLayout(false);
         this.ResumeLayout(false);

      }
      #endregion

      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main() 
      {
         Application.Run(new UIForm());
         Application.ExitThread();//allow other threads to continue
      }

      private bool tempPause = false;

      private void panel1_MouseDown(
         object sender, System.Windows.Forms.MouseEventArgs e)
      {
         if(this.StartStop.Text == "Stop")
         {
            this.StartStop.Text = "Start";
            tempPause = true;
            modeling.RequestPause();            
         }

         if(e.Button == MouseButtons.Left)
         {  
            Position position = new Position(e.X/cellSize,e.Y/cellSize);
            modeling.RequestUpdate(position,true);
         }
         else if(e.Button == MouseButtons.Right)
         {  
            Position position = new Position(e.X/cellSize,e.Y/cellSize);
            modeling.RequestUpdate(position,false);
         }
      }

      private void panel1_MouseUp(
         object sender, System.Windows.Forms.MouseEventArgs e)
      {
         if(this.StartStop.Text == "Start" && tempPause==true)
         {
            this.StartStop.Text = "Stop";
            tempPause = false;
            modeling.RequestTimeStepping();
         }

         if(e.Button == MouseButtons.Left)
         {  
            Position position = new Position(e.X/cellSize,e.Y/cellSize);
            modeling.RequestUpdate(position,true);
         }
         else if(e.Button == MouseButtons.Right)
         {  
            Position position = new Position(e.X/cellSize,e.Y/cellSize);
            modeling.RequestUpdate(position,false);
         }
         
      }

      private void panel1_Paint(
         object sender, System.Windows.Forms.PaintEventArgs a)
      {
         for(int i=0;i<cellsWide;++i)
         {
            for(int j=0;j<cellsHigh;++j)
            {
               using( Brush brush = new SolidBrush(cells[i,j].Color) )
                  a.Graphics.FillRectangle(brush,
                     cells[i,j].X,// + panel1.Location.X,
                     cells[i,j].Y,// + panel1.Location.Y,
                     cells[i,j].Size,cells[i,j].Size);
            }
         }
         
         if(this.StartStop.Text == "Stop")
         {
            modeling.RequestTimeStepping();
         }
      }

      private void StartStop_Click(object sender, System.EventArgs e)
      {
         if(this.StartStop.Text == "Start")
         {
            modeling.RequestTimeStepping();        
            this.StartStop.Text = "Stop";          
         }
         else if(this.StartStop.Text == "Stop")
         {
            modeling.RequestPause();         
            this.StartStop.Text = "Start";            
         }
      }

      private void panel1_DragLeave(object sender, System.EventArgs e)
      {
         Console.WriteLine("dragleave occured");
      }

      private void panel1_MouseMove(
         object sender, System.Windows.Forms.MouseEventArgs e)
      {
         if(e.Button == MouseButtons.Left)
         {  
            Position position = new Position(e.X/cellSize,e.Y/cellSize);
            modeling.RequestUpdate(position,true);
         }
         else if(e.Button == MouseButtons.Right)
         {  
            Position position = new Position(e.X/cellSize,e.Y/cellSize);
            modeling.RequestUpdate(position,false);
         }
      }

      private void btnClear_Click(object sender, System.EventArgs e)
      {
         modeling.RequestPause();
         
         foreach(Cell cell in cells)
         {
            modeling.RequestUpdate(
               new Position(cell.X/cellSize,cell.Y/cellSize), false);
            //Console.WriteLine("Position({0},{1})",cell.X,cell.Y);
         }
      }

      private void btnSave_Click(object sender, System.EventArgs e)
      {
         modeling.RequestPause();
         
         SaveFileDialog save = new SaveFileDialog();
         save.Filter = cellsWide.ToString() + "X" + cellsHigh.ToString() + 
            "Nodes Grid|*.64x64Nodes";

         if(save.ShowDialog() == DialogResult.OK)
         {
            modeling.ProcessSave(save.FileName);
         }     
         this.StartStop.Text = "Start";
      }

      private void btnLoad_Click(object sender, System.EventArgs e)
      {
         modeling.RequestPause();

         OpenFileDialog open = new OpenFileDialog();
         open.Filter = cellsWide.ToString() + "X" + cellsHigh.ToString() +
            " Nodes Grid|*.64x64Nodes";

         if(open.ShowDialog() == DialogResult.OK)
         {
            modeling.ProcessLoad(open.FileName);
         }
         this.StartStop.Text = "Start";
      }

      private void automaton_Click(object sender, System.EventArgs e)
      {
         this.btnCurrentAuto.Enabled = false;
         this.StartStop.Enabled = false;
         this.StartStop.Text = "Start";
         this.automaton.Enabled = false; 
         this.btnSave.Enabled = false;
         this.btnLoad.Enabled = false;

         modeling.RequestPause();
         panel1.Paint +=new PaintEventHandler(panel1_PaintCenter);
         panel1.Invalidate();
         
         //this.btnClear_Click(sender,e); 
         foreach(Cell cell in cells)
         {
            modeling.RequestUpdate(
               new Position(cell.X/cellSize,cell.Y/cellSize), false);
         }

         modeling.RequestActivateNeighbors(
            new Position(
            cells[cellsWide/2,cellsHigh/2].X/cellSize,
            cells[cellsWide/2,cellsHigh/2].Y/cellSize
            )
            );

         this.btnDone.Enabled = true;
      }

      private void panel1_PaintCenter(object sender,
         System.Windows.Forms.PaintEventArgs a)
      {
         using( Brush brush = new SolidBrush(Color.White) )
            a.Graphics.FillRectangle(brush,
               cells[cellsWide/2,cellsHigh/2].X,
               cells[cellsWide/2,cellsHigh/2].Y,
               cells[cellsWide/2,cellsHigh/2].Size,
               cells[cellsWide/2,cellsHigh/2].Size);
      }

      private void btnDone_Click(object sender, System.EventArgs e)
      {
         this.StartStop.Enabled = true;
         this.btnDone.Enabled = false;
         this.btnSave.Enabled = true;
         this.btnLoad.Enabled = true;

         panel1.Paint -= new PaintEventHandler(panel1_PaintCenter);
         panel1.Invalidate();

         ArrayList neighborhood = new ArrayList();

         foreach(Cell cell in cells)
         {
            if( (cell.Color == Color.Crimson) &&
               !( (cell.X == cells[cellsWide/2,cellsHigh/2].X) &&
               (cell.Y == cells[cellsWide/2,cellsHigh/2].Y) )
               )
            {
               neighborhood.Add(new Position(
                  (cell.X/cellSize) - 
                  (cells[cellsWide/2,cellsHigh/2].X / cellSize),
                  (cell.Y/cellSize) - 
                  (cells[cellsWide/2,cellsHigh/2].Y / cellSize)
                  ));
            }
         }

         this.IgnoreClicks = true;

         crntStatesOutput = new StatesOutput(neighborhood.Count);
         crntStatesOutput.ShowDialog();

         if(crntStatesOutput.ActivationValues != null)
         {
            modeling.RequestAutomaton(
               neighborhood, crntStatesOutput.ActivationValues,
               crntStatesOutput.NeighborWeight,
               crntStatesOutput.CenterWeight    
               );
         }

         this.IgnoreClicks = false;
         this.StartStop.Enabled = true;
         this.btnDone.Enabled = false;
         this.btnCurrentAuto.Enabled = true;
         this.automaton.Enabled = true;
         this.Focus();
      }

      private bool ignoreClicks = false;

      private bool IgnoreClicks
      {
         get
         {
            return ignoreClicks;
         }
         set
         {
            if(value == true && ignoreClicks == false)
            {
               this.Enabled = false;
            }
            else if(value == false && ignoreClicks == true)
            {
               this.Enabled = true;
            }
            ignoreClicks = value;
         }
      }

      private void sldSpeed_Changed(object sender, System.EventArgs e)
      {
         modeling.RequestLoopTime(this.sldrSpeed.Value);
      }

      private void btnStep_Click(object sender, System.EventArgs e)
      {
         this.StartStop.Enabled = false;
         modeling.RequestPause();
         this.StartStop.Text = "Start";
         modeling.RequestSteps((int)numSteps.Value);
         this.StartStop.Enabled = true;
      }

      private void btnCurrentAuto_Click(object sender, System.EventArgs e)
      {
         this.StartStop.Enabled = false;
         this.StartStop.Text = "Start";

         modeling.RequestPause();
         panel1.Paint +=new PaintEventHandler(panel1_PaintCenter);
         panel1.Invalidate();

         foreach(Cell cell in cells)
         {
            modeling.RequestUpdate(
               new Position(cell.X/cellSize,cell.Y/cellSize), false);
         }

         modeling.RequestActivateNeighbors(
            new Position(
            cells[cellsWide/2,cellsHigh/2].X/cellSize,
            cells[cellsWide/2,cellsHigh/2].Y/cellSize
            )
            );

         ArrayList neighborhood = new ArrayList();

         foreach(Cell cell in cells)
         {
            if( (cell.Color == Color.Crimson) &&
               !( (cell.X == cells[cellsWide/2,cellsHigh/2].X) &&
               (cell.Y == cells[cellsWide/2,cellsHigh/2].Y) )
               )
            {
               neighborhood.Add(new Position(
                  (cell.X/cellSize)
                  - (cells[cellsWide/2,cellsHigh/2].X / cellSize),
                  (cell.Y/cellSize)
                  - (cells[cellsWide/2,cellsHigh/2].Y / cellSize)
                  ));
            }
         }

         this.IgnoreClicks = true;

         Node node = modeling.GetNode(0);

         double nodeCenterWeight = 0D;
         double nodeNWeight = 0D;
         foreach(Link link in node.links)
         {
            if(   link.Destin.Equals(node) )
            {
               nodeCenterWeight = link.Weight;
            }
            else
            {
               nodeNWeight = link.Weight;

            }
         }

         crntStatesOutput = new StatesOutput(
            neighborhood.Count, nodeNWeight, nodeCenterWeight,
            modeling.ActivationValues);

         crntStatesOutput.ShowDialog();

         if(crntStatesOutput.ActivationValues != null)
         {
            modeling.RequestAutomaton(
               neighborhood, crntStatesOutput.ActivationValues,
               crntStatesOutput.NeighborWeight,
               crntStatesOutput.CenterWeight    
               );
         }

         this.IgnoreClicks = false;

         panel1.Paint -= new PaintEventHandler(panel1_PaintCenter);
         panel1.Invalidate();

         this.StartStop.Enabled = true;
         this.btnDone.Enabled = false;

         this.Focus();
      }
   }

   public class Cell
   {
      Panel panel;
      Color color;

      public Cell()
      {
         panel = new Panel();
         panel.Visible = false;
      }

      public int X
      {
         get
         {
            return this.panel.Location.X;
         }
         set
         {
            if(this.panel.Location.X != value)
            {
               this.panel.Location = new Point(value,this.panel.Location.Y);
            }
         }
      }

      public int Y
      {
         get
         {
            return this.panel.Location.Y;
         }
         set
         {
            if(this.panel.Location.Y != value)
            {
               this.panel.Location = new Point(this.panel.Location.X,value);
            }
         }
      }

      public Color Color
      {
         get
         {
            return this.color;
         }
         set
         {
            this.color = value;
         }
      }

      public Panel Panel
      {
         get
         {
            return this.panel;
         }
      }

      public int Size
      {
         get
         {
            return this.panel.Size.Width;
         }
         set
         {
            if(this.panel.Size.Width != value)
            {
               this.panel.Size = new Size(value,value);
            }
         }
      }
   }
}
//End file UIForm.cs

//Begin file StatesOutput.cs
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace CellLifeGame1
{
   /// <summary>
   /// Summary description for StatesOutput.
   /// </summary>
   
   public class StatesOutput : System.Windows.Forms.Form
   {
      private ArrayList stateRules;
      double neighborWeight;
      double centerWeight;
      private System.Windows.Forms.Panel panel1;
      private System.Windows.Forms.Button btnDone;
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.Container components = null;
      private System.Windows.Forms.Label labelNWeight;
      private System.Windows.Forms.Label labelCWeight;
      private System.Windows.Forms.TextBox textNWeight;
      private System.Windows.Forms.TextBox textCWeight;
      private System.Windows.Forms.Panel pnlActivationVals;
      private System.Windows.Forms.Label lblActivationValues;
      private System.Windows.Forms.TextBox textActivationValues;

      private ArrayList activationValues;

      public ArrayList ActivationValues
      {
         get
         {
            return activationValues;
         }
      }

      public double NeighborWeight
      {
         get
         {
            return neighborWeight;
         }
         set
         {
            neighborWeight = value;
            textNWeight.Text = value.ToString();
         }
      }
      public double CenterWeight
      {
         get
         {
            return centerWeight;
         }
         set
         {
            centerWeight = value;
            textCWeight.Text = value.ToString();
         }
      }

      public StatesOutput(int neighborhoodSize, 
         double nWeight, double cWeight, ArrayList activationValues)
      {
         this.activationValues = activationValues;
         
         Console.WriteLine(
            "neighborhoodsize in StatesOutput constructor: {0}",
            neighborhoodSize);

         //
         // Required for Windows Form Designer support
         //
         InitializeComponent();

         stateRules = new ArrayList();

         NeighborWeight = nWeight;
         CenterWeight = cWeight;

         bool isOuter = false;
         if(cWeight == .04D)
         {
            isOuter = true;
         }

         for(int i=0; i<=neighborhoodSize; ++i)
         {
            StateRule stateRule = new StateRule(i,isOuter,false);
            stateRules.Add(stateRule);
            
            foreach(double actVal in activationValues)
            {
               if(
                  Math.Abs(actVal - ( ((double)i) * this.NeighborWeight  ))
                  <= (0.0001D))
               {
                  stateRule.ResultIsAlive = true;
               }
            }
               
            if(Math.Abs(CenterWeight - .04D  ) <= (0.0001D))
            {
               stateRule = new StateRule(i,isOuter,true);
               stateRules.Add(stateRule);

               foreach(double actVal in activationValues)
               {
                  double linksSummation =
                     ((double)i) * this.NeighborWeight + this.CenterWeight;

                  if( Math.Abs(actVal - linksSummation ) <= (0.00001D))
                  {
                     stateRule.ResultIsAlive = true;
                  }
               }
            }
            Console.WriteLine("i in States add is {0}",i);
            Console.WriteLine(
               "neighborhoodsize in States add: {0}",neighborhoodSize);
         }
         
         this.SuspendLayout();

         int j=0;       
         foreach(StateRule stateRule in stateRules)
         {  
            Console.WriteLine("j in States attributes is {0}",j);
            stateRule.Location = new System.Drawing.Point(0, 32*j);
            stateRule.Name = "stateRule" + 1;
            //stateRule.Size = new System.Drawing.Size(400, 32);
            stateRule.TabIndex = j;
            panel1.Controls.Add(stateRule);
            stateRule.result.CheckedChanged +=
               new EventHandler(outputChange);
            ++j;
         }

         int calculatedHeight = 
            ((StateRule)stateRules[1]).Size.Height*
            (neighborhoodSize+1)*
            (isOuter?2:1);
            
         if(calculatedHeight < 600)
         {
            panel1.Size = new Size(416,   calculatedHeight);
         }
         else
         {
            panel1.Size = new Size(416,   600);
         }

         this.ClientSize = new System.Drawing.Size(
            panel1.Size.Width + panel1.Location.X*2,
            panel1.Size.Height + panel1.Location.Y);

         this.ResumeLayout(false);
      }


      public StatesOutput(int neighborhoodSize)
      {  

         Console.WriteLine(
            "neighborhoodsize in StatesOutput constructor: {0}"
            ,neighborhoodSize);
         //
         // Required for Windows Form Designer support
         //
         InitializeComponent();

         stateRules = new ArrayList();

         // Initializes the variables to pass to the MessageBox.Show method.
         string message = "Is your automaton Outer Totalistic?";
         string caption = "Outer";
         MessageBoxButtons buttons = MessageBoxButtons.YesNo;
         DialogResult outerResult;

         // Displays the MessageBox.
         outerResult = MessageBox.Show(this, message, caption, buttons,
            MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

         bool isOuter;
         DialogResult centerResult;
         
         if(outerResult == DialogResult.Yes)
         {
            isOuter  = true;
            CenterWeight = .04D;
         }
         else
         {
            isOuter = false;
            message = "Is the center cell part of the neighborhood?";
            caption = "Center Cell";
            
            centerResult = MessageBox.Show(this, message, caption, buttons,
               MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

            if(centerResult == DialogResult.Yes)
            {
               CenterWeight = .12D;
               neighborhoodSize += 1;
            }
            else
            {
               CenterWeight = .0D;
            }
         }
               
         NeighborWeight = .12D;

         for(int i=0; i<=neighborhoodSize; ++i)
         {
            stateRules.Add(new StateRule(i,isOuter,false));
            if(isOuter)
            {
               stateRules.Add(new StateRule(i,isOuter,true));
            }
            Console.WriteLine("i in States add is {0}",i);
            Console.WriteLine("neighborhoodsize in States add: {0}",
               neighborhoodSize);
         }
         
         this.SuspendLayout();

         int j=0;
         foreach(StateRule stateRule in stateRules)
         {  
            Console.WriteLine("j in States attributes is {0}",j);
            stateRule.Location = new System.Drawing.Point(0, 32*j);
            stateRule.Name = "stateRule" + 1;
            //stateRule.Size = new System.Drawing.Size(400, 32);
            stateRule.TabIndex = j;
            panel1.Controls.Add(stateRule);
            stateRule.result.CheckedChanged +=
               new EventHandler(outputChange);
            ++j;
         }

         int calculatedHeight = 
            ((StateRule)stateRules[1]).Size.Height*
            (neighborhoodSize+1)*
            (isOuter?2:1);
            
         if(calculatedHeight < 600)
         {
            panel1.Size = new Size(416,   calculatedHeight);
         }
         else
         {
            panel1.Size = new Size(416,   600);
         }

         this.ClientSize = new System.Drawing.Size(
            panel1.Size.Width + panel1.Location.X*2,
            panel1.Size.Height + panel1.Location.Y);

         this.ResumeLayout(false);
      }

      public StatesOutput()
      {  
         int neighborhoodSize = 8;

         Console.WriteLine(
            "neighborhoodsize in StatesOutput constructor: {0}",
            neighborhoodSize);
         //
         // Required for Windows Form Designer support
         //
         InitializeComponent();

         stateRules = new ArrayList();

         bool isOuter = true;
         
         for(int i=0; i<=neighborhoodSize; ++i)
         {
            StateRule stateRule = new StateRule(i,isOuter,false);

            if(i == 3)
            {
               stateRule.ResultIsAlive = true;
            }

            stateRules.Add(stateRule);

            if(isOuter)
            {
               stateRule = new StateRule(i,isOuter,true);
               if(i == 2 || i == 3)
               {
                  stateRule.ResultIsAlive = true;
               }
               stateRules.Add(stateRule);


            }
            Console.WriteLine("i in States add is {0}",i);
            Console.WriteLine("neighborhoodsize in States add: {0}",
               neighborhoodSize);
         }

         if(isOuter)
         {
            CenterWeight = .04D;          
         }
         else
         {
            CenterWeight = .0D;
         }
         
         NeighborWeight = .12D;
         
         this.SuspendLayout();

         int j=0;
         
         foreach(StateRule stateRule in stateRules)
         {  
            Console.WriteLine("j in States attributes is {0}",j);
            stateRule.Location = new System.Drawing.Point(0, 32*j);
            stateRule.Name = "stateRule" + 1;
            stateRule.TabIndex = j;
            panel1.Controls.Add(stateRule);

            stateRule.result.CheckedChanged +=
               new EventHandler(outputChange);

            ++j;
         }

         int calculatedHeight = 
            ((StateRule)stateRules[1]).Size.Height*
            (neighborhoodSize+1)*
            (isOuter?2:1);
            
         if(calculatedHeight < 600)
         {
            panel1.Size = new Size(416,   calculatedHeight);
         }
         else
         {
            panel1.Size = new Size(416,   600);
         }

         this.ClientSize = new System.Drawing.Size(
            panel1.Size.Width + panel1.Location.X*2,
            panel1.Size.Height + panel1.Location.Y);

         this.ResumeLayout(false);
      }

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      protected override void Dispose( bool disposing )
      {
         if( disposing )
         {
            if(components != null)
            {
               components.Dispose();
            }
         }
         base.Dispose( disposing );
      }

      #region Windows Form Designer generated code
      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.panel1 = new System.Windows.Forms.Panel();
         this.btnDone = new System.Windows.Forms.Button();
         this.labelNWeight = new System.Windows.Forms.Label();
         this.labelCWeight = new System.Windows.Forms.Label();
         this.textNWeight = new System.Windows.Forms.TextBox();
         this.textCWeight = new System.Windows.Forms.TextBox();
         this.pnlActivationVals = new System.Windows.Forms.Panel();
         this.lblActivationValues = new System.Windows.Forms.Label();
         this.textActivationValues = new System.Windows.Forms.TextBox();
         this.pnlActivationVals.SuspendLayout();
         this.SuspendLayout();
         // 
         // panel1
         // 
         this.panel1.AutoScroll = true;
         this.panel1.Location = new System.Drawing.Point(0, 64);
         this.panel1.Name = "panel1";
         this.panel1.Size = new System.Drawing.Size(448, 32);
         this.panel1.TabIndex = 0;
         // 
         // btnDone
         // 
         this.btnDone.Location = new System.Drawing.Point(8, 8);
         this.btnDone.Name = "btnDone";
         this.btnDone.Size = new System.Drawing.Size(56, 24);
         this.btnDone.TabIndex = 1;
         this.btnDone.Text = "Done";
         this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
         // 
         // labelNWeight
         // 
         this.labelNWeight.Location = new System.Drawing.Point(72, 8);
         this.labelNWeight.Name = "labelNWeight";
         this.labelNWeight.Size = new System.Drawing.Size(120, 16);
         this.labelNWeight.TabIndex = 2;
         this.labelNWeight.Text = "Neighbor Link Weight: ";
         this.labelNWeight.TextAlign =
            System.Drawing.ContentAlignment.TopRight;
         // 
         // labelCWeight
         // 
         this.labelCWeight.Location = new System.Drawing.Point(264, 8);
         this.labelCWeight.Name = "labelCWeight";
         this.labelCWeight.Size = new System.Drawing.Size(108, 16);
         this.labelCWeight.TabIndex = 3;
         this.labelCWeight.Text = "Center Link Weight: ";
         this.labelCWeight.TextAlign = 
            System.Drawing.ContentAlignment.TopRight;
         // 
         // textNWeight
         // 
         this.textNWeight.Location = new System.Drawing.Point(192, 8);
         this.textNWeight.Name = "textNWeight";
         this.textNWeight.ReadOnly = true;
         this.textNWeight.Size = new System.Drawing.Size(64, 20);
         this.textNWeight.TabIndex = 4;
         this.textNWeight.Text = "";
         // 
         // textCWeight
         // 
         this.textCWeight.Location = new System.Drawing.Point(376, 8);
         this.textCWeight.Name = "textCWeight";
         this.textCWeight.ReadOnly = true;
         this.textCWeight.Size = new System.Drawing.Size(64, 20);
         this.textCWeight.TabIndex = 5;
         this.textCWeight.Text = "";
         // 
         // pnlActivationVals
         // 
         this.pnlActivationVals.AutoScroll = true;
         this.pnlActivationVals.Controls.Add(this.textActivationValues);
         this.pnlActivationVals.Controls.Add(this.lblActivationValues);
         this.pnlActivationVals.Location = new System.Drawing.Point(0, 32);
         this.pnlActivationVals.Name = "pnlActivationVals";
         this.pnlActivationVals.Size = new System.Drawing.Size(448, 32);
         this.pnlActivationVals.TabIndex = 6;
         // 
         // lblActivationValues
         // 
         this.lblActivationValues.Location = new System.Drawing.Point(8, 8);
         this.lblActivationValues.Name = "lblActivationValues";
         this.lblActivationValues.Size = new System.Drawing.Size(96, 16);
         this.lblActivationValues.TabIndex = 0;
         this.lblActivationValues.Text = "Activation Values:";
         // 
         // textActivationValues
         // 
         this.textActivationValues.Location =
            new System.Drawing.Point(104, 8);
         this.textActivationValues.Name = "textActivationValues";
         this.textActivationValues.ReadOnly = true;
         this.textActivationValues.Size = new System.Drawing.Size(336, 20);
         this.textActivationValues.TabIndex = 1;
         this.textActivationValues.Text = "";
         // 
         // StatesOutput
         // 
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.ClientSize = new System.Drawing.Size(448, 405);
         this.Controls.Add(this.pnlActivationVals);
         this.Controls.Add(this.textCWeight);
         this.Controls.Add(this.textNWeight);
         this.Controls.Add(this.labelCWeight);
         this.Controls.Add(this.labelNWeight);
         this.Controls.Add(this.btnDone);
         this.Controls.Add(this.panel1);
         this.Name = "StatesOutput";
         this.Text = "StatesOutput";
         this.Load += new System.EventHandler(this.load);
         this.pnlActivationVals.ResumeLayout(false);
         this.ResumeLayout(false);

      }
      #endregion

      private void btnDone_Click(object sender, System.EventArgs e)
      {
         activationValues = new ArrayList();
         foreach(StateRule stateRule in stateRules)
         {
            if(stateRule.ResultIsAlive)
            {
               activationValues.Add(
                  (  ((double)stateRule.NeighborsAlive) *
                  this.NeighborWeight  ) +
                  (stateRule.CenterIsAlive?this.CenterWeight:0.0D)
                  );
            }
         }
         this.Close();
      }

      private void outputChange(object sender, System.EventArgs e)
      {
         textActivationValues.Text = "";
         foreach(StateRule stateRule in stateRules)
         {
            if(stateRule.ResultIsAlive)
            {
               double activationValue = 
                  (  ((double)stateRule.NeighborsAlive) *
                  this.NeighborWeight  ) +
                  (stateRule.CenterIsAlive?this.CenterWeight:0.0D);

               textActivationValues.Text +=
                  activationValue.ToString() + ';';
            }              
         }     
      }

      private void load(object sender, System.EventArgs e)
      {
         textActivationValues.Text = "";
         foreach(StateRule stateRule in stateRules)
         {
            if(stateRule.ResultIsAlive)
            {
               double activationValue = 
                  (  ((double)stateRule.NeighborsAlive) *
                  this.NeighborWeight  ) + 
                  (stateRule.CenterIsAlive?this.CenterWeight:0.0D);

               textActivationValues.Text += activationValue.ToString() + ';';
            }              
         }     
      }
   }
}
//End file StatesOutput.cs

//Begin file StateRule.cs
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace CellLifeGame1
{
   /// <summary>
   /// A case in a set of rules with a checkbox for the user to
   /// specify whether the result in that case is dead or alive.
   /// </summary>
   public class StateRule : System.Windows.Forms.UserControl
   {
      internal System.Windows.Forms.CheckBox result;

      private System.Windows.Forms.Label lblNeighborsAlive;
      private System.Windows.Forms.Label lblCenterAlive;
      /// <summary> 
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.Container components = null;

      public readonly int NeighborsAlive;
      public readonly bool CenterIsAlive;
      public readonly bool IsOuter;
      //public readonly double ActivationValue;
        
      public StateRule(int neighborsCnt, bool isOuter, bool centerIsAlive)
      {
         // This call is required by the Windows.Forms Form Designer.
         InitializeComponent();
         
         this.NeighborsAlive = neighborsCnt;
         this.CenterIsAlive = centerIsAlive;
         this.IsOuter = isOuter;

         lblNeighborsAlive.Text += neighborsCnt;
            
         if(isOuter == false)
         {
            lblCenterAlive.Visible = false;
         }
         else
         {
            if(centerIsAlive == true)
            {
               lblCenterAlive.Text += "Yes";
            }
            else
            { 
               lblCenterAlive.Text += "No";
            }
         }
      }
   
      public bool ResultIsAlive
      {
         get
         {
            return result.Checked;
         }
         set
         {
            result.Checked = value;
         }
      }

      /// <summary> 
      /// Clean up any resources being used.
      /// </summary>
      protected override void Dispose( bool disposing )
      {
         if( disposing )
         {
            if(components != null)
            {
               components.Dispose();
            }
         }
         base.Dispose( disposing );
      }

      #region Component Designer generated code
      /// <summary> 
      /// Required method for Designer support - do not modify 
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.result = new System.Windows.Forms.CheckBox();
         this.lblNeighborsAlive = new System.Windows.Forms.Label();
         this.lblCenterAlive = new System.Windows.Forms.Label();
         this.SuspendLayout();
         // 
         // result
         // 
         this.result.CheckAlign =
            System.Drawing.ContentAlignment.MiddleRight;
         this.result.Location = new System.Drawing.Point(256, 8);
         this.result.Name = "result";
         this.result.Size = new System.Drawing.Size(104, 16);
         this.result.TabIndex = 0;
         this.result.Text = "Results in life?";
         this.result.TextAlign = 
            System.Drawing.ContentAlignment.MiddleRight;
         // 
         // lblNeighborsAlive
         // 
         this.lblNeighborsAlive.Location = new System.Drawing.Point(8, 8);
         this.lblNeighborsAlive.Name = "lblNeighborsAlive";
         this.lblNeighborsAlive.Size = new System.Drawing.Size(128, 16);
         this.lblNeighborsAlive.TabIndex = 1;
         this.lblNeighborsAlive.Text = "Neighbors alive: ";
         // 
         // lblCenterAlive
         // 
         this.lblCenterAlive.Location = new System.Drawing.Point(144, 8);
         this.lblCenterAlive.Name = "lblCenterAlive";
         this.lblCenterAlive.Size = new System.Drawing.Size(112, 16);
         this.lblCenterAlive.TabIndex = 0;
         this.lblCenterAlive.Text = "Center is alive? ";
         // 
         // StateRule
         // 
         this.Controls.Add(this.lblCenterAlive);
         this.Controls.Add(this.lblNeighborsAlive);
         this.Controls.Add(this.result);
         this.Name = "StateRule";
         this.Size = new System.Drawing.Size(368, 32);
         this.ResumeLayout(false);

      }
      #endregion
   }
}
//End file StateRule.cs