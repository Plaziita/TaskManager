using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using TaskManager.Services;

namespace TaskManager.Pages;

[Authorize]
public class AnalyticsModel : PageModel
{
    private readonly TaskService _tasks;

    public AnalyticsModel(TaskService tasks)
    {
        _tasks = tasks;
    }

    [BindProperty(SupportsGet = true)]
    public string Range { get; set; } = "30"; 

    [BindProperty(SupportsGet = true)]
    public DateTime? Start { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? End { get; set; }

    private DateTime FromDate;
    private DateTime ToDate;

    public int TotalTasks { get; private set; }
    public int CompletedTasks { get; private set; }
    public double CompletedRatePct { get; private set; }
    public double AvgCompletedPerWeek { get; private set; }

    public int OpenCount { get; private set; }
    public int InProgressCount { get; private set; }
    public int BlockedCount { get; private set; }
    public int DoneCount { get; private set; }
    public List<DonutSlice> DonutSlices { get; private set; } = new();

    public List<WeeklyProductivityPoint> WeeklyProductivity { get; private set; } = new();
    public string ProductivitySvgPath { get; private set; } = string.Empty;
    public int ProductivityMax { get; private set; }

    public List<MonthlyBars> Monthly { get; private set; } = new();

    public async Task OnGet()
    {
        ApplyDateRange();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var all = (await _tasks.GetAllTaskAsync(userId))
            .Where(t => t.CreatedAt >= FromDate && t.CreatedAt <= ToDate)
            .ToList();

        TotalTasks = all.Count;

        string Norm(string? s)
        {
            var v = (s ?? "").Trim().ToLowerInvariant();
            return v switch
            {
                "done" => "Done",
                "in progress" => "In Progress",
                "open" => "Open",
                "blocked" => "Blocked",
                _ => "Other"
            };
        }

        DoneCount = all.Count(t => Norm(t.Status) == "Done");
        InProgressCount = all.Count(t => Norm(t.Status) == "In Progress");
        OpenCount = all.Count(t => Norm(t.Status) == "Open");
        BlockedCount = all.Count(t => Norm(t.Status) == "Blocked");

        CompletedTasks = DoneCount;
        CompletedRatePct = TotalTasks == 0 ? 0 : Math.Round(DoneCount * 100.0 / TotalTasks, 1);

        var donutData = new List<(string key, int count, string color)>
        {
            ("In Progress", InProgressCount, "#135bec"),
            ("Done", DoneCount, "#22c55e"),
            ("Open", OpenCount, "#f59e0b"),
            ("Blocked", BlockedCount, "#ef4444")
        };
        DonutSlices = BuildDonut(donutData, TotalTasks);

        var completed = all
            .Where(t => Norm(t.Status) == "Done")
            .Select(t => (t.DueDate ?? t.CreatedAt).Date)
            .ToList();

        var startWeek = FromDate.AddDays(-42); 
        WeeklyProductivity = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var ws = StartOfWeek(startWeek.AddDays(i * 7));
                var we = ws.AddDays(7);
                var count = completed.Count(x => x >= ws && x < we);
                return new WeeklyProductivityPoint($"W{ISOWeek.GetWeekOfYear(ws)}", count);
            })
            .ToList();

        ProductivityMax = Math.Max(1, WeeklyProductivity.Max(x => x.Completed));
        AvgCompletedPerWeek = Math.Round(WeeklyProductivity.Average(x => x.Completed), 1);
        ProductivitySvgPath = BuildLinePath(WeeklyProductivity.Select(x => (double)x.Completed).ToList());

        var firstMonth = new DateTime(ToDate.Year, ToDate.Month, 1).AddMonths(-4);
        Monthly = Enumerable.Range(0, 5)
            .Select(i =>
            {
                var ms = firstMonth.AddMonths(i);
                var me = ms.AddMonths(1);

                var created = all.Count(t => t.CreatedAt >= ms && t.CreatedAt < me);
                var done = all.Count(t =>
                    Norm(t.Status) == "Done" &&
                    ((t.DueDate ?? t.CreatedAt) >= ms && (t.DueDate ?? t.CreatedAt) < me));

                var label = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(ms.Month);
                return new MonthlyBars(label, created, done);
            })
            .ToList();
    }

    private void ApplyDateRange()
    {
        var now = DateTime.UtcNow;

        switch (Range)
        {
            case "7":
                FromDate = now.AddDays(-7);
                ToDate = now;
                break;
            case "90":
                FromDate = now.AddDays(-90);
                ToDate = now;
                break;
            case "custom":
                FromDate = Start ?? now.AddDays(-30);
                ToDate = End ?? now;
                break;
            default:
                FromDate = now.AddDays(-30);
                ToDate = now;
                break;
        }
    }

    private static DateTime StartOfWeek(DateTime dt)
    {
        int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
        return dt.AddDays(-diff).Date;
    }

    
        
private static List<DonutSlice> BuildDonut(List<(string key, int count, string color)> data, int total)
{
    var list = new List<DonutSlice>();
    if (total <= 0) 
        return list;

    var ordered = data
        .Where(x => x.count > 0)
        .OrderByDescending(x => x.count)
        .ToList();

    double acc = 0;

    foreach (var (key, count, color) in ordered)
    {
        double pct = (double)count / total * 100.0;

        list.Add(new DonutSlice
        {
            Key = key,
            Count = count,
            Percentage = Math.Round(pct, 1),
            Color = color,
            DashArray = $"{pct:0.###} {100 - pct:0.###}",
            DashOffset = $"-{acc:0.###}"
        });

        acc += pct;
    }

    return list;
}



    private static string BuildLinePath(List<double> values)
    {
        if (values.Count == 0) return "";
        var max = Math.Max(1.0, values.Max());
        var step = 100.0 / (values.Count - 1);

        var pts = values.Select((v, i) =>
        {
            var x = i * step;
            var y = 100 - (v / max * 100);
            return (x, y);
        }).ToList();

        string d = $"M{pts[0].x:0.###} {pts[0].y:0.###}";
        for (int i = 1; i < pts.Count; i++)
            d += $" L{pts[i].x:0.###} {pts[i].y:0.###}";
        return d;
    }
    
        public IActionResult OnGetExport()
        {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        var doc = new PdfDocument();
        doc.Info.Title = "Analytics Report";

        var page = doc.AddPage();
        var gfx = XGraphics.FromPdfPage(page);

        var fontTitle = new XFont("Helvetica", 22, XFontStyle.Bold);
        var font = new XFont("Helvetica", 12);

        gfx.DrawString("Analytics Report", fontTitle, XBrushes.Black, new XPoint(40, 50));
        gfx.DrawString($"Generated: {DateTime.Now}", font, XBrushes.Gray, new XPoint(40, 80));

        using var stream = new MemoryStream();
        doc.Save(stream, false);
        return File(stream.ToArray(), "application/pdf", "analytics.pdf");
        }


    public record DonutSlice
    {
        public string Key { get; init; }
        public int Count { get; init; }
        public double Percentage { get; init; }
        public string Color { get; init; }
        public string DashArray { get; init; }
        public string DashOffset { get; init; }
    }

    public record WeeklyProductivityPoint(string Label, int Completed);
    public record MonthlyBars(string Label, int Created, int Done);
}