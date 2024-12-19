using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restaurant_Ordering_System
{
    public partial class Admin : Form
    {
        OleDbConnection connection = new OleDbConnection();
        OleDbCommand command;
        private DataTable productDataTable;
        public Admin(OleDbConnection connection)
        {
            InitializeComponent();
            this.connection = connection;
            this.command = new OleDbCommand();
            LoadData();
        }

        private void AddProductButton_Click(object sender, EventArgs e)
        {
            // Validate input fields
            if (string.IsNullOrEmpty(productNameTextBox.Text) || selectedImagePath.Image == null)
            {
                MessageBox.Show("Please fill in all required fields.");
                return;
            }

            // Confirm if the user really wants to add the product
            var confirmResult = MessageBox.Show(
                "Are you sure you want to add this product?",
                "Confirm Add",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmResult == DialogResult.No)
            {
                // User chose to cancel the operation
                return;
            }

            // Convert image to byte array
            byte[] imageData;
            using (MemoryStream ms = new MemoryStream())
            {
                selectedImagePath.Image.Save(ms, selectedImagePath.Image.RawFormat);
                imageData = ms.ToArray();
            }

            // Add product to the database
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                command.CommandText = "INSERT INTO Products (ProductName, Description, Price, ImageData, Stock) VALUES (?, ?, ?, ?, ?)";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("ProductName", productNameTextBox.Text);
                command.Parameters.AddWithValue("Description", ddescription.Text);
                command.Parameters.AddWithValue("Price", priceTextBox.Text);
                command.Parameters.AddWithValue("ImageData", imageData);
                command.Parameters.AddWithValue("Stock", stockTextBox.Text);
                command.Connection = connection;

                command.ExecuteNonQuery();
                MessageBox.Show("Product added to the database.");
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
                command.Parameters.Clear();
            }

            // Clear input fields
            productNameTextBox.Text = "";
            ddescription.Text = "";
            priceTextBox.Text = "";
            selectedImagePath.Image = null;
            stockTextBox.Text = "";
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView2.SelectedRows[0];
                productNameTextBox.Text = selectedRow.Cells["ProductName"].Value.ToString();
                ddescription.Text = selectedRow.Cells["Description"].Value.ToString();
                priceTextBox.Text = selectedRow.Cells["Price"].Value.ToString();
                stockTextBox.Text = selectedRow.Cells["Stock"].Value.ToString();
                byte[] imageData = (byte[])selectedRow.Cells["ImageData"].Value;

                selectedImagePath.Image = Image.FromStream(new MemoryStream(imageData));
            }
            else
            {
                MessageBox.Show("Please select a row to edit.");
            }
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView2.SelectedRows[0];
                int productId = Convert.ToInt32(selectedRow.Cells["ProductID"].Value);

                // Confirm if the user really wants to update the product
                var confirmResult = MessageBox.Show(
                    "Are you sure you want to update this product?",
                    "Confirm Update",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (confirmResult == DialogResult.No)
                {
                    // User chose to cancel the operation
                    return;
                }

                try
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }

                    command.CommandText = "UPDATE Products SET ProductName = @ProductName, Description = @Description, Price = @Price, ImageData = @ImageData, Stock = @Stock WHERE ProductID = @ProductID";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@ProductName", productNameTextBox.Text);
                    command.Parameters.AddWithValue("@Description", ddescription.Text);
                    command.Parameters.AddWithValue("@Price", priceTextBox.Text);
                    command.Parameters.AddWithValue("@ImageData", GetImageData(selectedImagePath.Image));
                    command.Parameters.AddWithValue("@Stock", stockTextBox.Text);
                    command.Parameters.AddWithValue("@ProductID", productId);
                    command.Connection = connection;
                    command.ExecuteNonQuery();

                    MessageBox.Show("Product updated in the database.");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                    command.Parameters.Clear();
                }

                // Clear input fields
                productNameTextBox.Text = "";
                ddescription.Text = "";
                priceTextBox.Text = "";
                selectedImagePath.Image = null;
                stockTextBox.Text = "";
            }
            else
            {
                MessageBox.Show("Please select a row to update.");
            }
        }
        public void LoadProducts(OleDbConnection connection, OleDbCommand command)
        {
            string query = "SELECT ProductName, Description, Price, Stock, ImageData FROM Products";
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                command.Connection = connection;
                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    productDataTable.Clear(); // Clear existing data

                    while (reader.Read())
                    {
                        string productName = reader["ProductName"].ToString();
                        string description = reader["Description"].ToString();
                        string price = reader["Price"].ToString();
                        int stock = Convert.ToInt32(reader["Stock"]);
                        byte[] imageData = (byte[])reader["ImageData"];

                        // Add updated data to the data table
                        productDataTable.Rows.Add(productName, description, stock, price, imageData);
                    }

                    // Update the UI with the refreshed data
                    dataGridView2.DataSource = productDataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
                command.Dispose();
            }
        }

        private void btnbrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedImagePath.Image = Image.FromFile(openFileDialog.FileName);
                }
            }
        }
        private byte[] GetImageData(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
                return ms.ToArray();
            }
        }
        private void LoadData()
        {
            string query = "SELECT ProductID, ProductName, Description, Price, ImageData, Stock FROM Products";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);

            dataGridView2.DataSource = dataTable;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        private void btnSales_Click(object sender, EventArgs e)
        {
            Saless s = new Saless(connection);
            s.BringToFront();
            s.Show();
        }
        private void lblLogout_Click(object sender, EventArgs e)
        {
            // Display a confirmation dialog
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to log out?", "Log Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // If the user clicks 'Yes', proceed with the logout
            if (dialogResult == DialogResult.Yes)
            {
                this.Dispose(); // Dispose of the current form
                Form1 l = new Form1(); // Create a new instance of Form1 (login page or main page)
                l.ShowDialog(); // Show Form1 as a dialog
            }
        }
        private void Btnstaff_Click(object sender, EventArgs e)
        {
            ADD_STAFF sa = new ADD_STAFF(connection);
            sa.BringToFront();
            sa.Show();
        }
        private void Admin_Load(object sender, EventArgs e)
        {
            datee.Text = DateTime.Now.ToLongDateString();
            timee.Text = DateTime.Now.ToLongTimeString();
            timer2.Start();

            // Add Delete Button Column to DataGridView
            if (!dataGridView2.Columns.Contains("Delete"))
            {
                DataGridViewButtonColumn deleteButton = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "Action",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true
                };
                dataGridView2.Columns.Add(deleteButton);
            }
        }


        private void timer2_Tick(object sender, EventArgs e)
        {
            datee.Text = DateTime.Now.ToLongDateString();
            timee.Text = DateTime.Now.ToLongTimeString();
            timer2.Start();
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView2.Columns["Delete"].Index && e.RowIndex >= 0)
            {
                // Confirm deletion
                var confirmResult = MessageBox.Show(
                    "Are you sure you want to delete this product?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo);

                if (confirmResult == DialogResult.Yes)
                {
                    try
                    {
                        // Get ProductID of the selected row
                        int productId = Convert.ToInt32(dataGridView2.Rows[e.RowIndex].Cells["ProductID"].Value);

                        // Delete record from database
                        if (connection.State == ConnectionState.Closed)
                        {
                            connection.Open();
                        }
                        command.CommandText = "DELETE FROM Products WHERE ProductID = ?";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@ProductID", productId);
                        command.Connection = connection;
                        command.ExecuteNonQuery();

                        MessageBox.Show("Product deleted successfully.");
                        LoadData(); // Reload data after deletion
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                    finally
                    {
                        if (connection.State == ConnectionState.Open)
                        {
                            connection.Close();
                        }
                    }
                }
            }
        }

        internal void LoadProducts()
        {
            throw new NotImplementedException();
        }
    }
}