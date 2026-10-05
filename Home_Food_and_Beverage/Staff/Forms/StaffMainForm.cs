using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Home_Food_and_Beverage.Staff.UserControls;

namespace Home_Food_and_Beverage.Staff.Forms
{
	public partial class StaffMainForm : Form
	{
		public StaffMainForm()
		{
			InitializeComponent();
		}

		public void loadform(object form)
		{
			if (this.panel3.Controls.Count > 0)
				this.panel3.Controls.RemoveAt(0);
			Form f = form as Form;
			if (f != null)
			{
				f.TopLevel = false;
				f.FormBorderStyle = FormBorderStyle.None;
				f.Dock = DockStyle.Fill;
				this.panel3.Controls.Add(f);
				this.panel3.Tag = f;
				f.Show();
			}
		}

		private void pictureBox1_Click(object sender, EventArgs e)
		{

		}

		private void pictureBox1_Click_1(object sender, EventArgs e)
		{

		}

		private void button2_Click(object sender, EventArgs e)
		{
			loadform(new OrderTakingAndProcessing());
		}

		private void panel3_Paint(object sender, PaintEventArgs e)
		{
		
		}

		private void button4_Click(object sender, EventArgs e)
		{
			loadform(new InventoryAndStockTracker());
		}

		private void button3_Click(object sender, EventArgs e)
		{
			loadform(new RecipeReview());
		}

		private void button5_Click(object sender, EventArgs e)
		{
			loadform(new SalesAndRecords());
		}
	}
}
