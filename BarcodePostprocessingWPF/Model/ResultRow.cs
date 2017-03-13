namespace BarcodePostprocessingWPF.Model
{
	using System.Collections.Generic;

	public class ResultRow
	{
		public List<double> AdditionalCounts { get; set; }

		public string Filename { get; set; }

		public bool IsHeader { get; set; }

		public int SourceRow { get; set; }

		public int TargetRow { get; set; }
	}
}