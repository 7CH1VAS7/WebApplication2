using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }

            var dashboard = new DashboardViewModel
            {
                TotalProjects = await _context.Projects.CountAsync(),
                TotalDefects = await _context.Defects.CountAsync(),
                OpenDefects = await _context.Defects.CountAsync(d => d.Status != DefectStatus.Closed && d.Status != DefectStatus.Cancelled),
                RecentDefects = await _context.Defects
                    .Include(d => d.Project)
                    .Include(d => d.Assignee)
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(10)
                    .ToListAsync()
            };

            return View("Dashboard", dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }

    public class DashboardViewModel
    {
        public int TotalProjects { get; set; }
        public int TotalDefects { get; set; }
        public int OpenDefects { get; set; }
        public List<Defect> RecentDefects { get; set; } = new();
    }
}