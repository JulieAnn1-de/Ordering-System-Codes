using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Windows.Forms;

namespace Restaurant_Ordering_System
{
    public partial class Staff : Form
    {
        OleDbCommand command;
        private DataTable productDataTable;
        private OleDbConnection connection;
        private Saless salesForm;
        public Staff(OleDbConnection connection)
        {
            InitializeComponent();
            this.connection = connection;
            this.command = new OleDbCommand();
            dataGridView1.DataSource = productDataTable;
            txtCash.TextChanged += TxtCash_TextChanged;
            salesForm = new Saless(connection);

            InitializeDataTable();


        }
        private void InitializeDataTable()
        {
            productDataTable = new DataTable();
            productDataTable.Columns.Add("Product Name");
            productDataTable.Columns.Add("Description");
            productDataTable.Columns.Add("Quantity");
            productDataTable.Columns.Add("Price");
            productDataTable.Columns.Add("Amount");

        }
        public void LoadProducts(OleDbConnection connection, OleDbCommand command)
        {
            command.CommandText = "SELECT ProductName, Description, Price, Stock, ImageData FROM Products"; // Include Stock in query
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                command.Connection = connection;
                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    productDataTable.Clear();
                    while (reader.Read())
                    {
                        string? productName = reader["ProductName"].ToString();
                        string? description = reader["Description"].ToString();
                        string? price = reader["Price"].ToString();
                        int stock = Convert.ToInt32(reader["Stock"]);
                        byte[] imageData = (byte[])reader["ImageData"];

                        string category = GetCategoryFromDescription(description);
                        Panel productPanel = new Panel();
                        productPanel.BorderStyle = BorderStyle.FixedSingle;
                        productPanel.Width = 200;
                        productPanel.Height = 250; // Increase height to accommodate stock label

                        PictureBox pictureBox = new PictureBox();
                        pictureBox.Width = 195;
                        pictureBox.Height = 100;
                        pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox.Image = Image.FromStream(new MemoryStream(imageData));
                        pictureBox.Cursor = Cursors.Hand;

                        NumericUpDown quantityNumericUpDown = new NumericUpDown();
                        quantityNumericUpDown.Minimum = 1;
                        quantityNumericUpDown.Maximum = 100;
                        quantityNumericUpDown.Value = 1;
                        quantityNumericUpDown.Width = 60;

                        Label stockLabel = new Label();  // Label to display stock
                        stockLabel.Text = $"Stock: {stock}";
                        stockLabel.ForeColor = stock > 0 ? Color.Green : Color.Red;  // Green if in stock, Red if out of stock
                        stockLabel.Size = new Size(200, 20);
                        stockLabel.TextAlign = ContentAlignment.MiddleCenter;
                        stockLabel.Dock = DockStyle.Bottom;

                        Panel labelsPanel = new Panel();
                        labelsPanel.Dock = DockStyle.Bottom;
                        labelsPanel.AutoSize = true;

                        Label nameLabel = new Label();
                        nameLabel.AutoSize = true;
                        nameLabel.Text = productName;
                        labelsPanel.Dock = DockStyle.Top;
                        nameLabel.TextAlign = ContentAlignment.MiddleCenter;
                        nameLabel.Size = new Size(200, 20);

                        Label descriptionLabel = new Label();
                        descriptionLabel.AutoSize = true;
                        descriptionLabel.Text = description;
                        descriptionLabel.Top = nameLabel.Bottom;
                        descriptionLabel.Size = new Size(200, 20);

                        Label priceLabel = new Label();
                        priceLabel.AutoSize = true;
                        priceLabel.Text = $"₱{price}";
                        priceLabel.Dock = DockStyle.Right;
                        priceLabel.TextAlign = ContentAlignment.BottomRight;
                        priceLabel.Size = new Size(80, 20);

                        labelsPanel.Controls.Add(nameLabel);
                        labelsPanel.Controls.Add(descriptionLabel);
                        labelsPanel.Controls.Add(priceLabel);
                        productPanel.Controls.Add(pictureBox);
                        productPanel.Controls.Add(labelsPanel);
                        productPanel.Controls.Add(quantityNumericUpDown);
                        productPanel.Controls.Add(stockLabel); // Add the stock label
                        labelsPanel.Dock = DockStyle.Bottom;
                        quantityNumericUpDown.Dock = DockStyle.Bottom;

                        quantityNumericUpDown.ValueChanged += (s, e) =>
                        {
                            int quantity = (int)quantityNumericUpDown.Value;
                            if (quantity <= 0)
                            {
                                MessageBox.Show("Invalid quantity. Please enter a value greater than zero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            decimal Price = decimal.Parse(price);
                            decimal amount = quantity * Price;
                            foreach (DataRow row in productDataTable.Rows)
                            {
                                if (row["Product Name"].ToString() == productName)
                                {
                                    row["Quantity"] = quantity;
                                    row["Price"] = Price;
                                    row["Amount"] = amount;
                                }
                            }
                            CalculateTotalAmount();
                        };
                        pictureBox.Click += (s, e) =>
                        {
                            int quantity = (int)quantityNumericUpDown.Value;
                            decimal Price = decimal.Parse(price);
                            decimal amount = quantity * Price;

                            if (stock > 0 && quantity <= stock)  // Check if stock is sufficient
                            {
                                bool productExists = false;
                                foreach (DataRow row in productDataTable.Rows)
                                {
                                    if (row["Product Name"].ToString() == productName)
                                    {
                                        productExists = true;
                                        row["Quantity"] = quantity;
                                        row["Price"] = Price;
                                        row["Amount"] = amount;
                                        break;
                                    }
                                }
                                if (!productExists)
                                {
                                    productDataTable.Rows.Add(productName, description, quantity, Price, amount);
                                }
                                dataGridView1.DataSource = productDataTable;
                                CalculateTotalAmount();
                            }
                            else
                            {
                                MessageBox.Show($"Not enough stock available for {productName}.", "Out of Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        };

                        if (category.Equals("Burger", StringComparison.OrdinalIgnoreCase))
                        {
                            flowLayoutPanel1.Controls.Add(productPanel);
                        }
                        else if (category.Equals("Drinks", StringComparison.OrdinalIgnoreCase))
                        {
                            flowLayoutPanel2.Controls.Add(productPanel);
                        }
                        else if (category.Equals("Dessert", StringComparison.OrdinalIgnoreCase))
                        {
                            flowLayoutPanel3.Controls.Add(productPanel);
                        }
                        else if (category.Equals("Pizza", StringComparison.OrdinalIgnoreCase))
                        {
                            flowLayoutPanel4.Controls.Add(productPanel);
                        }
                        else if (category.Equals("Chicken", StringComparison.OrdinalIgnoreCase))
                        {
                            flowLayoutPanel5.Controls.Add(productPanel);
                        }
                        else if (category.Equals("Spaghetti", StringComparison.OrdinalIgnoreCase))
                        {
                            flowLayoutPanel6.Controls.Add(productPanel);
                        }
                    }
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
            dataGridView1.DataSource = productDataTable;
        }

        public void ClearProducts()
        {
            productDataTable.Clear();
            dataGridView1.DataSource = productDataTable;
        }
        private decimal CalculateTotalAmount()
        {
            decimal totalAmount = 0.00m;
            foreach (DataRow row in productDataTable.Rows)
            {
                if (row["Amount"] != DBNull.Value)
                {
                    totalAmount += Convert.ToDecimal(row["Amount"]);
                }
            }
            if (!string.IsNullOrEmpty(txtDiscount.Text) && decimal.TryParse(txtDiscount.Text, out decimal discount))
            {
                totalAmount -= discount;
            }
            totalLabel.Text = $"₱ {totalAmount}";
            return totalAmount;
        }
        private void CalculateChange()
        {
            decimal totalAmount = CalculateTotalAmount();
            if (!string.IsNullOrEmpty(txtCash.Text) && decimal.TryParse(txtCash.Text, out decimal cashAmount))
            {
                decimal change = cashAmount - totalAmount;
                txtChange.Text = $"{change}";
            }
            else
            {
                txtChange.Text = "0.00";
            }
        }
        private void TxtCash_TextChanged(object sender, EventArgs e)
        {
            CalculateChange();
        }
        private void Staff_Load(object sender, EventArgs e)


        {
            txtChange.ReadOnly = true;
            AddRemoveButtonColumn();
            Datess.Text = DateTime.Now.ToLongDateString();
            Timers.Text = DateTime.Now.ToLongTimeString();
            timer1.Start();
        }
        private string GetCategoryFromDescription(string description)
        {
            if (description.Contains("Burger"))
            {
                return "Burger";
            }
            else if (description.Contains("Drinks"))
            {
                return "Drinks";
            }
            else if (description.Contains("Dessert"))
            {
                return "Dessert";
            }
            else if (description.Contains("Pizza"))
            {
                return "Pizza";
            }
            else if (description.Contains("Chicken"))
            {
                return "Chicken";
            }
            else if (description.Contains("Spaghetti"))
            {
                return "Spaghetti";
            }
            else
            {
                return "Other";
            }
        }
        private void btnReceipt_Click(object sender, EventArgs e)
        {
            decimal totalAmount = CalculateTotalAmount();
            if (cmbPaymentMethod.SelectedItem == null)
            {
                MessageBox.Show("Please choose a payment method.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string? paymentMethod = cmbPaymentMethod.SelectedItem.ToString();
            decimal discount = 0;
            if (paymentMethod == "Cash")
            {
                if (!string.IsNullOrEmpty(txtCash.Text) && decimal.TryParse(txtCash.Text, out decimal cashAmount))
                {
                    decimal change = cashAmount - totalAmount;
                    if (change < 0)
                    {
                        MessageBox.Show("Invalid Cash Amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    foreach (DataRow row in productDataTable.Rows)
                    {
                        string productName = row["Product Name"].ToString();
                        int quantity = Convert.ToInt32(row["Quantity"]);

                        // Update stock in the database
                        UpdateStock(productName, quantity);

                        // Reduce stock and update the stock label in the UI
                        foreach (Panel productPanel in flowLayoutPanel1.Controls)  // or other flow panels if product is in different category
                        {
                            if (productPanel.Controls.OfType<Label>().Any(label => label.Text.Contains(productName)))
                            {
                                // Find the stock label and update it
                                Label stockLabel = productPanel.Controls.OfType<Label>()
                                    .FirstOrDefault(label => label.Text.StartsWith("Stock:"));
                                if (stockLabel != null)
                                {
                                    int currentStock = Convert.ToInt32(stockLabel.Text.Split(':')[1].Trim());
                                    currentStock -= quantity;  // Reduce stock by the quantity sold
                                    stockLabel.Text = $"Stock: {currentStock}";
                                    stockLabel.ForeColor = currentStock > 0 ? Color.Green : Color.Red;  // Update stock color
                                }
                                break;
                            }
                        }
                    }

                    if (productDataTable.Rows.Count == 0)
                    {
                        MessageBox.Show("Please add Orders to the list.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Process the kitchen orders
                    OrdersInKitchen(flowLayoutPanelKitchen);

                    // Add sales data
                    salesForm.AddSalesData(productDataTable, totalAmount, cashAmount, change, paymentMethod);
                    decimal.TryParse(txtDiscount.Text, out discount);

                    // Show the receipt
                    ReceiptForms receiptForm = new ReceiptForms(productDataTable, totalAmount, cashAmount, change, discount);
                    receiptForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Please enter a valid cash amount.");
                }
            }
            else if (paymentMethod == "Gcash")
            {
                // Process the kitchen orders
                OrdersInKitchen(flowLayoutPanelKitchen);

                // Add sales data for Gcash
                salesForm.AddSalesData(productDataTable, totalAmount, 0, 0, paymentMethod);
                decimal.TryParse(txtDiscount.Text, out discount);

                // Show the Gcash receipt
                ReceiptForms2 receiptForm = new ReceiptForms2(productDataTable, totalAmount, discount);
                receiptForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Invalid payment method selected.");
                return;
            }

            // Clear the shopping cart and reset the form
            ClearProducts();
            productDataTable.Clear();
            totalLabel.Text = "0.00";
            txtDiscount.Text = "";
            txtCash.Text = "";
            txtChange.Text = "";
        }


        public void UpdateStock(string productName, int quantitySold)
        {
            try
            {
                // Open connection
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                // Update stock in the database
                string updateQuery = "UPDATE Products SET Stock = Stock - ? WHERE ProductName = ?";
                using (OleDbCommand updateCommand = new OleDbCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("?", quantitySold);
                    updateCommand.Parameters.AddWithValue("?", productName);
                    updateCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating stock: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void CompleteOrder(FlowLayoutPanel kitchenPanel, Panel orderPanel)
        {
            kitchenPanel.Controls.Remove(orderPanel);
            MessageBox.Show("Order Completed!");
        }
        private int orderCounter = 1;
        private void OrdersInKitchen(FlowLayoutPanel kitchenPanel)
        {
            Panel orderPanel = new Panel();
            orderPanel.BorderStyle = BorderStyle.FixedSingle;
            orderPanel.Width = 200;
            orderPanel.Height = 350;
            orderPanel.AutoSize = true;

            Label order = new Label();
            int currentOrderNumber = orderCounter++;
            order.Text = $"Order #{currentOrderNumber} Summary";
            order.Font = new Font(order.Font, FontStyle.Bold);
            order.Location = new Point(10, 10);
            orderPanel.Controls.Add(order);
            foreach (DataRow row in productDataTable.Rows)
            {
                string? productName = row["Product Name"].ToString();
                int quantity = Convert.ToInt32(row["Quantity"]);

                Label lblProduct = new Label();
                lblProduct.Text = $"{productName} - {quantity} x";
                lblProduct.AutoSize = true;
                lblProduct.Location = new Point(10, order.Bottom + 5);

                if (cmbPaymentMethod.SelectedItem.ToString() == "Gcash")
                {
                    UpdateStock(productName, quantity);
                }
                orderPanel.Controls.Add(lblProduct);
                order = lblProduct;
            }
            Button BtnComplete = new Button();
            BtnComplete.Text = "Complete";
            BtnComplete.Location = new Point(10, order.Bottom + 15);
            BtnComplete.Click += (s, e) => CompleteOrder(kitchenPanel, orderPanel);
            orderPanel.Controls.Add(BtnComplete);
            kitchenPanel.Controls.Add(orderPanel);
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
            // If the user clicks 'No', do nothing and stay on the current form
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Timers.Text = DateTime.Now.ToLongTimeString();
            timer1.Start();
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView1.Columns["RemoveColumn"].Index)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                dataGridView1.Rows.RemoveAt(e.RowIndex);
                CalculateTotalAmount();
            }
        }
        private void AddRemoveButtonColumn()
        {
            DataGridViewButtonColumn removeButtonColumn = new DataGridViewButtonColumn();
            removeButtonColumn.HeaderText = "Remove";
            removeButtonColumn.Text = "Remove";
            removeButtonColumn.Name = "RemoveColumn";
            removeButtonColumn.UseColumnTextForButtonValue = true;
            dataGridView1.Columns.Add(removeButtonColumn);
            dataGridView1.Columns["RemoveColumn"].DisplayIndex = dataGridView1.Columns.Count - 1;
        }
        private void txtDiscount_TextChanged(object sender, EventArgs e)
        {
            CalculateTotalAmount();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel8_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            Saless s = new Saless(connection);
            s.BringToFront();
            s.Show();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
