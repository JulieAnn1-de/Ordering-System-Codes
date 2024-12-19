using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restaurant_Ordering_System
{
    public partial class ADD_STAFF : Form
    {
        OleDbConnection connection;
        OleDbCommand command;
        private int selectedStaffId;
        public ADD_STAFF(OleDbConnection connection)
        {
            InitializeComponent();
            this.connection = connection;
            this.command = new OleDbCommand();
            LoadData();
        }
        private void btnsave_Click(object sender, EventArgs e)
        {
            var input = txtPassword.Text;
            if (input == "")
            {
                MessageBox.Show("Password Should not be Empty!");
                return;
            }
            var hasNumber = new Regex(@"[0-9]+");
            var hasLowercase = new Regex(@"[a-z]+");
            var hasUppercase = new Regex(@"[A-Z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+]+");

            if (!hasNumber.IsMatch(input))
            {
                MessageBox.Show("The Password Should Contain at least one Numeric Value:\n");
                return;
            }
            else if (!hasLowercase.IsMatch(input))
            {
                MessageBox.Show("The Password Should Contain at least one Lower Case:\n");
                return;
            }
            else if (!hasUppercase.IsMatch(input))
            {
                MessageBox.Show("The Password Should Contain at least one Upper Case:\n");
                return;
            }
            else if (!hasSymbols.IsMatch(input))
            {
                MessageBox.Show("The Password Should Contain at least one Symbol:\n");
                return;
            }
            string name = txtName.Text;
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;
            string role = txtRole.Text;

            if (password != confirmPassword)
            {
                MessageBox.Show("Password and Confirm Password do not match.");
                return;
            }
            using (OleDbConnection connection = new OleDbConnection(this.connection.ConnectionString))
            {
                connection.Open();
                string insertQuery = "INSERT INTO Users (Staffname, Username, [Password], Role) VALUES (?, ?, ?, ?)";
                using (OleDbCommand cmd = new OleDbCommand(insertQuery, connection))
                {
                    cmd.Parameters.AddWithValue("Staffname", name);
                    cmd.Parameters.AddWithValue("Username", username);
                    cmd.Parameters.AddWithValue("Password", password);
                    cmd.Parameters.AddWithValue("Role", role);
                    cmd.ExecuteNonQuery();
                }
            }
            MessageBox.Show("Staff Register successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadData();
            txtName.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";
            txtConfirmPassword.Text = "";
            txtRole.Text = "";
        }
        private void LoadData()
        {
            using (OleDbConnection connection = new OleDbConnection(this.connection.ConnectionString))
            {
                connection.Open();
                string selectQuery = "SELECT * FROM Users";
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(selectQuery, connection))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
        }
        private void picClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                int selectedRowIndex = dataGridView1.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridView1.Rows[selectedRowIndex];

                selectedStaffId = Convert.ToInt32(selectedRow.Cells["ID"].Value);             
                txtName.Text = selectedRow.Cells["Staffname"].Value.ToString();
                txtUsername.Text = selectedRow.Cells["Username"].Value.ToString();
                txtPassword.Text = selectedRow.Cells["Password"].Value.ToString();
                txtConfirmPassword.Text = ""; 
                txtRole.Text = selectedRow.Cells["Role"].Value.ToString();
            }
            else
            {
                MessageBox.Show("Please select a staff member to edit.");
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedStaffId > 0)
            {
                string name = txtName.Text;
                string username = txtUsername.Text;
                string password = txtPassword.Text;
                string confirmPassword = txtConfirmPassword.Text;
                string role = txtRole.Text;

                if (password != confirmPassword)
                {
                    MessageBox.Show("Password and Confirm Password do not match.");
                    return;
                }

                using (OleDbConnection connection = new OleDbConnection(this.connection.ConnectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Users SET Staffname = ?, Username = ?, [Password] = ?, Role = ? WHERE ID = ?";
                    using (OleDbCommand cmd = new OleDbCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("Staffname", name);
                        cmd.Parameters.AddWithValue("Username", username);
                        cmd.Parameters.AddWithValue("Password", password);
                        cmd.Parameters.AddWithValue("Role", role);
                        cmd.Parameters.AddWithValue("ID", selectedStaffId);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Staff member updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
                ClearFields();
            }
            else
            {
                MessageBox.Show("Please select a staff member to update.");
            }
        }
        private void ClearFields()
        {
            txtName.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";
            txtConfirmPassword.Text = "";
            txtRole.Text = "";
            selectedStaffId = 0;
        }
    }
}
