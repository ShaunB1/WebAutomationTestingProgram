using System;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Text;
using System.Linq;
using System.Reflection;

namespace AutomationTestingProgram.AutomationFramework
{
    /// <summary>
    /// This test step converts PDFs to text files using PdfSharp.
    /// </summary>
    public class ConvertPDF : TestStep
    {
        /// <inheritdoc/>
        public override string TestCaseName { get; set; } = "ConvertPDF";

        private string _pdfFileToConvertExpected;
        private string _pdfFileToConvertActual;
        private string _outputDirectory;

        public ConvertPDF()
        {
            // Set up a temporary output directory for converted text files.
            _outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "temp");
            Directory.CreateDirectory(_outputDirectory); // Ensure the directory exists
            /*_outputDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigurationManager.AppSettings["TEMPORARY_FILES_FOLDER"]);*/

            // Hardcoded paths for the PDF files to be converted (can be customized) from test file
            _pdfFileToConvertExpected = this.Object;  // Hardcoded path to expected PDF
            _pdfFileToConvertActual = this.Value;      // Hardcoded path to actual PDF
        }

        /// <summary>
        /// This method performs the PDF conversion operation.
        /// </summary>
        public void RunConversion()
        {
            try
            {
                // Convert PDFs to text
                ConvertPdfToText(_pdfFileToConvertExpected, "expected.txt");
                ConvertPdfToText(_pdfFileToConvertActual, "actual.txt");
            }
            catch (Exception e)
            {
                // Handle exceptions if anything goes wrong
                Console.WriteLine("Convert PDF FAILED: " + e.Message);
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
                    /*this.TestStepStatus.Actual += $"\nConverted {pdfFilePath} to {outputFilePath}";*/
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
