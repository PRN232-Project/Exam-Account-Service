using ClosedXML.Excel;
using PRN232.ExamAccount.Domain.Entities;

namespace PRN232.ExamAccount.Api.Integration;

public static class GradingBatchExcelExporter
{
    public static byte[] Export(GradingBatch batch)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Bang diem");

        sheet.Cell("A1").Value = "BẢNG ĐIỂM CHẤM THI";
        sheet.Range("A1:L1").Merge();
        sheet.Cell("A1").Style.Font.Bold = true;
        sheet.Cell("A1").Style.Font.FontSize = 16;
        sheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        sheet.Cell("A2").Value = "Batch";
        sheet.Cell("B2").Value = batch.Code;
        sheet.Cell("D2").Value = "Ca thi";
        sheet.Cell("E2").Value = batch.ExamSession?.Code ?? "";
        sheet.Cell("G2").Value = "Mã đề";
        sheet.Cell("H2").Value = batch.ExamSession?.ExamPaper?.Code ?? "";

        sheet.Cell("A3").Value = "Giảng viên";
        sheet.Cell("B3").Value = batch.Lecturer?.FullName ?? "";
        sheet.Cell("D3").Value = "Ngày xuất (UTC)";
        sheet.Cell("E3").Value = DateTime.UtcNow;
        sheet.Cell("E3").Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
        sheet.Cell("G3").Value = "Trạng thái batch";
        sheet.Cell("H3").Value = batch.Status.ToString();

        var headers = new[]
        {
            "STT", "Mã sinh viên", "Họ và tên", "Mã đề", "Điểm",
            "Trạng thái", "Lần chấm", "Mã lỗi", "Chi tiết lỗi",
            "Trạng thái đạo văn", "Số vi phạm", "Tương đồng cao nhất (%)"
        };
        const int headerRow = 5;
        for (var column = 1; column <= headers.Length; column++)
            sheet.Cell(headerRow, column).Value = headers[column - 1];

        var items = batch.Items
            .OrderBy(x => x.ExamCandidate?.Student?.StudentCode)
            .ToList();
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var row = headerRow + index + 1;
            sheet.Cell(row, 1).Value = index + 1;
            sheet.Cell(row, 2).Value = item.ExamCandidate?.Student?.StudentCode ?? "";
            sheet.Cell(row, 3).Value = item.ExamCandidate?.Student?.FullName ?? "";
            sheet.Cell(row, 4).Value = item.ExamCandidate?.PaperCode ?? "";
            if (item.LatestScore.HasValue)
                sheet.Cell(row, 5).Value = item.LatestScore.Value;
            sheet.Cell(row, 6).Value = item.Status.ToString();
            sheet.Cell(row, 7).Value = item.LatestAttemptNumber;
            sheet.Cell(row, 8).Value = item.LastErrorCode;
            sheet.Cell(row, 9).Value = item.LastErrorMessage;
            sheet.Cell(row, 10).Value = item.PlagiarismStatus;
            sheet.Cell(row, 11).Value = item.PlagiarismViolationCount;
            if (item.PlagiarismMaxSimilarity.HasValue)
                sheet.Cell(row, 12).Value = item.PlagiarismMaxSimilarity.Value;
        }

        var tableRange = sheet.Range(headerRow, 1, Math.Max(headerRow + items.Count, headerRow), headers.Length);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        sheet.Range(headerRow, 1, headerRow, headers.Length).Style
            .Fill.SetBackgroundColor(XLColor.FromHtml("#1F4E78"));
        var headerStyle = sheet.Range(headerRow, 1, headerRow, headers.Length).Style;
        headerStyle.Font.Bold = true;
        headerStyle.Font.FontColor = XLColor.White;

        if (items.Count > 0)
        {
            sheet.Range(headerRow, 1, headerRow + items.Count, headers.Length).CreateTable();
            sheet.SheetView.FreezeRows(headerRow);
        }

        sheet.Column(1).Width = 7;
        sheet.Column(2).Width = 16;
        sheet.Column(3).Width = 28;
        sheet.Column(4).Width = 18;
        sheet.Column(5).Width = 10;
        sheet.Column(6).Width = 22;
        sheet.Column(7).Width = 11;
        sheet.Column(8).Width = 22;
        sheet.Column(9).Width = 40;
        sheet.Column(10).Width = 22;
        sheet.Column(11).Width = 13;
        sheet.Column(12).Width = 24;
        sheet.Columns(1, headers.Length).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        sheet.Columns(9, 10).Style.Alignment.WrapText = true;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
