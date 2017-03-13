namespace BarcodePostprocessingWPF.Model
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using PropertyChanged;

	[ImplementPropertyChanged]
	public class ViewModel
	{
		public ObservableCollection<KeyValuePair<string, string>> ComparedFileColumns { get; set; } =
			new ObservableCollection<KeyValuePair<string, string>>();

		public ObservableCollection<string> ComparedFiles { get; set; } = new ObservableCollection<string>();

		public ObservableCollection<KeyValuePair<string, string>> OfficialFileColumns { get; set; } =
			new ObservableCollection<KeyValuePair<string, string>>();

		public string OfficialFileName { get; set; }

		public ObservableCollection<KeyValuePair<string, string>> RawFileColumns { get; set; } =
			new ObservableCollection<KeyValuePair<string, string>>();

		public ObservableCollection<string> RawFiles { get; set; } = new ObservableCollection<string>();

		public string RawSummedFileName { get; set; }
	}
}