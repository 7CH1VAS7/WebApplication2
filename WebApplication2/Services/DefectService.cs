using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Services
{
    public class DefectService
    {
        [Authorize]
        public class ProjectsController : Controller
        {
            private readonly ApplicationDbContext _context;

            public ProjectsController(ApplicationDbContext context)
            {
                _context = context;
            }

            // GET: Projects
            public async Task<IActionResult> Index()
            {
                return View(await _context.Projects.ToListAsync());
            }

            // GET: Projects/Create
            [Authorize(Roles = "Admin,Manager")]
            public IActionResult Create()
            {
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            [Authorize(Roles = "Admin,Manager")]
            public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate")] Project project)
            {
                if (ModelState.IsValid)
                {
                    _context.Add(project);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(project);
            }

            // Добавьте остальные методы: Edit, Details, Delete
        }
    }
}
