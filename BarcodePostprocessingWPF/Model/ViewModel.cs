namespace BarcodePostprocessingWPF.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using PropertyChanged;

    [ImplementPropertyChanged]
    public class ViewModel
    {
        public ViewModel()
        {
            RawFiles = new ObservableCollection<string>();
            RawFileColumns = new ObservableCollection<KeyValuePair<int, string>>();
            OfficialFileColumns = new ObservableCollection<KeyValuePair<int, string>>();
        }

        public ObservableCollection<string> ComparedFiles { get; set; }

        public ObservableCollection<KeyValuePair<int, string>> OfficialFileColumns { get; set; }
        public ObservableCollection<KeyValuePair<int, string>> ComparedFileColumns { get; set; }

        public string OfficialFileName { get; set; }

        public ObservableCollection<KeyValuePair<int, string>> RawFileColumns { get; set; }

        public ObservableCollection<string> RawFiles { get; set; }

        public string RawSummedFileName { get; set; }
    }
}