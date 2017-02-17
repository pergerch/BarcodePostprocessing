namespace BarcodePostprocessingWPF.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BarcodePostprocessingWPF.Model;

    public class Inventory
    {
        public Tuple<string, string, int>[] Array
            =>
                InventoryItems.OrderBy(x => x.InternalCode)
                    .ThenBy(x => x.Barcode)
                    .Select(x => new Tuple<string, string, int>(x.Barcode, x.InternalCode, x.Count))
                    .ToArray();

        public int Count => InventoryItems.Count;

        public ICollection<InventoryItem> RemainingItems => InventoryItems;

        private ICollection<InventoryItem> InventoryItems { get; } = new List<InventoryItem>();

        public void AddBarcodeCount(string barcode, int count)
        {
            InventoryItem item = InventoryItems.FirstOrDefault(x => x.Barcode == barcode);

            if (item == null)
            {
                InventoryItems.Add(new InventoryItem { Barcode = barcode, Count = count });
            }
            else
            {
                item.Count += count;
            }
        }

        public void AddInternalCodeCount(string internalCode, int count)
        {
            InventoryItem item = InventoryItems.FirstOrDefault(x => x.InternalCode == internalCode);

            if (item == null)
            {
                InventoryItems.Add(new InventoryItem { InternalCode = internalCode, Count = count });
            }
            else
            {
                item.Count += count;
            }
        }

        public int GetMatches(List<string> barcodes, string internalCode, int num)
        {
            int matches = 0;

            InventoryItem internalCodeItem = InventoryItems.FirstOrDefault(x => x.InternalCode == internalCode);
            if (internalCodeItem != null)
            {
                matches += internalCodeItem.Count;
                InventoryItems.Remove(internalCodeItem);
            }

            List<InventoryItem> barcodeItems = new List<InventoryItem>();
            foreach (string barcode in barcodes)
            {
                InventoryItem barcodeItem = InventoryItems.FirstOrDefault(x => x.Barcode == barcode);
                if (barcodeItem != null)
                {
                    barcodeItems.Add(barcodeItem);
                    InventoryItems.Remove(barcodeItem);
                }
            }

            matches += barcodeItems.Sum(x => x.Count);

            return matches;
        }
    }
}