using Microsoft.Playwright;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace AutomationTestingProgram.Actions
{
    /// <summary>
    /// This action converts PDFs to text files using PdfSharp.
    /// </summary>
    public class ConvertPDF : IWebAction
    {
        public string Name { get; set; } = "ConvertPDF";

        private string? _pdfFileToConvertExpected;
        private string? _pdfFileToConvertActual;
        private string? _outputDirectory;

        /*public ConvertPDF()
        {
            // Set up a temporary output directory for converted text files.
            _outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "temp");
            Directory.CreateDirectory(_outputDirectory); // Ensure the directory exists
        }*/

        /// <inheritdoc/>
        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
        {
            // Set up a temporary output directory for converted text files.
            _outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "temp");
            Directory.CreateDirectory(_outputDirectory); // Ensure the directory exists

            // Hardcoded paths for the PDF files to be converted (can be customized)
            _pdfFileToConvertExpected = step.Object;  // Path to expected PDF
            _pdfFileToConvertActual = step.Value;      // Path to actual PDF

            try
            {
                // Convert PDFs to text files
                ConvertPdfToText(_pdfFileToConvertExpected, "expected.txt");
                ConvertPdfToText(_pdfFileToConvertActual, "actual.txt");

                /*step.Status = true;
                step.Actual = $"Successfully converted PDFs to text files.";*/
                return true;
            }
            catch (Exception e)
            {
                // Handle exceptions if anything goes wrong
                Console.WriteLine("Convert PDF FAILED: " + e.Message);
                /*step.Status = false;
                step.Actual = $"Error converting PDFs: {e.Message}";*/
                return false;
            }
        }

        /// <summary>
        /// Converts a PDF file to a text file.
        /// </summary>
        /// <param name="pdfFilePath">Path to the PDF file.</param>
        /// <param name="outputFileName">Output text file name.</param>
        private void ConvertPdfToText(string pdfFilePath, string outputFileName)
        {
            try
            {
                // Ensure the PDF file exists
                if (!File.Exists(pdfFilePath))
                {
                    throw new FileNotFoundException($"PDF file not found: {pdfFilePath}");
                }

                // Open the PDF file using PdfSharp
                using (PdfDocument document = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.ReadOnly))
                {
                    StringBuilder text = new StringBuilder();

                    // Loop through each page of the PDF
                    for (int pageIndex = 0; pageIndex < document.PageCount; pageIndex++)
                    {
                        var page = document.Pages[pageIndex];
                        text.AppendLine(ExtractTextFromPage(page));
                    }

                    // Save the extracted text to a file
                    string outputFilePath = Path.Combine(_outputDirectory, outputFileName);
                    File.WriteAllText(outputFilePath, text.ToString());

                    Console.WriteLine($"Converted {pdfFilePath} to {outputFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting PDF {pdfFilePath}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Extracts text from a given PDF page using PdfSharp.
        /// This method assumes the page has simple, extractable text.
        /// </summary>
        /// <param name="page">The PDF page to extract text from.</param>
        /// <returns>The extracted text.</returns>
        private string ExtractTextFromPage(PdfPage page)
        {
            // In PdfSharp, text extraction is quite rudimentary.
            // This basic implementation is limited, and more advanced extraction could require another tool like PDFBox or Tesseract.

            StringBuilder pageText = new StringBuilder();

            // Use PdfSharp's content parser to get the raw content of the page
            foreach (var item in page.Contents.Elements)
            {
                var content = item.ToString(); // Simple text output of content stream; this is quite raw
                pageText.Append(content);
            }

            return pageText.ToString();
        }
    }
}
