namespace BarcodePostprocessingWPF.Model
{
    public class ExcelRowToCompare
    {
        public string InternalCode { get; set; }

        public int? Count { get; set; }

        public string Filename { get; set; }

        public bool IsHeader { get; set; }

        public int Row { get; set; }
    }
}