using ClosedXML.Excel;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Services;

public interface IExcelService
{
    byte[] ExportTasks(IEnumerable<TaskItem> tasks, IDictionary<int, string> userNames);
    List<ImportedTaskRow> ImportTasks(Stream fileStream);
}

/// <summary>صف مستورد من ملف Excel — يُحوَّل لاحقاً إلى مهمة</summary>
public class ImportedTaskRow
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int ProgressPercent { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public string? Notes { get; set; }
    public string? AssignedToEmail { get; set; }
    public int RowNumber { get; set; }
    public List<string> Errors { get; } = new();
}

public class ExcelService : IExcelService
{
    private static readonly string[] Headers =
    {
        "العنوان", "الوصف", "الحالة", "الأولوية", "نسبة الإنجاز %",
        "تاريخ البدء", "تاريخ الاستحقاق", "الساعات المقدّرة", "الساعات الفعلية",
        "ملاحظات", "بريد المسؤول"
    };

    public byte[] ExportTasks(IEnumerable<TaskItem> tasks, IDictionary<int, string> userNames)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("المهام");
        ws.RightToLeft = true;

        for (int c = 0; c < Headers.Length; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = Headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563eb");
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var t in tasks)
        {
            ws.Cell(row, 1).Value = t.Title;
            ws.Cell(row, 2).Value = t.Description;
            ws.Cell(row, 3).Value = t.Status.ToString();
            ws.Cell(row, 4).Value = t.Priority.ToString();
            ws.Cell(row, 5).Value = t.ProgressPercent;
            if (t.StartDate.HasValue) ws.Cell(row, 6).Value = t.StartDate.Value;
            if (t.DueDate.HasValue) ws.Cell(row, 7).Value = t.DueDate.Value;
            ws.Cell(row, 8).Value = t.EstimatedHours;
            ws.Cell(row, 9).Value = t.ActualHours;
            ws.Cell(row, 10).Value = t.Notes;
            if (t.AssignedToUserId is int uid && userNames.TryGetValue(uid, out var name))
                ws.Cell(row, 11).Value = name;
            row++;
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public List<ImportedTaskRow> ImportTasks(Stream fileStream)
    {
        var result = new List<ImportedTaskRow>();
        using var wb = new XLWorkbook(fileStream);
        var ws = wb.Worksheets.First();
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        for (int r = 2; r <= lastRow; r++)
        {
            var title = ws.Cell(r, 1).GetString().Trim();
            if (string.IsNullOrWhiteSpace(title))
                continue; // تخطّي الصفوف الفارغة

            var item = new ImportedTaskRow
            {
                RowNumber = r,
                Title = title,
                Description = NullIfEmpty(ws.Cell(r, 2).GetString()),
                Status = NullIfEmpty(ws.Cell(r, 3).GetString()),
                Priority = NullIfEmpty(ws.Cell(r, 4).GetString()),
                ProgressPercent = (int)ws.Cell(r, 5).GetValue<double>(),
                StartDate = TryGetDate(ws.Cell(r, 6)),
                DueDate = TryGetDate(ws.Cell(r, 7)),
                EstimatedHours = TryGetDecimal(ws.Cell(r, 8)),
                ActualHours = TryGetDecimal(ws.Cell(r, 9)),
                Notes = NullIfEmpty(ws.Cell(r, 10).GetString()),
                AssignedToEmail = NullIfEmpty(ws.Cell(r, 11).GetString())
            };
            result.Add(item);
        }

        return result;
    }

    private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static DateTime? TryGetDate(IXLCell cell)
    {
        if (cell.IsEmpty()) return null;
        if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime();
        return DateTime.TryParse(cell.GetString(), out var d) ? d : null;
    }

    private static decimal? TryGetDecimal(IXLCell cell)
    {
        if (cell.IsEmpty()) return null;
        return decimal.TryParse(cell.GetString(), out var v) ? v : null;
    }
}
