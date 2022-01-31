using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataTable = System.Data.DataTable;
//using Microsoft.Office.Interop.Word;

namespace FinalProjectST
{
    public partial class TableForm : Form
    {
        private int tableID;
        public TableForm(int TableID)
        {
            this.tableID = TableID;
            InitializeComponent();
        }

        private void Table_Form_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData()
        {
            this.guna2CirclePictureBox1.Image = Image.FromFile("../../Image/table.jpg");
            DataTable table = TABLE.getTableByID(this.tableID);
            if (table.Rows.Count > 0)
            {
                this.label_ID.Text = "Table ID: " + table.Rows[0]["id"].ToString();
                this.label_Chair.Text = "Chair: " + table.Rows[0]["chairs"].ToString();
            }
            if (int.Parse(table.Rows[0]["status"].ToString()) == 0)
            {
                this.label_status.Text = "Status: Free";
                this.button_book.Enabled = true;
                this.Button_Pay.Enabled = false;
                this.Button_order.Enabled = false;
            }
            else
            {
                this.label_status.Text = "Status: Busy";
                this.button_book.Enabled = false;
                this.Button_Pay.Enabled = true;
                this.Button_order.Enabled = true;
                SqlCommand cmd = new SqlCommand("Select dbo.Get_OrderIdOfTable(@table) id", MY_DB.getConnection);
                cmd.Parameters.Add("@table", SqlDbType.Int).Value = this.tableID;
                DataTable data = MY_DB.getData(cmd);
                if (data.Rows.Count > 0)
                {
                    InterVar.orderID = int.Parse(data.Rows[0]["id"].ToString());
                }
                else
                {
                    InterVar.orderID = -1;
                }
                LoadOrdered();
                TotalAll();

            }

        }


        private void button_book_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you Want To Book This Table ?", "Book Table", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {

                InterVar.orderID = ORDER.CreateOrder(2, USER.getUserByEmail("admin"), tableID);
                if (InterVar.orderID != -1)
                {
                    if (TABLE.changeStatus(this.tableID, 1) != true)
                    {
                        ORDER.deleteOrder(InterVar.orderID);
                    }
                    else
                    {
                        this.LoadData();
                    }
                }
                else
                {
                    MessageBox.Show("Can't create the order !!!", "Book Table", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Button_Pay_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you Want To Pay This Table ?", "Payment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                if (ORDER.PayOrder(InterVar.orderID) == true)
                {
                    TABLE.changeStatus(tableID, 0);
                    SALES.InsertSales(DateTime.Today, InterVar.orderID);
                    InterVar.orderID = 0;

                    //result = MessageBox.Show("Do you Want To Print The Bill ?", "Payment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    //if (result == DialogResult.Yes)
                    //{
                    //    SaveFileDialog savefile = new SaveFileDialog();
                    //    savefile.DefaultExt = "*.pdf";
                    //    savefile.Filter = "PDF files (*.pdf)| *.pdf";

                    //    if (savefile.ShowDialog() == DialogResult.OK && savefile.FileName.Length > 0)
                    //    {
                    //        Export_Data_To_Word(this.dataGridView_Ordered, savefile.FileName);
                    //        MessageBox.Show("File saved!", "Message Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //    }
                    //}

                    LoadData();
                    dataGridView_Ordered.DataSource = null;
                    dataGridView_Ordered.Rows.Clear();
                    label_Total.Text = "0";
                }
                else
                {
                    MessageBox.Show("Error While Paying !!!", "Payment", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /*
        public void Export_Data_To_Word(DataGridView DGV, string filename)
        {
            Document oDoc = new Document();
            oDoc.Application.Visible = false;
            oDoc.PageSetup.Orientation = WdOrientation.wdOrientLandscape;
            object missing = System.Reflection.Missing.Value;
            DGV.Columns.RemoveAt(2);
            DGV.Columns.RemoveAt(0);
            if (DGV.Rows.Count != 0)
            {
                int RowCount = DGV.Rows.Count;
                int ColumnCount = DGV.Columns.Count;
                Object[,] DataArray = new object[RowCount + 1, ColumnCount + 1];
                int r = 0;
                for (int c = 0; c <= ColumnCount - 1; c++)
                    for (r = 0; r < RowCount; r++)
                    {
                        DataArray[r, c] = DGV.Rows[r].Cells[c].Value;
                    }
                dynamic oRange = oDoc.Content.Application.Selection.Range;
                string oTemp = "";
                for (r = 0; r < RowCount; r++)
                    for (int c = 0; c < ColumnCount; c++)
                        oTemp = oTemp + DataArray[r, c] + "\t";
                oRange.Text = oTemp;
                object Separator = WdTableFieldSeparator.wdSeparateByTabs;
                object ApplyBorders = true;
                object AutoFit = true;
                object AutoFitBehavior = WdAutoFitBehavior.wdAutoFitContent;
                oRange.ConvertToTable(ref Separator, ref RowCount, ref ColumnCount,
                                    Type.Missing, Type.Missing, ref ApplyBorders,
                                    Type.Missing, Type.Missing, Type.Missing,
                                    Type.Missing, Type.Missing, Type.Missing,
                                    Type.Missing, ref AutoFit, ref AutoFitBehavior, Type.Missing);
                oRange.Select();
                oDoc.Application.Selection.Tables[1].Select();
                oDoc.Application.Selection.Tables[1].Rows.AllowBreakAcrossPages = 0;
                oDoc.Application.Selection.Tables[1].Rows.Alignment = 0;
                oDoc.Application.Selection.Tables[1].Rows[1].Select();
                oDoc.Application.Selection.InsertRowsAbove(1);
                oDoc.Application.Selection.Tables[1].Rows[1].Select();
                oDoc.Application.Selection.Tables[1].Rows[1].Range.Bold = 1;
                oDoc.Application.Selection.Tables[1].Rows[1].Range.Font.Name = "Times New Roman";
                oDoc.Application.Selection.Tables[1].Rows[1].Range.Font.Size = 16;
                for (int c = 0; c < ColumnCount; c++)
                    oDoc.Application.Selection.Tables[1].Cell(1, c + 1).Range.Text = DGV.Columns[c].HeaderText;
                oDoc.Application.Selection.Tables[1].set_Style("Grid Table 4 - Accent 5");
                oDoc.Application.Selection.Tables[1].Rows[1].Select();
                oDoc.Application.Selection.Cells.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                oDoc.Application.Selection.Tables[1].Select();
                oDoc.Application.Selection.Cut();
            }

            Paragraph paraHeading = oDoc.Content.Paragraphs.Add(ref missing);
            paraHeading.Range.Text = "Godzilla Restaurant";
            paraHeading.Range.Font.Size = 25;
            paraHeading.Range.Font.Name = "Times New Roman";
            paraHeading.Range.Bold = 1;
            paraHeading.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
            paraHeading.Range.Font.Color = Microsoft.Office.Interop.Word.WdColor.wdColorBlack;
            paraHeading.Range.InsertParagraphAfter();

            paraHeading.Range.Text = "Order Bill";
            paraHeading.Range.Font.Size = 20;
            paraHeading.Range.Font.Name = "Times New Roman";
            paraHeading.Range.Bold = 1;
            paraHeading.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
            paraHeading.Range.Font.Color = Microsoft.Office.Interop.Word.WdColor.wdColorBlack;
            paraHeading.Range.InsertParagraphAfter();
            paraHeading.Range.Font.Size = 16;
            paraHeading.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            paraHeading.Range.Text = this.label_ID.Text + "\n" +
                                     this.label_Chair.Text + "\n" +
                                     "Order Table: ";
            paraHeading.Range.InsertParagraphAfter();
            object start = oDoc.Content.End - 1;
            object end = oDoc.Content.End;
            Range rng = oDoc.Range(ref start, ref end);
            rng.Select();
            rng.Paste();
            paraHeading.Range.Text = label2.Text + " " + label_Total.Text;
            paraHeading.Range.Font.Size = 30;
            paraHeading.Range.Font.Name = "Times New Roman";
            paraHeading.Range.Bold = 1;
            paraHeading.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
            paraHeading.Range.InsertParagraphAfter();
            oDoc.Application.Visible = false;
            oDoc.SaveAs2(filename, WdExportFormat.wdExportFormatPDF);
            object saveOptionsObject = WdSaveOptions.wdDoNotSaveChanges;
            oDoc.Close(ref saveOptionsObject, ref missing, ref missing);
        }
        */

        public void LoadOrdered()
        {
            dataGridView_Ordered.DataSource = ORDER.getOrdered(InterVar.orderID);
            dataGridView_Ordered.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView_Ordered.AllowUserToAddRows = false;
            dataGridView_Ordered.Columns["fid"].Visible = false;
            dataGridView_Ordered.Columns["tid"].Visible = false;
            dataGridView_Ordered.Columns["name"].Width = 300;
            dataGridView_Ordered.Columns["cost"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView_Ordered.Columns["amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView_Ordered.Columns["total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView_Ordered.Columns["name"].HeaderText = "NAME";
            dataGridView_Ordered.Columns["type"].HeaderText = "TYPE";
            dataGridView_Ordered.Columns["cost"].HeaderText = "COST";
            dataGridView_Ordered.Columns["amount"].HeaderText = "AMOUNT";
            dataGridView_Ordered.Columns["total"].HeaderText = "TOTAL";
        }


        private void Button_order_Click(object sender, EventArgs e)
        {
            OrderForm f = new OrderForm();
            f.ShowDialog();
            LoadOrdered();
            TotalAll();
        }

        private void Button_Confirm_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void TotalAll()
        {

            try
            {
                label_Total.Text = ORDER.getPaymentOfOrder(InterVar.orderID).ToString();
            }
            catch
            {

            }

        }
    }
}
