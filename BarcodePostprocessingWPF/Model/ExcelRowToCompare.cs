namespace BarcodePostprocessingWPF.Model
{
    public class ExcelRowToCompare
    {
        public double? Count { get; set; }

        public string Filename { get; set; }

        public string InternalCode { get; set; }

        public bool IsHeader { get; set; }

        public int Row { get; set; }
    }
}