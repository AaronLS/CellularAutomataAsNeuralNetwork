//Begin file UIForm.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using CellLifeGame1.Model;


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
			this.sldrSpeed.Minimum = 1;
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
				if(	link.Destin.Equals(node) )
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
