using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.IO;
using System.Text;

namespace BGPT.Helpers
{
    public static class PdfTextExtractorHelper
    {
        public static string ExtractTextFromPdf(Stream pdfStream)
        {
            StringBuilder text = new StringBuilder();

            PdfDocument pdfDoc = new PdfDocument(new PdfReader(pdfStream));
            for (int page = 1; page <= pdfDoc.GetNumberOfPages(); ++page)
            {
                text.Append(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page)));
            }

            pdfDoc.Close();

            return text.ToString();
        }
    }
}
