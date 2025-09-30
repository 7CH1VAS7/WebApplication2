using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Text;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Reports
        public IActionResult Index()
        {
            return View();
        }

        // GET: Reports/Defects
        public async Task<IActionResult> DefectsReport(string exportType)
        {
            var defects = await _context.Defects
                .Include(d => d.Project)
                .Include(d => d.Assignee)
                .Include(d => d.Creator)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            ViewBag.TotalDefects = defects.Count;
            ViewBag.NewDefects = defects.Count(d => d.Status == DefectStatus.New);
            ViewBag.InProgressDefects = defects.Count(d => d.Status == DefectStatus.InProgress);
            ViewBag.ClosedDefects = defects.Count(d => d.Status == DefectStatus.Closed);

            if (exportType == "csv")
            {
                return ExportDefectsToCsv(defects);
            }
            else if (exportType == "excel")
            {
                return ExportDefectsToExcel(defects);
            }

            return View(defects);
        }

        // GET: Reports/Projects
        public async Task<IActionResult> ProjectsReport()
        {
            var projects = await _context.Projects
                .Include(p => p.Defects)
                .Select(p => new ProjectReportViewModel
                {
                    Project = p,
                    TotalDefects = p.Defects.Count,
                    NewDefects = p.Defects.Count(d => d.Status == DefectStatus.New),
                    InProgressDefects = p.Defects.Count(d => d.Status == DefectStatus.InProgress),
                    ClosedDefects = p.Defects.Count(d => d.Status == DefectStatus.Closed),
                    OverdueDefects = p.Defects.Count(d => d.DueDate.HasValue && d.DueDate < DateTime.Today && d.Status != DefectStatus.Closed)
                })
                .ToListAsync();

            return View(projects);
        }

        // GET: Reports/Statistics
        public async Task<IActionResult> Statistics()
        {
            var defects = await _context.Defects.ToListAsync();
            var projects = await _context.Projects.ToListAsync();

            var model = new StatisticsViewModel
            {
                TotalProjects = projects.Count,
                TotalDefects = defects.Count,
                DefectsByStatus = defects.GroupBy(d => d.Status)
                    .ToDictionary(g => g.Key, g => g.Count()),
                DefectsByPriority = defects.GroupBy(d => d.Priority)
                    .ToDictionary(g => g.Key, g => g.Count()),
                DefectsByMonth = defects.Where(d => d.CreatedAt.Year == DateTime.Now.Year)
                    .GroupBy(d => d.CreatedAt.Month)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageResolutionTime = CalculateAverageResolutionTime(defects)
            };

            return View(model);
        }

        // Private methods for export
        private IActionResult ExportDefectsToCsv(List<Defect> defects)
        {
            var csv = new StringBuilder();
            csv.AppendLine("ID;Заголовок;Проект;Статус;Приоритет;Исполнитель;Создатель;Дата создания;Срок");

            foreach (var defect in defects)
            {
                csv.AppendLine($"{defect.Id};{defect.Title};{defect.Project?.Name};{defect.Status};{defect.Priority};{defect.Assignee?.UserName};{defect.Creator?.UserName};{defect.CreatedAt:dd.MM.yyyy};{defect.DueDate:dd.MM.yyyy}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"defects_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private IActionResult ExportDefectsToExcel(List<Defect> defects)
        {
            // Для простоты используем CSV как Excel
            // В реальном проекте можно использовать библиотеку типа ClosedXML
            var csv = new StringBuilder();
            csv.AppendLine("ID\tЗаголовок\tПроект\tСтатус\tПриоритет\tИсполнитель\tСоздатель\tДата создания\tСрок");

            foreach (var defect in defects)
            {
                csv.AppendLine($"{defect.Id}\t{defect.Title}\t{defect.Project?.Name}\t{defect.Status}\t{defect.Priority}\t{defect.Assignee?.UserName}\t{defect.Creator?.UserName}\t{defect.CreatedAt:dd.MM.yyyy}\t{defect.DueDate:dd.MM.yyyy}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "application/vnd.ms-excel", $"defects_report_{DateTime.Now:yyyyMMdd_HHmmss}.xls");
        }

        private TimeSpan? CalculateAverageResolutionTime(List<Defect> defects)
        {
            var closedDefects = defects.Where(d => d.Status == DefectStatus.Closed).ToList();
            if (!closedDefects.Any()) return null;

            var totalResolutionTime = closedDefects
                .Where(d => d.CreatedAt != default)
                .Average(d => (DateTime.UtcNow - d.CreatedAt).TotalDays);

            return TimeSpan.FromDays(totalResolutionTime);
        }
    }

    // ViewModels for reports
    public class ProjectReportViewModel
    {
        public Project Project { get; set; }
        public int TotalDefects { get; set; }
        public int NewDefects { get; set; }
        public int InProgressDefects { get; set; }
        public int ClosedDefects { get; set; }
        public int OverdueDefects { get; set; }
    }

    public class StatisticsViewModel
    {
        public int TotalProjects { get; set; }
        public int TotalDefects { get; set; }
        public Dictionary<DefectStatus, int> DefectsByStatus { get; set; }
        public Dictionary<DefectPriority, int> DefectsByPriority { get; set; }
        public Dictionary<int, int> DefectsByMonth { get; set; }
        public TimeSpan? AverageResolutionTime { get; set; }
    }
}