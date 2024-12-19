using System.Data;
using System.Data.OleDb;

namespace Restaurant_Ordering_System
{
    public partial class Form1 : Form
    {
        OleDbConnection connection = new OleDbConnection();
        OleDbCommand command;
        Staff form2;
        Admin adminForm;

        public string? connectionString { get; internal set; }

        public Form1()
        {
            InitializeComponent();
            string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source = C:\\Ordering System\\Restaurant Ordering Systems\\NEWDATA.accdb";
            connection.ConnectionString = connectionString;
            form2 = new Staff(connection);
            adminForm = new Admin(connection);
            command = new OleDbCommand();
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
        private void btnn_Click(object sender, EventArgs e)
        {

        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "SELECT COUNT(*) FROM Users WHERE StrComp(username, ?, 0) = 0 AND StrComp(password, ?, 0) = 0";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("username", txtuser.Text);
                command.Parameters.AddWithValue("password", txtpassword.Text);
                int userCount = Convert.ToInt32(command.ExecuteScalar());
                command.CommandText = "SELECT COUNT(*) FROM Addmin WHERE StrComp(username, ?, 0) = 0 AND StrComp(password, ?, 0) = 0";
                int adminCount = Convert.ToInt32(command.ExecuteScalar());
                if (userCount == 1)
                {
                    command.CommandText = "SELECT Role FROM Users WHERE StrComp(username, ?, 0) = 0 AND StrComp(password, ?, 0) = 0";
                }
                else if (adminCount == 1)
                {
                    command.CommandText = "SELECT Role FROM Addmin WHERE StrComp(username, ?, 0) = 0 AND StrComp(password, ?, 0) = 0";
                }
                else
                {
                    MessageBox.Show("Incorrect Username or Password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string? role = command.ExecuteScalar()?.ToString();
                MessageBox.Show($"Login Successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (userCount == 1)
                {
                    if (role.Equals("Staff", StringComparison.OrdinalIgnoreCase))
                    {
                        if (form2 == null || form2.IsDisposed)
                        {
                            form2 = new Staff(connection);
                        }
                        form2.ClearProducts();
                        form2.LoadProducts(connection, command);
                        connection.Close();
                        this.Hide();
                        form2.ShowDialog();
                        form2.Dispose();
                        this.Close();


                    }
                }
                else if (adminCount == 1)
                {
                    if (adminForm == null || adminForm.IsDisposed)
                    {
                        adminForm = new Admin(connection);
                    }
                    this.Hide();
                    adminForm.ShowDialog();
                    this.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
                txtuser.Text = "";
                txtpassword.Text = "";
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void chcpass_CheckedChanged(object sender, EventArgs e)
        {
            if (chcpass.Checked)
            {
                txtpassword.PasswordChar = '\0';
            }
            else
            {
                txtpassword.PasswordChar = '*';
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}