using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace MergePdfs
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var newPdf = CreateNewDocument();

                newPdf.MergeDocuments(new string[] { "pdf01.pdf", "pdf02.pdf" });

                var page = GetPage("pdf01.pdf", 2);

                newPdf.AddPage(page);

                newPdf.AddImage("img01.png");

                newPdf.AddPagination();

                newPdf.Save();
                // var bytes = newPdf.GetBytesFromMemoryStream();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        static PdfPage GetPage(string fileName, int pageIndex)
        {
            try
            {
                var p = Path.Combine(@"C:\Dev\MergePdfs\Assets", fileName);

                if (!File.Exists(p))
                {
                    throw new FileNotFoundException("File not found to get Page", fileName: fileName);
                }

                using var fs = File.OpenRead(p);

                var pdfDocument = PdfReader.Open(fs, PdfDocumentOpenMode.Import);

                if (pdfDocument.PageCount < pageIndex)
                {
                    throw new IndexOutOfRangeException();
                }

                return pdfDocument.Pages[pageIndex];
            }
            catch
            {
                throw;
            }
        }

        static PdfDocument CreateNewDocument() => new();

    }

    public static class Extensions
    {
        public static PdfDocument MergeDocuments(this PdfDocument pdfDocument, IList<string> fileNames)
        {
            try
            {
                foreach (var fileName in fileNames)
                {

                    var file = Path.Combine(@"C:\Dev\MergePdfs\Assets", fileName);
                    if (!File.Exists(file))
                    {
                        throw new FileNotFoundException("File not found to Merge", fileName: fileName);
                    }
                    using var fs = File.OpenRead(file);
                    var inputDocument = PdfReader.Open(fs, PdfDocumentOpenMode.Import);
                    var count = inputDocument.PageCount;
                    for (int i = 0; i < count; i++)
                    {
                        pdfDocument.AddPage(inputDocument.Pages[i]);
                    }
                }

                return pdfDocument;

            }
            catch (Exception)
            {
                throw;
            }

        }

        public static PdfDocument AddImage(this PdfDocument document, string fileName)
        {
            try
            {
                XImage image = XImage.FromFile(Path.Combine(@"C:\Dev\MergePdfs\Assets", fileName));

                if (image == null)
                {
                    throw new FileNotFoundException("Image not found", fileName);
                }

                var pageToImage = document.AddPage();
                using var gfx = XGraphics.FromPdfPage(pageToImage);
                var maxHeight = pageToImage.Height - 110;
                var maxWidth = pageToImage.Width - 110;

                if (image.Size.Width > maxWidth || image.Size.Height > maxHeight)
                {
                    return RisizeToDraw(document, gfx, image, maxHeight, maxWidth);
                }

                gfx.DrawImage(image, 50, 50,
                    image.Size.Width,
                    image.Size.Height);
                return document;
            }
            catch
            {
                throw;
            }

        }

        private static PdfDocument RisizeToDraw(PdfDocument document, XGraphics gfx, XImage image,
            double maxHeight, double maxWidth)
        {
            double newWidth;
            double newHeigth;

            var imageWidth = image.Size.Width;
            var imageHeigth = image.Size.Height;

            if (imageWidth > imageHeigth)
            {
                newWidth = maxWidth;
                newHeigth = Math.Round((newWidth / imageWidth) * imageHeigth);
            }
            else
            if (imageHeigth > imageWidth)
            {
                newHeigth = maxHeight;
                newWidth = Math.Round((newHeigth / imageHeigth) * imageWidth);
            }
            else
            {
                newHeigth = newWidth = maxHeight;
            }

            gfx.DrawImage(image, 50, 50,
              newWidth,
              newHeigth);
            return document;
        }
        public static PdfDocument AddPagination(this PdfDocument document)
        {
            int totalPages = document.Pages.Count;
            int currentPage = 0;
            var font = new XFont("OpenSans", 10, XFontStyle.Bold);
            foreach (var page in document.Pages)
            {
                ++currentPage;
                using var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawString(
                    $"{currentPage}/{totalPages}", font, XBrushes.Black,
                    new XPoint(page.Width - 30, page.Height - 30),
                    XStringFormats.BaseLineRight);
            }
            return document;
        }

        public static void Save(this PdfDocument document)
        {
            var outFilePath = Path.Combine(@"C:\Dev\MergePdfs\Assets", $"{Guid.NewGuid()}.pdf");
            var dir = Path.GetDirectoryName(outFilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            document.Save(outFilePath);
        }

        public static byte[] GetBytesFromMemoryStream(this PdfDocument document)
        {
            using var ms = new MemoryStream();
            document.Save(ms);
            return ms.ToArray();
        }
    }
}
