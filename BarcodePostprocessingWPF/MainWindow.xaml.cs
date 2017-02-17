namespace BarcodePostprocessingWPF
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using BarcodePostprocessingWPF.Core;
    using BarcodePostprocessingWPF.Model;
    using BarcodePostprocessingWPF.Repository;
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

                Dictionary<string, string> columns =
                    ExcelHelper.ReadFirstRowFromExcelFile(this.viewModel.ComparedFiles.First());
                this.viewModel.ComparedFileColumns.Clear();
                foreach (KeyValuePair<string, string> column in columns ?? new Dictionary<string, string>())
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

                Dictionary<string, string> columns =
                    ExcelHelper.ReadFirstRowFromExcelFile(this.viewModel.RawFiles.First());
                this.viewModel.RawFileColumns.Clear();
                foreach (KeyValuePair<string, string> column in columns ?? new Dictionary<string, string>())
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

            Inventory inventory = ExcelHelper.ReadBarcodeAndCountFromExcelFile(new Inventory(),
                this.viewModel.RawSummedFileName, 1, 2, 3);

            List<int> barcodeColumns =
                this.OfficialBarcodeList.Items.Cast<KeyValuePair<string, string>>()
                    .Select(x => Helper.NumberFromExcelColumn(x.Key))
                    .ToList();
            int internalCodeColumn =
                Helper.NumberFromExcelColumn(
                    ((KeyValuePair<string, string>)this.OfficialInternalCodeColumnBox.SelectedItem).Key);
            int countColumn =
                Helper.NumberFromExcelColumn(
                    ((KeyValuePair<string, string>)this.OfficialCountColumnBox.SelectedItem).Key);
            int priceColumn =
                Helper.NumberFromExcelColumn(
                    ((KeyValuePair<string, string>)this.OfficialPriceColumnBox.SelectedItem).Key);

            ExcelHelper.CompareSumWithOfficial(fileName, inventory, barcodeColumns, internalCodeColumn, countColumn, priceColumn,
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

            int barcodeColumn =
                Helper.NumberFromExcelColumn(
                    ((KeyValuePair<string, string>)this.CompareBarcodeColumnBox.SelectedItem).Key);

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
            Inventory inventory = new Inventory();
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

            int barcodeColumn =
                Helper.NumberFromExcelColumn(
                    ((KeyValuePair<string, string>)this.RawBarcodeColumnBox.SelectedItem).Key);
            int internalCodeColumn =
                Helper.NumberFromExcelColumn(
                    ((KeyValuePair<string, string>)this.RawInternalCodeColumnBox.SelectedItem).Key);
            int countColumn =
                Helper.NumberFromExcelColumn(((KeyValuePair<string, string>)this.RawCoundColumnBox.SelectedItem).Key);

            foreach (string item in this.viewModel.RawFiles)
            {
                inventory = ExcelHelper.ReadBarcodeAndCountFromExcelFile(inventory, item, barcodeColumn,
                    internalCodeColumn, countColumn, this.RawFileSkipHeaderCheckbox.IsChecked);
            }

            if (inventory != null && inventory.Count > 0)
            {
                ExcelHelper.WriteCollectionToExcelFile(fileName, inventory, "Summed");

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

        private void OfficialBarcodeAdd_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.OfficialBarcodeColumnBox.SelectedItem != null)
            {
                KeyValuePair<string, string> item =
                    (KeyValuePair<string, string>)this.OfficialBarcodeColumnBox.SelectedItem;
                if (
                    this.OfficialBarcodeList.Items.Cast<KeyValuePair<string, string>>()
                        .Any(currentItem => item.Key == currentItem.Key))
                {
                    // If the key already exists in the list then skip the "add"
                    return;
                }

                this.OfficialBarcodeList.Items.Add(item);
            }
        }

        private void OfficialBarcodeRemove_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.OfficialBarcodeList.SelectedItem != null)
            {
                this.OfficialBarcodeList.Items.Remove(this.OfficialBarcodeList.SelectedItem);
            }
        }

        private void OfficialFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog { Multiselect = true, Filter = "Excel file|*.xlsx" };

            if (openDialog.ShowDialog() == true)
            {
                this.viewModel.OfficialFileName = openDialog.FileName;

                Dictionary<string, string> columns =
                    ExcelHelper.ReadFirstRowFromExcelFile(this.viewModel.OfficialFileName);
                this.viewModel.OfficialFileColumns.Clear();
                foreach (KeyValuePair<string, string> column in columns ?? new Dictionary<string, string>())
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