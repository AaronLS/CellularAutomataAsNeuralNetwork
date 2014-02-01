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