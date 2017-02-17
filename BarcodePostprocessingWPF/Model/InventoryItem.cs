namespace BarcodePostprocessingWPF.Model
{
    // TODO: Refactor to interface/base class with two implementation types BarcodeItem, InternalCodeItem
    // Then use .OfType in LINQ statements
    public class InventoryItem
    {
        public string Barcode { get; set; }

        public int Count { get; set; }

        public string InternalCode { get; set; }
    }
}