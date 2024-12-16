using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Hyphenation;
using iText.Layout.Properties;
using ppm_fe.Models;
using ppm_fe.ViewModels;
using Cell = iText.Layout.Element.Cell;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;
using Tab = iText.Layout.Element.Tab;
using Table = iText.Layout.Element.Table;
using Text = iText.Layout.Element.Text;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using VerticalAlignment = iText.Layout.Properties.VerticalAlignment;

namespace ppm_fe.Helpers
{
    public class PdfHelper : BaseViewModel
    {
        public static readonly string DEST = "results/LARAPPMFrontend/logotransparent.pdf";

        public static async Task<PdfResult> GeneratePdfBilling(string destinationPath, BillingsInfoProfile billing)
        {
            var pdf = new PdfHelper();
            return await pdf.ManipulateBillingPdf(destinationPath, billing);
        }

        public async Task<PdfResult> ManipulateBillingPdf(string dest, BillingsInfoProfile billing)
        {
            PdfResult result = new PdfResult { IsSuccess = false };

            await Task.Run(() =>
            {
                try
                {
                    using (PdfDocument pdfDoc = new(new PdfWriter(dest)))
                    {
                        pdfDoc.SetDefaultPageSize(PageSize.A4); // Set to A4 portrait

                        Document doc = new(pdfDoc);

                        PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                        PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
                        PdfFont boldFontTitle = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);

                        string VorOrtORExkursion = billing.IsVorOrt ? "vor Ort" : "Ausflug";

                        // Create a table for the header with two columns (70% and 30%)
                        Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 70f, 30f })).UseAllAvailableWidth();

                        // Left column - 4 sentences
                        Cell leftCell = new Cell();
                        doc.Add(new Paragraph("An den").SetFont(regularFont).SetFontSize(10).SetMarginBottom(0).SetMarginTop(0).SetFixedLeading(12));
                        doc.Add(new Paragraph("Verein zur Förderung der Jugendarbeit e.V.").SetFont(regularFont).SetFontSize(10).SetMarginBottom(0).SetMarginTop(0).SetFixedLeading(12));
                        doc.Add(new Paragraph("Neuhöfer Str. 23 Halle 13").SetFont(regularFont).SetFontSize(10).SetMarginBottom(0).SetMarginTop(0).SetFixedLeading(12));
                        doc.Add(new Paragraph("21107 Hamburg").SetFont(regularFont).SetFontSize(10).SetMarginBottom(0).SetMarginTop(0).SetFixedLeading(12));

                        // Create a div for right-aligned content
                        Div rightAlignedDiv = new Div()
                            .SetWidth(UnitValue.CreatePercentValue(30))
                            .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.RIGHT);

                        // Add Date
                        rightAlignedDiv.Add(CreateBorderedParagraph("Datum ", billing.Date, regularFont, boldFont));
                        doc.Add(new Paragraph().SetMarginBottom(10));

                        // Add Billing Number
                        rightAlignedDiv.Add(CreateBorderedParagraph("Rechnungsnummber ", billing.BillingNumber.ToString(), regularFont, boldFont));
                        doc.Add(new Paragraph().SetMarginBottom(10));

                        // Add TaxID
                        rightAlignedDiv.Add(CreateBorderedParagraph("Steueridentifkationsnummer", billing.Steueridentifikationsnummer, regularFont, boldFont, true));
                        doc.Add(new Paragraph().SetMarginBottom(10));

                        // Add the right-aligned div to the document
                        doc.Add(rightAlignedDiv);

                        // Add some space
                        doc.Add(new Paragraph().SetMarginBottom(10));

                        doc.Add(new Paragraph("Rechnung für Arbeit in Wohnunterkünften").SetTextAlignment(TextAlignment.LEFT).SetFont(boldFontTitle).SetFontSize(20));
                        doc.Add(new Paragraph().SetMarginBottom(-10));
                        doc.Add(new Paragraph()
                            .Add(new Text(VorOrtORExkursion).SetFont(boldFontTitle).SetFontSize(20))
                            .Add(new Text(" ").SetFont(boldFontTitle).SetFontSize(20))
                            .Add(new Text(billing.Team).SetFont(boldFontTitle).SetFontSize(20))
                            .SetTextAlignment(TextAlignment.LEFT)
                             );

                        doc.Add(new Paragraph().SetMarginBottom(10));

                        // Create a div for right-aligned content
                        Div alignedDiv = new Div()
                            .SetWidth(UnitValue.CreatePercentValue(50))
                            .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.LEFT);

                        doc.Add(CreateLabeledPair("Name", "Vorname", boldFont, 8));

                        Table table = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth();

                        // Create the cell for the first name
                        Cell firstNameCell = new Cell()
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)) // Add a black frame
                            .SetPadding(5) // Add some space within the cell
                            .Add(new Paragraph(billing.Firstname).SetFont(regularFont).SetFontSize(12));

                        // Create the cell for the last name
                        Cell lastNameCell = new Cell()
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)) // Add a black frame
                            .SetPadding(5) // Add some space within the cell
                            .Add(new Paragraph(billing.Lastname).SetFont(regularFont).SetFontSize(12));

                        // Add the cells to the table
                        table.AddCell(firstNameCell);
                        table.AddCell(lastNameCell);

                        // Add the table to the document
                        doc.Add(table);

                        doc.Add(new Paragraph("Anschrift").SetFont(boldFont).SetFontSize(8).SetMarginBottom(0));

                        // Create a div element
                        Div div = new Div()
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)) // Add a black frame, 1 point thick
                            .SetPadding(5) // Add some space within the div
                            .SetWidth(UnitValue.CreatePercentValue(100)); // Sets the width to 100%

                        // Add the content to the div
                        div.Add(new Paragraph(billing.UserAddress).SetFont(regularFont).SetFontSize(12).SetMargin(0));

                        // Add the div to the document
                        doc.Add(div);
                        doc.Add(new Paragraph().SetMarginBottom(10));
                        doc.Add(alignedDiv);
                        doc.Add(new Paragraph().SetMarginBottom(20));
                        doc.Add(new Paragraph()
                            .Add(new Text("Im Monat ").SetFont(regularFont).SetFontSize(12))
                            .Add(new Text(billing.Month).SetFont(boldFont).SetFontSize(12))
                            .Add(new Text("  habe ich für das Projekt „Falkenflitzer“ in selbständiger freizeitpädagogischer Tätigkeit gearbeitet und stelle folgendes in Rechnung:")
                                .SetFont(regularFont)
                                .SetFontSize(12))
                            .SetTextAlignment(TextAlignment.LEFT)
                            );

                        // Assuming you're inside your PDF creation method
                        foreach (var detail in billing.BillingDetails)
                        {
                            doc.Add(new Paragraph()
                                .Add(new Text("Am ").SetFont(regularFont).SetFontSize(12))
                                .Add(new Text(detail.DateWorkDay.ToString("dd.MM.yyyy")).SetFont(boldFont).SetFontSize(12))
                                .Add(new Text(" habe ich als ").SetFont(regularFont).SetFontSize(12))
                                .Add(new Text(billing.Role).SetFont(boldFont).SetFontSize(12))
                                .Add(new Text(" ").SetFont(regularFont).SetFontSize(12))
                                .Add(new Text(detail.NumberOfHours.ToString("F2")).SetFont(boldFont).SetFontSize(12))
                                .Add(new Text(" Stunden a ").SetFont(regularFont).SetFontSize(12))
                                .Add(new Text(detail.Stundenlohn.ToString("F2")).SetFont(boldFont).SetFontSize(12))
                                .Add(new Text(" € geleistet und stelle ").SetFont(regularFont).SetFontSize(12))
                                .Add(new Text(detail.WorkDay.ToString("F2")).SetFont(boldFont).SetFontSize(12))
                                .Add(new Text(" € in Rechnung.").SetFont(regularFont).SetFontSize(12))
                                );
                            
                            // Add some space between each detail
                            doc.Add(new Paragraph().SetMarginBottom(0));
                        }

                        doc.Add(new Paragraph().SetMarginBottom(10));
                        doc.Add(new Paragraph()
                           .Add(new Text("Summe: ").SetFont(regularFont).SetFontSize(12))
                           .Add(new Text(billing.SommeAll.ToString("F2")).SetFont(boldFont).SetFontSize(12))
                           .Add(new Text(" €").SetFont(regularFont).SetFontSize(12))
                           .SetTextAlignment(TextAlignment.RIGHT)
                               );

                        doc.Add(new Paragraph("Neue Bankverbindung:").SetTextAlignment(TextAlignment.LEFT).SetFont(boldFont).SetFontSize(12));
                        doc.Add(new Paragraph().SetMarginBottom(0));
                        doc.Add(new Paragraph()
                             .Add(new Text("Bankname: ").SetFont(regularFont).SetFontSize(12))
                             .Add(new Text(billing.BankName).SetFont(boldFont).SetFontSize(12))
                             .SetTextAlignment(TextAlignment.LEFT)
                             .SetMarginBottom(0)
                             );

                        doc.Add(new Paragraph()
                             .Add(new Text("IBAN: ").SetFont(regularFont).SetFontSize(12))
                             .Add(new Text(billing.Iban).SetFont(boldFont).SetFontSize(12))
                             .SetTextAlignment(TextAlignment.LEFT)
                             .SetMarginBottom(0)
                             );
                        doc.Add(new Paragraph()
                             .Add(new Text("BIC: ").SetFont(regularFont).SetFontSize(12))
                             .Add(new Text(billing.Bic).SetFont(boldFont).SetFontSize(12))
                             .SetTextAlignment(TextAlignment.LEFT)
                             .SetMarginBottom(10)
                             );

                        // Calculate the width based on the page width and margins
                        float pageWidth = pdfDoc.GetDefaultPageSize().GetWidth();
                        float footerWidth = pageWidth - doc.GetLeftMargin() - doc.GetRightMargin();

                        // Create a div container for the footer area
                        Div footer = new Div()
                            .SetKeepTogether(true)
                            .SetFixedPosition(
                                doc.GetLeftMargin(),  // X-Position: left margin of the document
                                doc.GetBottomMargin(),  // Y-Position: bottom margin of the document
                                footerWidth  // Calculated width
                            )
                            .SetPadding(0);


                        // Add elements to the footer
                        footer.Add(new Paragraph("Ich versichere die Richtigkeit meiner Angaben. Es entstehen keine steuer- und sozialversicherungsrechtlichen" +
                            " Ansprüche gegen den Verein zur Förderung der Jugendarbeit e.V.. Alle Beträge sind laut § 19 UStG umsatzsteuerfrei.:").SetTextAlignment(TextAlignment.LEFT).SetFont(regularFont).SetFontSize(10));
                        footer.Add(new Paragraph().SetMarginBottom(10));
                        footer.Add(new Paragraph(".............................................................................")
                            .SetTextAlignment(TextAlignment.RIGHT).SetFont(regularFont).SetFontSize(8));
                        footer.Add(new Paragraph("Falkenflitzer: sachlich und rechnerisch richtig")
                            .SetTextAlignment(TextAlignment.RIGHT).SetFont(regularFont).SetFontSize(8));

                        // Add the footer to the document
                        doc.Add(footer);

                        // Close the document
                        doc.Close();
                    }
                    result.IsSuccess = true;
                }
                catch (IOException ex)
                {
                    result.ErrorMessage = "Die PDF-Datei wird gerade von einer anderen Anwendung verwendet. Bitte schließen Sie alle Programme, die die Datei verwenden könnten, und versuchen Sie es erneut.";
                    LoggerHelper.LogError(GetType().Name, nameof(ManipulateBillingPdf), $"IOException occurred: {ex.Message}", new { dest, billing }, ex.StackTrace);
                }
                catch (Exception ex)
                {
                    result.ErrorMessage = $"Ein unerwarteter Fehler ist aufgetreten: {ex.Message}";
                    LoggerHelper.LogError(GetType().Name, nameof(ManipulateBillingPdf), $"Unexpected exception occurred: {ex.Message}", new { dest, billing }, ex.StackTrace);
                }
            });

            return result;
        }

        private IBlockElement CreateBorderedParagraph(string label, string value, PdfFont regularFont, PdfFont boldFont, bool isVertical = false)
        {
            // Create a table with one cell
            Table table = new Table(1).UseAllAvailableWidth();

            // Create a cell with border
            Cell cell = new Cell()
                .SetBorder(new SolidBorder(1))
                .SetPadding(5);

            if (isVertical)
            {
                // For TaxID: Label above, value below
                cell.Add(new Paragraph(label).SetFont(regularFont).SetFontSize(10).SetMarginBottom(0));
                cell.Add(new Paragraph(value).SetFont(boldFont).SetFontSize(10).SetMarginTop(0));
            }
            else
            {
                // For other fields: Label and value on the same line
                Table innerTable = new Table(UnitValue.CreatePercentArray(new float[] { 30, 70 }));

                Cell labelCell = new Cell()
                    .Add(new Paragraph(label).SetFont(regularFont).SetFontSize(10))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetPadding(0);
                innerTable.AddCell(labelCell);

                Cell valueCell = new Cell()
                    .Add(new Paragraph(value).SetFont(boldFont).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetPadding(0);
                innerTable.AddCell(valueCell);

                cell.Add(innerTable);
            }

            // Add the cell to the table
            table.AddCell(cell);

            // Create a div to wrap the table and add margin
            Div wrapper = new Div()
                .Add(table)
                .SetMarginBottom(10); // Add margin to the bottom

            // Return the wrapper div
            return wrapper;
        }

        public static Paragraph CreateLabeledPair(string label1, string label2, PdfFont font, float fontSize)
        {
            return new Paragraph()
                .Add(new Text(label1).SetFont(font).SetFontSize(fontSize))
                   .Add(new Tab())
                   .Add(new Tab())
                   .Add(new Tab())
                   .Add(new Tab())
                   .Add(new Tab())
                   .Add(new Paragraph().SetMarginRight(10))
                .Add(new Text(label2).SetFont(font).SetFontSize(fontSize));
        }

        public static async Task<string> GeneratePdfWithTable(string title, Work work)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await Task.Run(() =>
                {
                    PdfWriter writer = new PdfWriter(memoryStream);
                    PdfDocument pdfDoc = new PdfDocument(writer);
                    pdfDoc.SetDefaultPageSize(PageSize.A4.Rotate());

                    // Add content to the PDF
                    Document doc = new Document(pdfDoc);

                    // Add content to the PDF
                    PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                    // Add title
                    doc.Add(new Paragraph(title).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    // Create a table for the header with two columns (70% and 30%)
                    Table headerTable = new Table(UnitValue.CreatePercentArray([10f, 15f, 50f, 5f, 20f])).UseAllAvailableWidth();

                    bool isValidDate = DateTime.TryParse(work.Date, out DateTime parsedDate);
                    if (isValidDate)
                    {
                        string formattedDate = parsedDate.ToString("M/d/yyyy");

                        Cell headingCellDatum = new Cell().Add(new Paragraph("Datum"))
                                         .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER) // Align text to the left
                                         .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE) // Align text to the left
                                         .SetFont(boldFont) // Set the font to bold
                                         .SetFontSize(14) // Set font size
                                         .SetBold() // Another way to make the text bold if the font supports it
                                         .SetBackgroundColor(ColorConstants.LIGHT_GRAY); // Set background color to light gray
                        headerTable.AddCell(headingCellDatum);
                        headerTable.AddCell(new Cell().Add(new Paragraph(formattedDate)).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER) // Align text to the left
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE) // Align text to the left
                            );
                    }
                    else
                    {
                        headerTable.AddCell(new Cell().Add(new Paragraph("Datum: Parsing Error")).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT));
                    }
                    headerTable.AddCell(new Cell().Add(new Paragraph("Ort: " + work.Ort)).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER) // Align text to the left
                            .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)); // Align text to the left);

                    string helpersText = work.ListOfHelpers != null
                                            ? string.Join(", ", work.ListOfHelpers.Where(h => h != null))
                                            : string.Empty;

                    Cell headingCellHepers = new Cell().Add(new Paragraph("Helfer"))
                             .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER) // Align text to the left
                             .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE) // Align text to the left
                             .SetFont(boldFont) // Set the font to bold
                             .SetFontSize(14) // Set font size
                             .SetBold() // Another way to make the text bold if the font supports it
                             .SetBackgroundColor(ColorConstants.LIGHT_GRAY); // Set background color to light gray
                    headerTable.AddCell(headingCellHepers);
                    headerTable.AddCell(new Cell().Add(new Paragraph(helpersText)).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT));

                    doc.Add(headerTable);

                    // Add some space between header and body
                    doc.Add(new Paragraph("\n"));

                    // Get the page size
                    Rectangle pageSize = doc.GetPdfDocument().GetDefaultPageSize();
                    float pageHeight = pageSize.GetHeight();

                    // Calculate the available height (subtract margins and any other content height)
                    float availableHeight = pageHeight - doc.GetTopMargin() - doc.GetBottomMargin();

                    // Create the body table with a single column
                    Table bodyTable = new Table(1).UseAllAvailableWidth();

                    Cell headingCell = new Cell().Add(new Paragraph("Plan"))
                                             .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER) // Align text to the left
                                             .SetFont(boldFont) // Set the font to bold
                                             .SetFontSize(14) // Set font size
                                             .SetBold() // Another way to make the text bold if the font supports it
                                             .SetHeight(40) // Set the minimum height
                                             .SetBackgroundColor(ColorConstants.LIGHT_GRAY); // Set background color to light gray

                    bodyTable.AddCell(headingCell).SetMinHeight(availableHeight); // Set the minimum height;

                    // Create a cell for the paragraph content
                    Cell contentCell = new Cell()
                        .Add(new Paragraph(work.Plan)
                            .SetTextAlignment(TextAlignment.JUSTIFIED)
                            .SetFontSize(10) // Adjust font size as needed
                        )
                        .SetPadding(10) // Add some padding
                        .SetVerticalAlignment(VerticalAlignment.TOP);

                    // Add the content cell to the body table
                    bodyTable.AddCell(contentCell);

                    // Set properties for automatic text fitting
                    bodyTable.SetKeepTogether(true);
                    bodyTable.SetSkipFirstHeader(true);
                    bodyTable.SetSkipLastFooter(true);

                    // Add the body table to the document
                    doc.Add(bodyTable);

                    doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                    // Second page - All content in one page
                    Table mainTable = new Table(1).UseAllAvailableWidth();

                    // Reflection section
                    Table reflectionTable = new Table(UnitValue.CreatePercentArray(new float[] { 100f, 0f })).UseAllAvailableWidth();
                    Cell reflectionHeadingCell = new Cell(1, 2)
                        .Add(new Paragraph("Reflection"))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(boldFont)
                        .SetFontSize(14)
                        .SetBold()
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .SetPadding(0); // Increased padding

                    reflectionTable.AddHeaderCell(reflectionHeadingCell);
                    reflectionTable.SetWidth(UnitValue.CreatePercentValue(100));
                    float fontSize = work.Reflection.Length > 200 ? 8f : 10f; // Adjust font size for long text

                    reflectionTable.AddCell(
                         new Cell()
                             .Add(new Paragraph(work.Reflection)
                             .SetHyphenation(new HyphenationConfig("de", "DE", 3, 3))) // Breaks words with a hyphen
                             .SetFont(regularFont)
                             .SetFontSize(fontSize)
                             .SetFontSize(10)
                             .SetPadding(10)
                             .SetWordSpacing(10f)
                     );
                    Cell reflectionCell = new Cell().Add(reflectionTable)
                                .SetPadding(10)
                                .SetMinHeight(250);

                    mainTable.AddCell(reflectionCell);

                    // Kids Data section (20% of the page height)
                    Table kidsDataTable = new Table(UnitValue.CreatePercentArray(new float[] { 25f, 25f, 25f, 25f })).UseAllAvailableWidth();
                    Cell kidsDataHeadingCell = new Cell(1, 4)
                        .Add(new Paragraph("Kinder Zahlen"))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(boldFont)
                        .SetFontSize(12)
                        .SetBold()
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY);

                    kidsDataTable.AddHeaderCell(kidsDataHeadingCell);
                    kidsDataTable.AddCell(new Cell().Add(new Paragraph("Altersgroupe")).SetFont(boldFont).SetFontSize(9));
                    kidsDataTable.AddCell(new Cell().Add(new Paragraph("Jungen")).SetFont(boldFont).SetFontSize(9));
                    kidsDataTable.AddCell(new Cell().Add(new Paragraph("Mädchen")).SetFont(boldFont).SetFontSize(9));
                    kidsDataTable.AddCell(new Cell().Add(new Paragraph("Zusammen")).SetFont(boldFont).SetFontSize(9));

                    // Kids Data Section
                    int totalBoys = 0;
                    int totalGirls = 0;

                    // Kids Data Section
                    foreach (var kidData in work.KidsData)
                    {
                        kidsDataTable.AddCell(new Cell().Add(new Paragraph(kidData.AgeRange ?? "")).SetFont(regularFont).SetFontSize(9));

                        kidsDataTable.AddCell(new Cell().Add(new Paragraph(kidData.NumberBoys != null ? kidData.NumberBoys.ToString() : "")).SetFont(regularFont).SetFontSize(9));
                        kidsDataTable.AddCell(new Cell().Add(new Paragraph(kidData.NumberGirls != null ? kidData.NumberGirls.ToString() : "")).SetFont(regularFont).SetFontSize(9));

                        string rowTotal = (kidData.NumberBoys != null && kidData.NumberGirls != null) ?
                               (kidData.NumberBoys + kidData.NumberGirls).ToString() : "";
                        //int rowTotal = (kidData.NumberBoys != 0 + (kidData.NumberGirls != 0);
                        kidsDataTable.AddCell(new Cell().Add(new Paragraph(rowTotal.ToString())).SetFont(regularFont).SetFontSize(9));

                        if (kidData != null)
                        {
                            totalBoys += kidData.NumberBoys;
                            totalGirls += kidData.NumberGirls;
                        }
                    }

                    //Cell kidsDataCell = new Cell().Add(kidsDataTable).SetHeight(130); // Kept the same height
                    Cell kidsDataCell = new Cell().Add(kidsDataTable).SetHeight(150);
                    mainTable.AddCell(kidsDataCell);

                    // Add footer row with totals
                    Cell totalCell = new Cell(1, 4)
                        .Add(new Paragraph($"Total Jungen: {totalBoys} | Total Mädchen: {totalGirls} | Total Kinderzahl: {totalBoys + totalGirls}"))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(boldFont)
                        .SetFontSize(10)
                        .SetBold()
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY);

                    kidsDataTable.AddFooterCell(totalCell);

                    // Additional information section (20% of the page height)
                    Table additionalInfoTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.33f })).UseAllAvailableWidth();

                    // Additional information section (Parent Contact, Wellbeing, Wishes)
                    Cell parentContactCell = new Cell()
                        .Add(new Paragraph("Elternkontakt").SetFont(boldFont).SetFontSize(10))
                        .Add(new Paragraph(work.ParentContact ?? "").SetFont(regularFont).SetFontSize(9))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetPadding(5);

                    Cell wellbeingChildrenCell = new Cell()
                        .Add(new Paragraph("Kinderswohl").SetFont(boldFont).SetFontSize(10))
                        .Add(new Paragraph(work.WellbeingOfChildren ?? "").SetFont(regularFont).SetFontSize(9))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetPadding(5);

                    Cell wishesCell = new Cell()
                        .Add(new Paragraph("Wünsche").SetFont(boldFont).SetFontSize(10))
                        .Add(new Paragraph(work.Wishes ?? "").SetFont(regularFont).SetFontSize(9))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetPadding(5);

                    additionalInfoTable.AddCell(parentContactCell);
                    additionalInfoTable.AddCell(wellbeingChildrenCell);
                    additionalInfoTable.AddCell(wishesCell);

                    Cell additionalInfoCell = new Cell().Add(additionalInfoTable); // Reduced height
                    mainTable.AddCell(additionalInfoCell);
                    doc.Add(mainTable);
                    pdfDoc.Close();

                    // Convert the MemoryStream back to base64
                    string outputBase64 = Convert.ToBase64String(memoryStream.ToArray());
                    pdfDoc.Close();
                });

                // Convert MemoryStream to base64 string
                byte[] pdfBytes = memoryStream.ToArray();
                return Convert.ToBase64String(pdfBytes);
            }
        }
    }
}
