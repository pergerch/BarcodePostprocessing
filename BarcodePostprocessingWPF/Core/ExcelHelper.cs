namespace BarcodePostprocessingWPF.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Documents;
    using BarcodePostprocessingWPF.Model;
    using BarcodePostprocessingWPF.Repository;
    using Microsoft.HockeyApp;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class ExcelHelper
    {
        public static void CompareSumWithOfficial(string filename, Inventory inventory,
            List<int> barcodeColumns, int internalCodeColumn, int numColumn, int priceColumn, bool? skipFirstRow = null)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filename);
                ExcelPackage package = new ExcelPackage(fileInfo);
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

                int lastColumn = workSheet.Dimension.End.Column;
                int firstRow = (skipFirstRow == true)
                    ? workSheet.Dimension.Start.Row + 1 : workSheet.Dimension.Start.Row;

                for (int i = firstRow; i <= workSheet.Dimension.End.Row; i++)
                {
                    if (workSheet.Cells[i, internalCodeColumn].Value == null)
                    {
                        workSheet.Cells[i, internalCodeColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells[i, internalCodeColumn].Style.Fill.BackgroundColor.SetColor(Color.DarkRed);

                        continue;
                    }

                    string internalCode = workSheet.Cells[i, internalCodeColumn].Value?.ToString().Trim();
                    double num, price;
                    if (!double.TryParse(workSheet.Cells[i, numColumn].Value?.ToString(), out num))
                    {
                        throw new DataException(
                            $"Error in row {i}. Column 'Count' is empty or not a number: {workSheet.Cells[i, numColumn].Value}.{Environment.NewLine}" +
                            $"File: {fileInfo.Name}");
                    }
                    if (!double.TryParse(workSheet.Cells[i, priceColumn].Value?.ToString(), out price))
                    {
                        throw new DataException(
                            $"Error in row {i}. Column 'Price' is empty or not a number: {workSheet.Cells[i, priceColumn].Value}.{Environment.NewLine}" +
                            $"File: {fileInfo.Name}");
                    }

                    List<string> barcodes = new List<string>();
                    foreach (int barcodeColumn in barcodeColumns)
                    {
                        string barcode = workSheet.Cells[i, barcodeColumn].Value?.ToString().Trim();
                        if (!string.IsNullOrEmpty(barcode))
                        {
                            barcodes.Add(barcode);
                        }
                    }

                    int matches = inventory.GetMatches(barcodes, internalCode, (int)num);

                    int variance = matches - (int)num;
                    workSheet.Cells[i, lastColumn + 1].Value = internalCode;
                    workSheet.Cells[i, lastColumn + 1].Style.Border.Left.Style = ExcelBorderStyle.Double;
                    workSheet.Cells[i, lastColumn + 1].Style.Border.Left.Color.SetColor(Color.Black);

                    workSheet.Cells[i, lastColumn + 2].Value = matches;
                    workSheet.Cells[i, lastColumn + 3].Value = variance;
                    workSheet.Cells[i, lastColumn + 4].Value = variance * price;
                    if (variance < 0)
                    {
                        workSheet.Cells[i, lastColumn + 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells[i, lastColumn + 3].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                        workSheet.Cells[i, lastColumn + 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells[i, lastColumn + 4].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    }
                    else if (variance > 0)
                    {
                        workSheet.Cells[i, lastColumn + 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells[i, lastColumn + 3].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                        workSheet.Cells[i, lastColumn + 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells[i, lastColumn + 4].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }
                    else
                    {
                        workSheet.Cells[i, lastColumn + 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells[i, lastColumn + 3].Style.Fill.BackgroundColor.SetColor(Color.LimeGreen);
                        workSheet.Cells[i, lastColumn + 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells[i, lastColumn + 4].Style.Fill.BackgroundColor.SetColor(Color.LimeGreen);
                    }
                }

                // Any additional values that were not present in the official one
                int nextRow = workSheet.Dimension.End.Row;
                ICollection<InventoryItem> items = inventory.RemainingItems;
                foreach (InventoryItem item in items ?? new List<InventoryItem>())
                {
                    nextRow++;
                    if (!string.IsNullOrEmpty(item.InternalCode))
                    {
                        workSheet.Cells[nextRow, lastColumn + 1].Value = "i " + item.InternalCode;
                    }
                    else if (!string.IsNullOrEmpty(item.Barcode))
                    {
                        workSheet.Cells[nextRow, lastColumn + 1].Value = item.Barcode;
                    }

                    workSheet.Cells[nextRow, lastColumn + 1].Style.Border.Left.Style = ExcelBorderStyle.Double;
                    workSheet.Cells[nextRow, lastColumn + 1].Style.Border.Left.Color.SetColor(Color.Black);

                    workSheet.Cells[nextRow, lastColumn + 2].Value = item.Count;
                    workSheet.Cells[nextRow, lastColumn + 3].Value = item.Count;
                    workSheet.Cells[nextRow, lastColumn + 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[nextRow, lastColumn + 3].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                }

                package.Save();
            }
            catch (DataException ex)
            {
                MessageBox.Show(ex.Message, "Data Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                ((HockeyClient)HockeyClient.Current).HandleException(ex);
            }
            catch (IOException ex)
            {
                MessageBox.Show("Cannot access file: " + filename, "IO Exception", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                ((HockeyClient)HockeyClient.Current).HandleException(ex);
            }
        }

        public static Inventory ReadBarcodeAndCountFromExcelFile(Inventory inventory, string filename, int barcodeColumn,
            int internalCodeColumn, int numColumn, bool? skipFirstRow = null)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filename);
                ExcelPackage package = new ExcelPackage(fileInfo);
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

                int firstRow = (skipFirstRow == true)
                    ? workSheet.Dimension.Start.Row + 1 : workSheet.Dimension.Start.Row;

                for (int i = firstRow; i <= workSheet.Dimension.End.Row; i++)
                {
                    string barcode = workSheet.Cells[i, barcodeColumn].Value?.ToString().Trim();
                    string internalCode = workSheet.Cells[i, internalCodeColumn].Value?.ToString().Trim();

                    int num;
                    if (!int.TryParse(workSheet.Cells[i, numColumn].Value?.ToString(), out num))
                    {
                        throw new DataException(
                            $"Error in row {i}. Column 'Count' is empty or not a number: {workSheet.Cells[i, numColumn].Value}.{Environment.NewLine}" +
                            $"File: {fileInfo.Name}");
                    }

                    if (!string.IsNullOrEmpty(barcode))
                    {
                        inventory.AddBarcodeCount(barcode, num);
                    }
                    else if (!string.IsNullOrEmpty(internalCode))
                    {
                        inventory.AddInternalCodeCount(internalCode, num);
                    }
                    else
                    {
                        throw new DataException(
                            $"Error in row {i}. Neither barcode nor internal code is set.{Environment.NewLine}" +
                            $"File: {fileInfo.Name}");
                    }
                }

                return inventory;
            }
            catch (DataException ex)
            {
                MessageBox.Show(ex.Message, "Data Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                ((HockeyClient)HockeyClient.Current).HandleException(ex);
                return null;
            }
            catch (IOException ex)
            {
                MessageBox.Show("Cannot access file: " + filename, "IO Exception", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                ((HockeyClient)HockeyClient.Current).HandleException(ex);
                return null;
            }
        }

        public static Dictionary<string, string> ReadFirstRowFromExcelFile(string filename)
        {
            try
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                ExcelPackage package = new ExcelPackage(new FileInfo(filename));
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

                int firstRow = workSheet.Dimension.Start.Row;
                for (int i = workSheet.Dimension.Start.Column; i <= workSheet.Dimension.End.Column; i++)
                {
                    values.Add(Helper.ExcelColumnFromNumber(i), workSheet.Cells[firstRow, i].Value.ToString().Trim());
                }

                return values;
            }
            catch (IOException ex)
            {
                MessageBox.Show("Cannot access file: " + filename, "IO Exception", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                ((HockeyClient)HockeyClient.Current).HandleException(ex);
                return null;
            }
        }

        public static List<ExcelRowToCompare> ReadRowsFromExcelFile(string filename, int barcodeColumn,
            bool? skipFirstRow = null)
        {
            try
            {
                List<ExcelRowToCompare> rows = new List<ExcelRowToCompare>();
                ExcelPackage package = new ExcelPackage(new FileInfo(filename));
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

                int firstRow = workSheet.Dimension.Start.Row;
                if (skipFirstRow == true)
                {
                    rows.Add(new ExcelRowToCompare { IsHeader = true, Row = firstRow, Filename = filename });
                    firstRow++;
                }

                for (int i = firstRow; i <= workSheet.Dimension.End.Row; i++)
                {
                    rows.Add(new ExcelRowToCompare
                    {
                        Barcode = workSheet.Cells[i, barcodeColumn].Value.ToString().Trim(),
                        Count = int.Parse(workSheet.Cells[i, barcodeColumn + 1].Value.ToString()),
                        Row = i,
                        Filename = filename
                    });
                }

                return rows;
            }
            catch (IOException ex)
            {
                MessageBox.Show("Cannot access file: " + filename, "IO Exception", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                ((HockeyClient)HockeyClient.Current).HandleException(ex);
                return null;
            }
        }

        public static void WriteCollectionToExcelFile(string filename, Inventory inventory, string sheetName)
        {
            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                ExcelPackage package = new ExcelPackage(new FileInfo(filename));
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(sheetName);

                workSheet.Cells["A1"].LoadFromCollection(inventory.Array);

                package.Save();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Cannot access file: " + filename, "IO Exception", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                ((HockeyClient)HockeyClient.Current).HandleException(ex);
            }
        }

        public static void WriteComparedSumsToExcelFile(string filename, List<ExcelRowToCompare> allItems,
            string sheetName)
        {
            try
            {
                List<List<string>> result = new List<List<string>>();
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                ExcelPackage package = new ExcelPackage(new FileInfo(filename));
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(sheetName);

                int currentRowIndex = 1;
                if (allItems.Any(x => x.IsHeader))
                {
                    ExcelRowToCompare headerRow = allItems.First(x => x.IsHeader);

                    using (ExcelPackage packageFrom = new ExcelPackage(new FileInfo(headerRow.Filename)))
                    {
                        ExcelWorksheet workSheetFrom = packageFrom.Workbook.Worksheets[1];
                        workSheetFrom.Cells[1, 1, 1, 999].Copy(workSheet.Cells[1, 1, 1, 999]);
                    }

                    allItems.RemoveAll(x => x.IsHeader);
                    currentRowIndex++;
                }

                List<string> barcodes = allItems.Select(x => x.Barcode).Distinct().ToList();

                foreach (string barcode in barcodes)
                {
                    ExcelRowToCompare row =
                        allItems.Where(x => x.Barcode == barcode).SingleOrDefault(x => x.Count > 0) ??
                        allItems.First(x => x.Barcode == barcode);

                    using (ExcelPackage packageFrom = new ExcelPackage(new FileInfo(row.Filename)))
                    {
                        ExcelWorksheet workSheetFrom = packageFrom.Workbook.Worksheets[1];
                        workSheetFrom.Cells[row.Row, 1, row.Row, 999].Copy(
                            workSheet.Cells[currentRowIndex, 1, currentRowIndex, 999]);
                    }

                    currentRowIndex++;
                }

                package.Save();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Cannot access file: " + filename, "IO Exception", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                ((HockeyClient)HockeyClient.Current).HandleException(ex);
            }
        }
    }
}