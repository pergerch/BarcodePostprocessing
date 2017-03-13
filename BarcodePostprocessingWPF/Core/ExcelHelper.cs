namespace BarcodePostprocessingWPF.Core
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Windows;
	using BarcodePostprocessingWPF.Model;
	using BarcodePostprocessingWPF.Repository;
	using Microsoft.HockeyApp;
	using OfficeOpenXml;
	using OfficeOpenXml.Style;

	public class ExcelHelper
	{
		public static void CompareSumWithOfficial(string filename, Inventory inventory, List<int> barcodeColumns,
			int internalCodeColumn, int numColumn, int priceColumn, bool? skipFirstRow = null)
		{
			FileInfo fileInfo = new FileInfo(filename);
			try
			{
				ExcelPackage package = new ExcelPackage(fileInfo);
				ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

				int lastColumn = workSheet.Dimension.End.Column;
				int firstRow = (skipFirstRow == true) ? workSheet.Dimension.Start.Row + 1 : workSheet.Dimension.Start.Row;

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
							$"Error in row {i}. Column 'Count' is empty or not a number: '{workSheet.Cells[i, numColumn].Value}'.{Environment.NewLine}" +
							$"File: {fileInfo.Name}");
					}
					if (!double.TryParse(workSheet.Cells[i, priceColumn].Value?.ToString(), out price))
					{
						throw new DataException(
							$"Error in row {i}. Column 'Price' is empty or not a number: '{workSheet.Cells[i, priceColumn].Value}'.{Environment.NewLine}" +
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

					double matches = inventory.GetMatches(barcodes, internalCode, num);

					double variance = matches - num;
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
				MessageBox.Show("Cannot access file: " + fileInfo.Name, "IO Exception", MessageBoxButton.OK, MessageBoxImage.Error);
				((HockeyClient)HockeyClient.Current).HandleException(ex);
			}
		}

		public static void CopyResultRows(string filename, string sheetName, List<ResultRow> resultRows)
		{
			FileInfo fileInfo = new FileInfo(filename);
			try
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}

				ExcelPackage package = new ExcelPackage(fileInfo);
				ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(sheetName);

				ResultRow headerRow = resultRows.First(x => x.IsHeader);

				using (ExcelPackage packageFrom = new ExcelPackage(new FileInfo(headerRow.Filename)))
				{
					ExcelWorksheet workSheetFrom = packageFrom.Workbook.Worksheets[1];
					workSheetFrom.Cells[1, 1, 1, 999].Copy(workSheet.Cells[1, 1, 1, 999]);
				}

				List<string> files = resultRows.Select(x => x.Filename).Distinct().ToList();

				foreach (string file in files)
				{
					using (ExcelPackage packageFrom = new ExcelPackage(new FileInfo(file)))
					{
						foreach (ResultRow resultRow in resultRows.Where(x => x.Filename == file))
						{
							ExcelWorksheet workSheetFrom = packageFrom.Workbook.Worksheets[1];
							workSheetFrom.Cells[resultRow.SourceRow, 1, resultRow.SourceRow, 999].Copy(
								workSheet.Cells[resultRow.TargetRow, 1, resultRow.TargetRow, 999]);

							if (resultRow.AdditionalCounts != null && resultRow.AdditionalCounts.Count > 0)
							{
								int lastColumn = workSheet.Dimension.End.Column;
								workSheet.Cells[resultRow.TargetRow, 1, resultRow.TargetRow, lastColumn].Style.Fill.PatternType =
									ExcelFillStyle.Solid;
								workSheet.Cells[resultRow.TargetRow, 1, resultRow.TargetRow, lastColumn].Style.Fill.BackgroundColor.SetColor(
									Color.DeepSkyBlue);

								foreach (double additionalCount in resultRow.AdditionalCounts)
								{
									workSheet.Cells[resultRow.TargetRow, ++lastColumn].Value = additionalCount;
								}
							}
						}
					}
				}

				package.Save();
			}
			catch (IOException ex)
			{
				MessageBox.Show("Cannot access file: " + fileInfo.Name, "IO Exception", MessageBoxButton.OK, MessageBoxImage.Error);
				((HockeyClient)HockeyClient.Current).HandleException(ex);
			}
		}

		public static Inventory ReadBarcodeAndCountFromExcelFile(Inventory inventory, string filename, int barcodeColumn,
			int internalCodeColumn, int numColumn, bool? skipFirstRow = null)
		{
			FileInfo fileInfo = new FileInfo(filename);
			try
			{
				ExcelPackage package = new ExcelPackage(fileInfo);
				ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

				int firstRow = (skipFirstRow == true) ? workSheet.Dimension.Start.Row + 1 : workSheet.Dimension.Start.Row;

				for (int i = firstRow; i <= workSheet.Dimension.End.Row; i++)
				{
					string barcode = workSheet.Cells[i, barcodeColumn].Value?.ToString().Trim();
					string internalCode = workSheet.Cells[i, internalCodeColumn].Value?.ToString().Trim();

					double num;
					if (!double.TryParse(workSheet.Cells[i, numColumn].Value?.ToString(), out num))
					{
						throw new DataException(
							$"Error in row {i}. Column 'Count' is empty or not a number: '{workSheet.Cells[i, numColumn].Value}'.{Environment.NewLine}" +
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
						throw new DataException($"Error in row {i}. Neither barcode nor internal code is set.{Environment.NewLine}" +
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
				MessageBox.Show("Cannot access file: " + fileInfo.Name, "IO Exception", MessageBoxButton.OK, MessageBoxImage.Error);
				((HockeyClient)HockeyClient.Current).HandleException(ex);
				return null;
			}
		}

		public static Dictionary<string, string> ReadFirstRowFromExcelFile(string filename)
		{
			FileInfo fileInfo = new FileInfo(filename);
			try
			{
				Dictionary<string, string> values = new Dictionary<string, string>();
				ExcelPackage package = new ExcelPackage(fileInfo);
				ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

				int firstRow = workSheet.Dimension.Start.Row;
				for (int i = workSheet.Dimension.Start.Column; i <= workSheet.Dimension.End.Column; i++)
				{
					values.Add(Helper.ExcelColumnFromNumber(i), workSheet.Cells[firstRow, i].Value?.ToString().Trim());
				}

				return values;
			}
			catch (IOException ex)
			{
				MessageBox.Show("Cannot access file: " + fileInfo.Name, "IO Exception", MessageBoxButton.OK, MessageBoxImage.Error);
				((HockeyClient)HockeyClient.Current).HandleException(ex);
				return null;
			}
		}

		public static List<ExcelRowToCompare> ReadRowsFromExcelFile(string filename, int internalCodeColumn,
			bool? skipFirstRow = null)
		{
			FileInfo fileInfo = new FileInfo(filename);
			try
			{
				List<ExcelRowToCompare> rows = new List<ExcelRowToCompare>();
				ExcelPackage package = new ExcelPackage(fileInfo);
				ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

				int firstRow = workSheet.Dimension.Start.Row;
				if (skipFirstRow == true)
				{
					rows.Add(new ExcelRowToCompare { IsHeader = true, Row = firstRow, Filename = filename });
					firstRow++;
				}

				for (int i = firstRow; i <= workSheet.Dimension.End.Row; i++)
				{
					double num;
					if (!double.TryParse(workSheet.Cells[i, internalCodeColumn + 1].Value?.ToString(), out num))
					{
						throw new DataException(
							$"Error in row {i}. Column 'Count' is empty or not a number: '{workSheet.Cells[i, internalCodeColumn + 1].Value}'.{Environment.NewLine}" +
							$"File: {fileInfo.Name}");
					}

					string internalCode = workSheet.Cells[i, internalCodeColumn].Value?.ToString().Trim();
					if (!string.IsNullOrEmpty(internalCode))
					{
						rows.Add(new ExcelRowToCompare { InternalCode = internalCode, Count = num, Row = i, Filename = filename });
					}
					else
					{
						throw new DataException(
							$"Error in row {i}. Column 'Internal Code' is empty or not a number: '{workSheet.Cells[i, internalCodeColumn].Value}'.{Environment.NewLine}" +
							$"File: {fileInfo.Name}");
					}
				}

				return rows;
			}
			catch (DataException ex)
			{
				MessageBox.Show(ex.Message, "Data Exception", MessageBoxButton.OK, MessageBoxImage.Error);
				((HockeyClient)HockeyClient.Current).HandleException(ex);
				return null;
			}
			catch (IOException ex)
			{
				MessageBox.Show("Cannot access file: " + fileInfo.Name, "IO Exception", MessageBoxButton.OK, MessageBoxImage.Error);
				((HockeyClient)HockeyClient.Current).HandleException(ex);
				return null;
			}
		}

		public static void WriteCollectionToExcelFile(string filename, Inventory inventory, string sheetName)
		{
			FileInfo fileInfo = new FileInfo(filename);
			try
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}

				ExcelPackage package = new ExcelPackage(fileInfo);
				ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(sheetName);

				workSheet.Cells["A1"].LoadFromCollection(inventory.Array);

				package.Save();
			}
			catch (IOException ex)
			{
				MessageBox.Show("Cannot access file: " + fileInfo.Name, "IO Exception", MessageBoxButton.OK, MessageBoxImage.Error);
				((HockeyClient)HockeyClient.Current).HandleException(ex);
			}
		}

		public static List<ResultRow> WriteComparedSumsToExcelFile(List<ExcelRowToCompare> allItems)
		{
			List<ResultRow> resultRows = new List<ResultRow>();

			int currentRowIndex = 1;
			if (allItems.Any(x => x.IsHeader))
			{
				ExcelRowToCompare row = allItems.First(x => x.IsHeader);
				resultRows.Add(new ResultRow
				{
					Filename = row.Filename,
					SourceRow = row.Row,
					TargetRow = currentRowIndex,
					IsHeader = true
				});

				allItems.RemoveAll(x => x.IsHeader);
				currentRowIndex++;
			}

			List<string> internalCodes = allItems.Select(x => x.InternalCode).Distinct().ToList();

			foreach (string internalCode in internalCodes)
			{
				List<ExcelRowToCompare> rows = allItems.Where(x => x.InternalCode == internalCode).ToList();
				ExcelRowToCompare row;
				List<double> additionalCounts = null;
				bool multipleOccurance = false;

				// How many rows with a non-zero count?
				switch (rows.Count(x => x.Count != 0))
				{
					case 0:
						// TODO: This could be an issue if the stock has changed in the meantime
						row = rows.First();
						resultRows.Add(new ResultRow { Filename = row.Filename, SourceRow = row.Row, TargetRow = currentRowIndex });

						break;
					case 1:
						row = rows.Single(x => x.Count > 0);
						resultRows.Add(new ResultRow { Filename = row.Filename, SourceRow = row.Row, TargetRow = currentRowIndex });

						break;
					default:
						row = rows.First(x => x.Count > 0);
						string file = row.Filename;
						additionalCounts =
							rows.Where(x => x.Count > 0 && x.Count != null && x.Filename != file).Select(x => x.Count.Value).ToList();

						resultRows.Add(new ResultRow
						{
							Filename = row.Filename,
							SourceRow = row.Row,
							TargetRow = currentRowIndex,
							AdditionalCounts = additionalCounts
						});

						break;
				}

				currentRowIndex++;
			}

			return resultRows;
		}
	}
}