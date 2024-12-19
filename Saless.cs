using System.Data;
using System.Data.OleDb;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace Restaurant_Ordering_System
{

    public partial class Saless : Form
    {
        OleDbConnection connection;
        OleDbCommand command;
        public Saless(OleDbConnection connection)
        {
            InitializeComponent();
            this.connection = connection;
            this.command = new OleDbCommand();
            LoadSalesData();
            CalculateTotalSales();
        }
        public void AddSalesData(DataTable productDataTable, decimal totalAmount, decimal cashAmount, decimal change, string paymentMethod)
        {
            foreach (DataRow row in productDataTable.Rows)
            {
                string? productName = row["Product Name"].ToString();
                decimal price = Convert.ToDecimal(row["Price"]);
                int quantity = Convert.ToInt32(row["Quantity"]);
                DateTime date = DateTime.Now;
                decimal amount = Convert.ToDecimal(row["Amount"]);
                InsertSalesData(productName, price, quantity, date, amount, paymentMethod);
            }
            CalculateTotalSales();
        }
        private void InsertSalesData(string productName, decimal price, int quantity, DateTime date, decimal amount, string paymentMethod)
        {
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                string qry = "INSERT INTO Orders (ProductName, Price, Quantity, Datess, Amount, PaymentMethod) VALUES (?, ?, ?, ?, ?, ?)";

                using (OleDbCommand command = new OleDbCommand(qry, connection))
                {
                    command.Parameters.AddWithValue("ProductName", productName);
                    command.Parameters.AddWithValue("Price", price);
                    command.Parameters.AddWithValue("Quantity", quantity);
                    command.Parameters.AddWithValue("Datess", date.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("Amount", amount);
                    command.Parameters.AddWithValue("PaymentMethod", paymentMethod);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding sales data to the database: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
        private void LoadSalesData()
        {
            DateTime fromDate = dateTimePickerFrom.Value.Date;
            DateTime toDate = dateTimePickerTo.Value.Date.AddDays(1).AddSeconds(-1);

            string query = "SELECT OrderID, ProductName, Price, Quantity, [Datess], Amount, PaymentMethod FROM Orders WHERE [Datess] BETWEEN ? AND ?";
            OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
            adapter.SelectCommand.Parameters.AddWithValue("fromDate", fromDate.ToString("yyyy-MM-dd HH:mm:ss"));
            adapter.SelectCommand.Parameters.AddWithValue("toDate", toDate.ToString("yyyy-MM-dd HH:mm:ss"));
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);

            dgvSales.DataSource = dataTable;
            dgvSales.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSales.ReadOnly = true;
            CalculateTotalSales();
        }
        private void CalculateTotalSales()
        {
            decimal totalSales = 0.00m;
            int totalRecords = dgvSales.RowCount;
            foreach (DataGridViewRow row in dgvSales.Rows)
            {
                if (row.Cells["Amount"].Value != null)
                {
                    totalSales += Convert.ToDecimal(row.Cells["Amount"].Value);
                }
            }
            lblTotalList.Text = $"{totalRecords}";
            lblTotalSales.Text = $"₱ {totalSales}";
        }
        private void btnSearchDates_Click(object sender, EventArgs e)
        {
            LoadSalesData();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Save Sales Data as PDF"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Create a PDF document
                    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 10f);
                    PdfWriter.GetInstance(pdfDoc, new FileStream(saveFileDialog.FileName, FileMode.Create));
                    pdfDoc.Open();

                    // Add title
                    iTextSharp.text.Font titleFont = FontFactory.GetFont("Arial", 16f, iTextSharp.text.Font.BOLD);
                    Paragraph title = new Paragraph("Sales Report\n", titleFont)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    pdfDoc.Add(title);

                    pdfDoc.Add(new Paragraph("\n")); // Add space

                    // Add table
                    PdfPTable pdfTable = new PdfPTable(dgvSales.Columns.Count)
                    {
                        WidthPercentage = 100
                    };

                    // Add table headers
                    iTextSharp.text.Font headerFont = FontFactory.GetFont("Arial", 12f, iTextSharp.text.Font.BOLD);
                    foreach (DataGridViewColumn column in dgvSales.Columns)
                    {
                        PdfPCell headerCell = new PdfPCell(new Phrase(column.HeaderText, headerFont))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER
                        };
                        pdfTable.AddCell(headerCell);
                    }

                    // Add table rows
                    iTextSharp.text.Font cellFont = FontFactory.GetFont("Arial", 10f);
                    foreach (DataGridViewRow row in dgvSales.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell.Value != null)
                            {
                                PdfPCell pdfCell = new PdfPCell(new Phrase(cell.Value.ToString(), cellFont))
                                {
                                    HorizontalAlignment = Element.ALIGN_CENTER
                                };
                                pdfTable.AddCell(pdfCell);
                            }
                        }
                    }

                    // Add table to document
                    pdfDoc.Add(pdfTable);
                    pdfDoc.Close();

                    MessageBox.Show("PDF saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dgvSales_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}


