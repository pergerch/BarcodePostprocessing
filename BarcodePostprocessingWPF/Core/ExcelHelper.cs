namespace BarcodePostprocessingWPF.Core
{
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using OfficeOpenXml;

    public class ExcelHelper
    {
        public static void CompareSumWithOfficial(string filename, Dictionary<string, int> sumBarcodes,
            int barcodeColumn, int numColumn, int priceColumn, bool? skipFirstRow = null)
        {
            try
            {
                ExcelPackage package = new ExcelPackage(new FileInfo(filename));
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

                int lastColumn = workSheet.Dimension.End.Column;
                int firstRow = (skipFirstRow == true)
                    ? workSheet.Dimension.Start.Row + 1 : workSheet.Dimension.Start.Row;

                for (int i = firstRow; i <= workSheet.Dimension.End.Row; i++)
                {
                    string barcode = workSheet.Cells[i, barcodeColumn].Value.ToString().Trim();
                    int num = int.Parse(workSheet.Cells[i, numColumn].Value.ToString());
                    double price = double.Parse(workSheet.Cells[i, priceColumn].Value.ToString());

                    int current = 0;
                    if (sumBarcodes.ContainsKey(barcode))
                    {
                        current = sumBarcodes[barcode];
                        sumBarcodes.Remove(barcode);
                    }
                    int variance = current - num;
                    workSheet.Cells[i, lastColumn + 1].Value = barcode;
                    workSheet.Cells[i, lastColumn + 2].Value = current;
                    workSheet.Cells[i, lastColumn + 3].Value = variance;
                    workSheet.Cells[i, lastColumn + 4].Value = variance * price;
                }

                // Any additional values that were not present in the official one
                if (sumBarcodes.Count > 0)
                {
                    int nextRow = workSheet.Dimension.End.Row + 1;
                    foreach (KeyValuePair<string, int> pair in sumBarcodes)
                    {
                        workSheet.Cells[nextRow, lastColumn + 1].Value = pair.Key;
                        workSheet.Cells[nextRow, lastColumn + 2].Value = pair.Value;
                        workSheet.Cells[nextRow, lastColumn + 3].Value = pair.Value;
                    }
                }

                package.Save();
            }
            catch (IOException)
            {
                MessageBox.Show("Cannot access file: " + filename, "IO Exception", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public static Dictionary<string, int> ReadBarcodeAndCountFromExcelFile(Dictionary<string, int> stock,
            string filename, int barcodeColumn, int numColumn, bool? skipFirstRow = null)
        {
            try
            {
                ExcelPackage package = new ExcelPackage(new FileInfo(filename));
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

                int firstRow = (skipFirstRow == true)
                    ? workSheet.Dimension.Start.Row + 1 : workSheet.Dimension.Start.Row;

                for (int i = firstRow; i <= workSheet.Dimension.End.Row; i++)
                {
                    string barcode = workSheet.Cells[i, barcodeColumn].Value.ToString().Trim();
                    int num = int.Parse(workSheet.Cells[i, numColumn].Value.ToString());

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
            catch (IOException)
            {
                MessageBox.Show("Cannot access file: " + filename, "IO Exception", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return null;
            }
        }

        public static Dictionary<int, string> ReadFirstRowFromExcelFile(string filename)
        {
            try
            {
                Dictionary<int, string> values = new Dictionary<int, string>();
                ExcelPackage package = new ExcelPackage(new FileInfo(filename));
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

                int firstRow = workSheet.Dimension.Start.Row;
                for (int i = workSheet.Dimension.Start.Column; i <= workSheet.Dimension.End.Column; i++)
                {
                    values.Add(i, workSheet.Cells[firstRow, i].Value.ToString().Trim());
                }

                return values;
            }
            catch (IOException)
            {
                MessageBox.Show("Cannot access file: " + filename, "IO Exception", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return null;
            }
        }

        public static void WriteCollectionToExcelFile<T>(string filename, IEnumerable<T> collection, string sheetName)
        {
            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                ExcelPackage package = new ExcelPackage(new FileInfo(filename));
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(sheetName);

                workSheet.Cells["A1"].LoadFromCollection(collection);

                package.Save();
            }
            catch (IOException)
            {
                MessageBox.Show("Cannot access file: " + filename, "IO Exception", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}