using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Drawing.Printing;
using System.Reflection.Metadata;
using System.Threading.Channels;
using System.Xml.Linq;
using iText.Layout.Font;
using Document = iTextSharp.text.Document;

namespace Restaurant_Ordering_System
{
    public partial class ReceiptForms : Form
    {
        private readonly float content;
        private DataTable receiptData;
        private decimal totalAmount;
        private decimal cashAmount;
        private decimal change;
        private decimal discount;

        public ReceiptForms(DataTable receiptData, decimal totalAmount, decimal cashAmount, decimal change, decimal discount)
        {
            InitializeComponent();
            this.receiptData = receiptData;
            this.totalAmount = totalAmount;
            this.cashAmount = cashAmount;
            this.change = change;
            this.discount = discount;
            DisplayReceipt(receiptData, totalAmount, cashAmount, change, discount);
        }




        private void DisplayReceipt(DataTable receiptData, decimal totalAmount, decimal cashAmount, decimal change, decimal discount)
        {
            DateTime now = DateTime.Now;
            richTextBoxReceipt.AppendText("                 Restaurant Receipt\n");
            richTextBoxReceipt.AppendText($"                {now.ToShortDateString()} at {now.ToShortTimeString()}\n\n");
            richTextBoxReceipt.AppendText("=======================================================\n\n");
            richTextBoxReceipt.Font = new System.Drawing.Font(FontFamily.GenericMonospace, richTextBoxReceipt.Font.Size);
            richTextBoxReceipt.AppendText($"{string.Format("{0,-20} {1,-10} {2,-10} {3,-15}", "Product Name: ", "Quantity", "Price", "Amount")}\n");
            foreach (DataRow row in receiptData.Rows)
            {
                string? productName = row["Product Name"].ToString();
                int quantity = Convert.ToInt32(row["Quantity"]);
                decimal price = Convert.ToDecimal(row["Price"]);
                decimal amount = Convert.ToDecimal(row["Amount"]);

                richTextBoxReceipt.AppendText($"{string.Format("{0,-20} {1,-10} {2,-10:C} {3,-15:C}", productName, quantity, price, amount)}\n");
            }
            richTextBoxReceipt.AppendText("=======================================================\n\n");
            richTextBoxReceipt.AppendText($"{"Total",-20} {totalAmount:C}\n");
            richTextBoxReceipt.AppendText($"{"Discount",-20} {discount:C}\n");
            richTextBoxReceipt.AppendText($"{"Cash",-20} {cashAmount:C}\n");
            richTextBoxReceipt.AppendText($"{"Change",-20} {change:C}\n\n");

            richTextBoxReceipt.AppendText("========================================================\n");
            richTextBoxReceipt.AppendText("           Thank you for dining with us!\n");
            richTextBoxReceipt.AppendText("========================================================\n");
        }

        private void ReceiptForms_Load(object sender, EventArgs e)
        {
        }
        public string ReceiptContent
        {
            get { return richTextBoxReceipt.Text; }
            set { richTextBoxReceipt.Text = value; }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Get content from RichTextBox
            string receiptContent = richTextBoxReceipt.Text;

            // Call SaveReceiptAsPdf with the correct parameters
            SaveReceiptAsPdf(receiptData, totalAmount, cashAmount, change, discount);

            this.Close(); // Close the form after saving
        }

        private void SaveReceiptAsPdf(DataTable receiptData, decimal totalAmount, decimal cashAmount, decimal change, decimal discount)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = "Receipt.pdf"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;

                Document document = new Document(PageSize.A4, 36f, 36f, 36f, 36f);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));

                document.Open();

                // Font initialization
                iTextSharp.text.Font fontNormal = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontBold = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font fontItalic = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.ITALIC);

                // Add content to the PDF
                document.Add(new Paragraph("Restaurant Receipt", fontBold));
                document.Add(new Paragraph($"Date: {DateTime.Now.ToShortDateString()} Time: {DateTime.Now.ToShortTimeString()}\n\n", fontNormal));

                // Add table content
                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;

                table.AddCell(new PdfPCell(new Phrase("Product Name", fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase("Quantity", fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase("Price", fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase("Amount", fontBold)) { HorizontalAlignment = Element.ALIGN_CENTER });

                foreach (DataRow row in receiptData.Rows)
                {
                    string productName = row["Product Name"].ToString();
                    int quantity = Convert.ToInt32(row["Quantity"]);
                    decimal price = Convert.ToDecimal(row["Price"]);
                    decimal amount = Convert.ToDecimal(row["Amount"]);

                    table.AddCell(new PdfPCell(new Phrase(productName, fontNormal)));
                    table.AddCell(new PdfPCell(new Phrase(quantity.ToString(), fontNormal)));
                    table.AddCell(new PdfPCell(new Phrase(price.ToString("C"), fontNormal)));
                    table.AddCell(new PdfPCell(new Phrase(amount.ToString("C"), fontNormal)));
                }

                // Add totals
                document.Add(table);
                document.Add(new Paragraph("\n-------------------------------------------------------", fontNormal));
                document.Add(new Paragraph($"Total: {totalAmount:C}", fontNormal));
                document.Add(new Paragraph($"Discount: {discount:C}", fontNormal));
                document.Add(new Paragraph($"Cash: {cashAmount:C}", fontNormal));
                document.Add(new Paragraph($"Change: {change:C}", fontNormal));
                document.Add(new Paragraph("\n-------------------------------------------------------", fontNormal));
                document.Add(new Paragraph("Thank you for dining with us!", fontItalic));
                document.Add(new Paragraph("\n-------------------------------------------------------", fontNormal));

                document.Close();

                MessageBox.Show("Receipt saved as PDF.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void richTextBoxReceipt_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

