using Home_Food_and_Beverage.Data;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Home_Food_and_Beverage.Admin.Forms
{
    public partial class EmployeeManagement : Form
    {
        private StaffRepository staffRepository = new StaffRepository();
        private Models.Staff currentStaff;

        public EmployeeManagement()
        {
            InitializeComponent();
            LoadStaff();

            comboBox2.SelectedIndex = 0;
            ClearForm();

            if (!comboBox2.Items.Contains("All"))
                comboBox2.Items.Insert(0, "All");

            comboBox2.SelectedIndex = 0;
        }


        // =====================================================
        //       RIGHT SIDE - Employee Management Form
        // =====================================================

        public void LoadStaffData(Models.Staff staff)
        {
            currentStaff = staff;
            txtEmployeeId.Text = staff.Id.ToString();
            txtName.Text = staff.Name;
            txtPin.Text = staff.Pin;
            cboRole.Text = staff.Role;
            
        }

        private void ClearForm()
        {
            currentStaff = null;
            dataGridView1.ClearSelection();
            txtEmployeeId.Text = "(new)";
            txtName.Clear();
            txtPin.Clear();
            cboRole.SelectedIndex = -1;
            
        }

        private void LoadStaff()
        {
            string role = comboBox2.SelectedItem?.ToString() ?? "All";
            List<Models.Staff> staffList = staffRepository.SearchStaff(role, textBox1.Text.Trim());

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = staffList;

            if (dataGridView1.Columns["Pin"] != null)
                dataGridView1.Columns["Pin"].Visible = true;

            if (dataGridView1.Columns["Id"] != null)
                dataGridView1.Columns["Id"].HeaderText = "Employee ID";
        }

        // Add button click event handler

        private void button1_Click_1(object sender, EventArgs e)
        {

            if (!ValidateFields()) return;

            staffRepository.AddStaff(new Models.Staff
            {
                Name = txtName.Text.Trim(),
                Pin = txtPin.Text.Trim(),
                Role = cboRole.Text
            });

            MessageBox.Show("Staff added successfully!");
            ClearForm();
            LoadStaff();
        }

        // Edit button click event handler

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            if (currentStaff == null)
            {
                MessageBox.Show("Select an employess from the directory first.");
                return;
            }

            if (!ValidateFields()) return;

            currentStaff.Name = txtName.Text.Trim(); ;
            currentStaff.Pin = txtPin.Text.Trim();
            currentStaff.Role = cboRole.Text;

            staffRepository.UpdateStaff(currentStaff);
            MessageBox.Show("Staff updated successfully!");
        }

        // Delete button click event handler

        private void button3_Click(object sender, EventArgs e)
        {
            if (currentStaff == null)
            {
                MessageBox.Show("Select an employee from the directory first.");
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Delete " + currentStaff.Name + "?", "Confirm delete", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            try
            {
                staffRepository.DeleteStaff(currentStaff.Id);
            }
            catch (MySqlException ex) when (ex.Number == 1451)
            {
                MessageBox.Show(
                    "This employee can't be deleted because they have orders on record.",
                    "Cannot delete");
                return;
            }

            MessageBox.Show("Employee deleted.");
            ClearForm();
            LoadStaff();
        }

        // Clear button click event handler

        private void button5_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        // Download CVS button click event handler

        private void button4_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                FileName = "all_employees_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv"
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            // Always the full employee list, ignoring the Role filter and search box
            List<Models.Staff> allStaff = staffRepository.GetAllStaff();

            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Id,Name,PIN,Role");

            foreach (Models.Staff s in allStaff)
                csv.AppendLine($"{s.Id},{Escape(s.Name)},{Escape(s.Pin)},{Escape(s.Role)}");

            try
            {
                File.WriteAllText(dialog.FileName, csv.ToString(), Encoding.UTF8);
                MessageBox.Show($"Saved {allStaff.Count} employees.");
            }
            catch (IOException)
            {
                // Happens if the file is already open in Excel
                MessageBox.Show("Couldn't save the file. If it's open in Excel, close it and try again.");
            }
        }
        private string Escape(string value)
        {
            return "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
        }

        //Validate input fields

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtPin.Text) ||
                cboRole.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill in all fields.");
                return false;
            }
            return true;
        }

        // =====================================================
        //  LEFT SIDE - Employee Directory
        // =====================================================

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

                // Id handling - id field is read-only
                if (e.RowIndex < 0) return;

                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                if (row.DataBoundItem is Models.Staff staff)
                {
                    currentStaff = staff;
                    txtEmployeeId.Text = staff.Id.ToString();
                    txtName.Text = staff.Name;
                    txtPin.Text = staff.Pin;
                    cboRole.SelectedItem = staff.Role;
                    lblHint.Text = "";
                }

            
        }

        // Role Filter

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStaff();
        }


        // Search Bar

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LoadStaff();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            AdminDashboard dashboard = new AdminDashboard();
            dashboard.Show();
            this.Hide();
        }
    }
}
