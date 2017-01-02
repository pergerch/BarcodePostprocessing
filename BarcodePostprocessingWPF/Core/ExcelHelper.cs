﻿namespace BarcodePostprocessingWPF.Core
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using BarcodePostprocessingWPF.Model;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

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
                    workSheet.Cells[i, lastColumn + 1].Style.Border.Left.Style = ExcelBorderStyle.Double;
                    workSheet.Cells[i, lastColumn + 1].Style.Border.Left.Color.SetColor(Color.Black);

                    workSheet.Cells[i, lastColumn + 2].Value = current;
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
                if (sumBarcodes.Count > 0)
                {
                    int nextRow = workSheet.Dimension.End.Row;
                    foreach (KeyValuePair<string, int> pair in sumBarcodes)
                    {
                        nextRow++;
                        workSheet.Cells[nextRow, lastColumn + 1].Value = pair.Key;
                        workSheet.Cells[nextRow, lastColumn + 1].Style.Border.Left.Style = ExcelBorderStyle.Double;
                        workSheet.Cells[nextRow, lastColumn + 1].Style.Border.Left.Color.SetColor(Color.Black);
                        workSheet.Cells[nextRow, lastColumn + 2].Value = pair.Value;
                        workSheet.Cells[nextRow, lastColumn + 3].Value = pair.Value;
                        workSheet.Cells[nextRow, lastColumn + 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells[nextRow, lastColumn + 3].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
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
            catch (IOException)
            {
                MessageBox.Show("Cannot access file: " + filename, "IO Exception", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}