using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System.Globalization;
using System.Net;

namespace AdmitCard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdmitcardController : ControllerBase
    {
        private readonly string inpath;
        private readonly string outpath;
        private readonly string inspath;
       
        public AdmitcardController(IConfiguration config)
        {
            inpath = config["Path:incomingpdfPath"];
            outpath = config["Path:downloadedPath"];
            inspath = config["Path:instructionPath"];
        }

        [HttpGet("GetPdf")]
        public async Task<IActionResult> GetPdf(string candidateno)
        {
            Console.Out.WriteLine("Value of parameter received is:"+candidateno);
            if(candidateno == null)
            {
                return null;
            }
            
            string infilepath = inpath + candidateno + ".pdf";
            string inspdfpath = inspath;
            //var result=AddTimestampToPdf(infilepath, candidateno);
            var result = MergePdfWithTimestamp(infilepath, inspdfpath, candidateno);
            if (result!=null)
            {
                result = null;
               return result;
            }
            return null;
        }

        private IActionResult  MergePdfWithTimestamp(string pdf1Path, string pdf2Path, string candidateno)
        {
            string mergedPdfPath;
            MemoryStream ms = null;
            try
            { 
           
            // Load the two PDFs
            PdfDocument pdf1 = PdfReader.Open(pdf1Path, PdfDocumentOpenMode.Import);
            PdfDocument pdf2 = PdfReader.Open(pdf2Path, PdfDocumentOpenMode.Import);

            // Create a new merged PDF
            PdfDocument mergedPdf = new PdfDocument();

            // Iterate through each page of the first PDF
            for (int pageIndex = 0; pageIndex < pdf1.PageCount; pageIndex++)
            {
                // Get the page from the first PDF
                PdfPage pdf1Page = pdf1.Pages[pageIndex];

                // Add the page to the merged PDF
                PdfPage newPage = mergedPdf.AddPage(pdf1Page);

                // Add a timestamp to the page
                AddTimestamp(newPage);
            }

            // Iterate through each page of the second PDF
            for (int pageIndex = 0; pageIndex < pdf2.PageCount; pageIndex++)
            {
                // Get the page from the second PDF
                PdfPage pdf2Page = pdf2.Pages[pageIndex];

                // Add the page to the merged PDF
                PdfPage newPage = mergedPdf.AddPage(pdf2Page);

                // Add a timestamp to the page
                AddTimestamp(newPage);
            }

            // Save the merged PDF to a file
        //    mergedPdf.Save(mergedPdfPath);
            // Save the modified document to a new file


            byte[]? response = null;
            ms = new MemoryStream();
            mergedPdf.Save(ms);
            response = ms.ToArray();
            string fileName = "AdmitCard" + candidateno + ".pdf";
           // File(response, "application/pdf", fileName);
            return File(response, "application/pdf", fileName);
        }
            catch(Exception ex) 
            {
                return null;
            }
            finally
            {
                ms?.Dispose();
            }
    }

    static void AddTimestamp(PdfPage page)
        {
            try
            {
                // Get the page size
                //     XSize pageSize = page.Size;

                // Create a graphics object for the page
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Set the font and brush for the timestamp
                XFont font = new XFont("Arial", 10, XFontStyle.Regular);
                XBrush brush = XBrushes.Black;
                // Set Ip Address Here
                string strHostName = System.Net.Dns.GetHostName();
                IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
                IPAddress[] addr = ipEntry.AddressList;
                var ipAddress = addr[addr.Length - 1].ToString();

                // Get the current date and time
                string timestamp = "Admit card Downloaded from http://upsssc.gov.in/, " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ", accessed IP Address : " + ipAddress;
                // Get the current timestamp
                //  string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                // Calculate the position for the timestamp (top-right corner with some margin)
                double x = page.Width.Point - gfx.MeasureString(timestamp, font).Width - 50;
                double y1 = page.Height.Point - gfx.MeasureString(timestamp, font).Height - 815; ;

                // Draw the timestamp on the page
                gfx.DrawString(timestamp, font, brush, new XPoint(x, y1));

                // Calculate the position for the timestamp (bottom-right corner with some margin)
                //  double x = page.Width.Point - gfx.MeasureString(timestamp, font).Width - 50;
                double y = page.Height.Point - gfx.MeasureString(timestamp, font).Height - 10;

                // Draw the timestamp on the page
                gfx.DrawString(timestamp, font, brush, new XPoint(x, y));
            }
            catch { }
        }



        private IActionResult AddTimestampToOtherPdf(string inputFilePath, string candidateno)
        {
            MemoryStream ms = null;
            var timestamp = string.Empty;
            try
            {
                int i = 0;
                // Create a new PDF document
                using (var document = new PdfDocument())
                {
                    // Open the existing PDF file
                    using (var existingDocument = PdfReader.Open(inputFilePath, PdfDocumentOpenMode.Import))
                    {
                        // Iterate through each page of the existing document
                        foreach (var existingPage in existingDocument.Pages)
                        {
                            // Create a new page for the modified document
                            var page = document.AddPage(existingPage);

                            // Create a graphics object for drawing on the page
                            using (var gfx = XGraphics.FromPdfPage(page))
                            {
                                // Set the font and size for the timestamp
                                var font = new XFont("Arial", 9, XFontStyle.Regular);

                                // Set the position where the timestamp should be displayed
                                var position = new XPoint(70, 15);

                                // Set Ip Address Here
                                string strHostName = System.Net.Dns.GetHostName();
                                IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
                                IPAddress[] addr = ipEntry.AddressList;
                                var ipAddress = addr[addr.Length - 1].ToString();
                                //if (i == 0)
                                //{
                                // Get the current date and time
                                timestamp = "Admit card Downloaded from http://upsssc.gov.in/," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ", accessed IP Address : " + ipAddress;

                                // Draw the timestamp on the page
                                gfx.DrawString(timestamp, font, XBrushes.Black, position);
                                //}
                                // i += 1;
                                position = new XPoint(70, 800);
                                gfx.DrawString(timestamp, font, XBrushes.Black, position);

                            }
                        }
                    }

                    // Save the modified document to a new file


                    byte[]? response = null;
                    ms = new MemoryStream();
                    document.Save(ms);
                    response = ms.ToArray();
                    string fileName = "AdmitCard" + candidateno + ".pdf";
                    File(response, "application/pdf", fileName);
                    return File(response, "application/pdf", fileName);


                }

            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                ms?.Dispose();
            }
        }
    }

}

