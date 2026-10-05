using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Home_Food_and_Beverage.Admin;
using Home_Food_and_Beverage.Staff.Forms;

namespace Home_Food_and_Beverage
{
	public partial class StaffLoginForm : Form
	{
		public StaffLoginForm()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			
		}

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        // Event Handler for the "Login as Admin" button click

        private void button2_Click(object sender, EventArgs e)
        {
            HeadAdminLogin adminLoginForm = new HeadAdminLogin();
            adminLoginForm.Show();
            this.Hide();
        }
    }
}
