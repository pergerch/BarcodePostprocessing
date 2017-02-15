namespace BarcodePostprocessingWPF.Core
{
    using System;

    public static class Helper
    {
        public static string ExcelColumnFromNumber(int column)
        {
            string columnString = string.Empty;
            while (column > 0)
            {
                int currentLetterNumber = (column - 1) % 26;
                char currentLetter = (char)(currentLetterNumber + 65);
                columnString = currentLetter + columnString;
                column = (column - (currentLetterNumber + 1)) / 26;
            }

            return columnString;
        }

        public static int NumberFromExcelColumn(string column)
        {
            int retVal = 0;
            column = column.ToUpper();
            for (int i = column.Length - 1; i >= 0; i--)
            {
                char colPiece = column[i];
                int colNum = colPiece - 64;
                retVal = retVal + colNum * (int)Math.Pow(26, column.Length - (i + 1));
            }

            return retVal;
        }
    }
}