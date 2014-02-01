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
				panel1.Size = new Size(416,	calculatedHeight);
			}
			else
			{
				panel1.Size = new Size(416,	600);
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
				panel1.Size = new Size(416,	calculatedHeight);
			}
			else
			{
				panel1.Size = new Size(416,	600);
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
				panel1.Size = new Size(416,	calculatedHeight);
			}
			else
			{
				panel1.Size = new Size(416,	600);
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