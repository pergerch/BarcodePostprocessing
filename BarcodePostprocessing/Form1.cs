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
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BarcodePostprocessing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnAddInputFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Multiselect = true;
            openDialog.Filter = "Excel file|*.xlsx|All files|*.*";

            if (openDialog.ShowDialog() == DialogResult.OK && openDialog.FileNames.Length > 0)
            {
                lstInputFiles.Items.AddRange(openDialog.FileNames);
            }
        }

        private void btnMergeAndSave_Click(object sender, EventArgs e)
        {
            Dictionary<string, int> stock = new Dictionary<string, int>();
            string fileName;

            if (lstInputFiles.Items.Count <= 0)
            {
                MessageBox.Show("Please select the input files first.", "Input missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel file|*.xlsx";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveDialog.FileName;
                txtMergedDataInput.Text = fileName;
            }
            else
            {
                return;
            }

            foreach (var item in lstInputFiles.Items)
            {
                stock = ReadExcelFile(stock, item.ToString(), 1, 2);
            }

            if (stock.Count > 0)
            {
                var result = stock.OrderBy(x => x.Key);
                var package = new ExcelPackage(new FileInfo(fileName));

                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add("Merged");

                workSheet.Cells["A1"].LoadFromCollection(result);

                package.Save();

                MessageBox.Show("Merging done and stored.", "Merging successful", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private Dictionary<string, int> ReadExcelFile(Dictionary<string, int> stock, string filename, int barcodeColumn, int numColumn)
        {
            var package = new ExcelPackage(new FileInfo(filename));

            ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

            for (int j = workSheet.Dimension.Start.Row; j <= workSheet.Dimension.End.Row; j++)
            {
                string barcode = workSheet.Cells[j, barcodeColumn].Value.ToString().Trim();
                int num = int.Parse(workSheet.Cells[j, numColumn].Value.ToString());

                if (stock.ContainsKey(barcode))
                {
                    int current = stock[barcode];
                    current += num;
                    stock[barcode] = current;
                }
                else
                {
                    stock.Add(barcode, num);
                }
            }

            return stock;
        }

        private void btnOpenMergedData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Excel file|*.xlsx|All files|*.*";

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                txtMergedDataInput.Text = openDialog.FileName;
            }
        }

        private void btnOpenOfficialData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Excel file|*.xlsx|All files|*.*";

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                txtOfficialDataInput.Text = openDialog.FileName;
            }
        }

        private void btnCompareAndSave_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtMergedDataInput.Text) || !File.Exists(txtOfficialDataInput.Text))
            {
                return;
            }

            string fileName;

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel file|*.xlsx";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveDialog.FileName;
                File.Copy(txtOfficialDataInput.Text, fileName);
            }
            else
            {
                return;
            }

            Dictionary<string, int> stock = ReadExcelFile(new Dictionary<string, int>(), txtMergedDataInput.Text, 1, 2);

            var package = new ExcelPackage(new FileInfo(fileName));

            ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

            for (int j = workSheet.Dimension.Start.Row; j <= workSheet.Dimension.End.Row; j++)
            {
                string barcode = workSheet.Cells[j, 1].Value.ToString().Trim();
                int num = int.Parse(workSheet.Cells[j, 3].Value.ToString());
                double price = double.Parse(workSheet.Cells[j, 4].Value.ToString());

                int current = 0;
                if (stock.ContainsKey(barcode))
                {
                    current = stock[barcode];
                    stock.Remove(barcode);
                }
                int variance = current - num;
                workSheet.Cells[j, 5].Value = barcode;
                workSheet.Cells[j, 6].Value = current;
                workSheet.Cells[j, 7].Value = variance;
                workSheet.Cells[j, 8].Value = variance * price;

                workSheet.Cells[j, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                if (variance < 0)
                {
                    workSheet.Cells[j, 7].Style.Fill.BackgroundColor.SetColor(Color.OrangeRed);
                }
                else if (variance > 0)
                {
                    workSheet.Cells[j, 7].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                }
                else
                {
                    workSheet.Cells[j, 7].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                }
            }

            // Any additional values that were not present in the official one
            if (stock.Count > 0)
            {
                int i = workSheet.Dimension.End.Row + 1;
                foreach (KeyValuePair<string, int> pair in stock)
                {
                    workSheet.Cells[i, 5].Value = pair.Key;
                    workSheet.Cells[i, 6].Value = pair.Value;
                    workSheet.Cells[i, 7].Value = pair.Value;
                    workSheet.Cells[i, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i, 7].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                }
            }

            package.Save();

            MessageBox.Show("Comparison done and stored.", "Comparison successful", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
