using System.Text;
using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig;

namespace WebApp.Infrastructure.Helpers
{
    public class FilesTextExtracter
    {
        public static string ExtractTextFromPdf(Stream inputStream)
        {
            if (inputStream == null)
                throw new ArgumentException("PDF stream is null.");

            // Copy the input stream to a MemoryStream if it doesn't support seeking
            MemoryStream pdfStream;
            if (!inputStream.CanSeek)
            {
                pdfStream = new MemoryStream();
                inputStream.CopyTo(pdfStream);
                pdfStream.Position = 0;
            }
            else
            {
                pdfStream = inputStream as MemoryStream ?? new MemoryStream();
                if (pdfStream.Length == 0)
                {
                    inputStream.Position = 0;
                    inputStream.CopyTo(pdfStream);
                    pdfStream.Position = 0;
                }
            }

            if (pdfStream.Length == 0)
                throw new ArgumentException("PDF stream is empty.");

            // Read first 4 bytes
            byte[] headerBytes = new byte[4];
            pdfStream.Read(headerBytes, 0, 4);
            var pdfHeader = System.Text.Encoding.ASCII.GetString(headerBytes);

            // Reset stream position
            pdfStream.Position = 0;

            if (!pdfHeader.StartsWith("%PDF"))
                throw new InvalidDataException("The file is not a valid PDF.");

            using var pdf = PdfDocument.Open(pdfStream);
            var sb = new StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                sb.AppendLine(page.Text);
            }
            return sb.ToString();
        }



        public static string ExtractTextFromWord(Stream stream)
        {
            //using var stream = new MemoryStream(docxBytes);
            using var wordDoc = WordprocessingDocument.Open(stream, false);
            return wordDoc.MainDocumentPart?.Document?.InnerText ?? string.Empty;
        }

        private static byte[] StreamToBytes(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
