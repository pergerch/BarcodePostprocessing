namespace BarcodePostprocessingWPF
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using BarcodePostprocessingWPF.Core;
    using BarcodePostprocessingWPF.Model;
    using Microsoft.Win32;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewModel viewModel = new ViewModel();

        public MainWindow()
        {
            InitializeComponent();
            SetLanguageDictionary();
            DataContext = this.viewModel;
        }

        private void BtnAddComparedDataFiles_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog { Multiselect = true, Filter = "Excel file|*.xlsx" };

            if (openDialog.ShowDialog() == true)
            {
                foreach (string fileName in openDialog.FileNames)
                {
                    this.viewModel.ComparedFiles.Add(fileName);
                }

                Dictionary<int, string> columns =
                    ExcelHelper.ReadFirstRowFromExcelFile(this.viewModel.ComparedFiles.First());
                this.viewModel.ComparedFileColumns.Clear();
                foreach (KeyValuePair<int, string> column in columns)
                {
                    this.viewModel.ComparedFileColumns.Add(column);
                }
            }
        }

        private void BtnAddRawDataFiles_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog { Multiselect = true, Filter = "Excel file|*.xlsx" };

            if (openDialog.ShowDialog() == true)
            {
                foreach (string fileName in openDialog.FileNames)
                {
                    this.viewModel.RawFiles.Add(fileName);
                }

                Dictionary<int, string> columns = ExcelHelper.ReadFirstRowFromExcelFile(this.viewModel.RawFiles.First());
                this.viewModel.RawFileColumns.Clear();
                foreach (KeyValuePair<int, string> column in columns)
                {
                    this.viewModel.RawFileColumns.Add(column);
                }
            }
        }

        private void BtnCompareFiles_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.RawSummedFileName == null || this.viewModel.OfficialFileName == null ||
                !File.Exists(this.viewModel.RawSummedFileName) || !File.Exists(this.viewModel.OfficialFileName))
            {
                return;
            }

            string fileName;

            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "Excel file|*.xlsx" };

            if (saveDialog.ShowDialog() == true)
            {
                fileName = saveDialog.FileName;
                File.Copy(this.viewModel.OfficialFileName, fileName, true);
            }
            else
            {
                return;
            }

            Dictionary<string, int> stock = ExcelHelper.ReadBarcodeAndCountFromExcelFile(new Dictionary<string, int>(),
                this.viewModel.RawSummedFileName, 1, 2);
            int barcodeColumn = ((KeyValuePair<int, string>)this.OfficialBarcodeColumnBox.SelectionBoxItem).Key;
            int countColumn = ((KeyValuePair<int, string>)this.OfficialCountColumnBox.SelectionBoxItem).Key;
            int priceColumn = ((KeyValuePair<int, string>)this.OfficialPriceColumnBox.SelectionBoxItem).Key;

            ExcelHelper.CompareSumWithOfficial(fileName, stock, barcodeColumn, countColumn, priceColumn,
                this.OfficialFileSkipHeaderCheckbox.IsChecked);

            MessageBox.Show(FindResource("ComparisonDoneText").ToString(),
                FindResource("ComparisonDoneCaption").ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSumCompareFiles_OnClick(object sender, RoutedEventArgs e)
        {
            List<ExcelRowToCompare> allItems = new List<ExcelRowToCompare>();
            string fileName;

            if (this.viewModel.ComparedFiles.Count <= 0)
            {
                MessageBox.Show(FindResource("InputMissingText").ToString(),
                    FindResource("InputMissingCaption").ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "Excel file|*.xlsx" };
            if (saveDialog.ShowDialog() == true)
            {
                fileName = saveDialog.FileName;
            }
            else
            {
                return;
            }

            int barcodeColumn = ((KeyValuePair<int, string>)this.CompareBarcodeColumnBox.SelectionBoxItem).Key;

            foreach (string item in this.viewModel.ComparedFiles)
            {
                allItems.AddRange(ExcelHelper.ReadRowsFromExcelFile(item, barcodeColumn,
                    this.CompareFileSkipHeaderCheckbox.IsChecked));
            }

            if (allItems.Count > 0)
            {
                ExcelHelper.WriteComparedSumsToExcelFile(fileName, allItems, "Summed");

                MessageBox.Show(FindResource("SumDoneCaption").ToString(), FindResource("SumDoneText").ToString(),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnSumFiles_OnClick(object sender, RoutedEventArgs e)
        {
            Dictionary<string, int> stock = new Dictionary<string, int>();
            string fileName;

            if (this.viewModel.RawFiles.Count <= 0)
            {
                MessageBox.Show(FindResource("InputMissingText").ToString(),
                    FindResource("InputMissingCaption").ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "Excel file|*.xlsx" };
            if (saveDialog.ShowDialog() == true)
            {
                fileName = saveDialog.FileName;
                this.viewModel.RawSummedFileName = fileName;
            }
            else
            {
                return;
            }

            int barcodeColumn = ((KeyValuePair<int, string>)this.RawBarcodeColumnBox.SelectionBoxItem).Key;
            int countColumn = ((KeyValuePair<int, string>)this.RawCoundColumnBox.SelectionBoxItem).Key;

            foreach (string item in this.viewModel.RawFiles)
            {
                stock = ExcelHelper.ReadBarcodeAndCountFromExcelFile(stock, item, barcodeColumn, countColumn,
                    this.RawFileSkipHeaderCheckbox.IsChecked);
            }

            if (stock.Count > 0)
            {
                IOrderedEnumerable<KeyValuePair<string, int>> result = stock.OrderBy(x => x.Key);
                ExcelHelper.WriteCollectionToExcelFile(fileName, result, "Summed");

                MessageBox.Show(FindResource("SumDoneCaption").ToString(), FindResource("SumDoneText").ToString(),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LanguageBulgarianButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionary(LanguageEnum.Bulgarian);
        }

        private void LanguageEnglishButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionary(LanguageEnum.English);
        }

        private void LanguageGermanButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionary(LanguageEnum.German);
        }

        private void OfficialFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog { Multiselect = true, Filter = "Excel file|*.xlsx" };

            if (openDialog.ShowDialog() == true)
            {
                this.viewModel.OfficialFileName = openDialog.FileName;

                Dictionary<int, string> columns = ExcelHelper.ReadFirstRowFromExcelFile(this.viewModel.OfficialFileName);
                this.viewModel.OfficialFileColumns.Clear();
                foreach (KeyValuePair<int, string> column in columns)
                {
                    this.viewModel.OfficialFileColumns.Add(column);
                }
            }
        }

        private void RawSummedFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog { Multiselect = true, Filter = "Excel file|*.xlsx" };

            if (openDialog.ShowDialog() == true)
            {
                this.viewModel.RawSummedFileName = openDialog.FileName;
            }
        }

        private void SetLanguageDictionary(LanguageEnum? lang = null)
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (lang)
            {
                case LanguageEnum.English:
                    dict.Source = new Uri("..\\Resources\\StringResources.xaml", UriKind.Relative);
                    break;
                case LanguageEnum.Bulgarian:
                    dict.Source = new Uri("..\\Resources\\StringResources.bg.xaml", UriKind.Relative);
                    break;
                case LanguageEnum.German:
                    dict.Source = new Uri("..\\Resources\\StringResources.de.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("..\\Resources\\StringResources.xaml", UriKind.Relative);
                    break;
            }
            Resources.MergedDictionaries.Add(dict);
        }

        private void StatusButtonCopyright_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://spatial-focus.net");
        }
    }
}