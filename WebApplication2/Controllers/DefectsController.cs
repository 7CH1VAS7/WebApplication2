using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [Authorize]
    public class DefectsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IFileService _fileService;

        public DefectsController(AppDbContext context, UserManager<IdentityUser> userManager, IFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
        }

        // GET: Defects
        public async Task<IActionResult> Index(string search, string status, int? projectId, string priority)
        {
            var defects = _context.Defects
                .Include(d => d.Project)
                .Include(d => d.Assignee)
                .Include(d => d.Creator)
                .AsQueryable();

            // Фильтрация
            if (!string.IsNullOrEmpty(search))
            {
                defects = defects.Where(d => d.Title.Contains(search) || d.Description.Contains(search));
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<DefectStatus>(status, out var statusEnum))
            {
                defects = defects.Where(d => d.Status == statusEnum);
            }

            if (projectId.HasValue)
            {
                defects = defects.Where(d => d.ProjectId == projectId);
            }

            if (!string.IsNullOrEmpty(priority) && Enum.TryParse<DefectPriority>(priority, out var priorityEnum))
            {
                defects = defects.Where(d => d.Priority == priorityEnum);
            }

            ViewData["Projects"] = await _context.Projects.ToListAsync();
            return View(await defects.ToListAsync());
        }

        // GET: Defects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var defect = await _context.Defects
                .Include(d => d.Project)
                .Include(d => d.Assignee)
                .Include(d => d.Creator)
                .Include(d => d.Comments)
                    .ThenInclude(c => c.Author)
                .Include(d => d.Attachments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (defect == null)
            {
                return NotFound();
            }

            return View(defect);
        }

        // GET: Defects/Create
        [Authorize(Roles = "Admin,Manager,Engineer")]
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View();
        }

        // POST: Defects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Engineer")]
        public async Task<IActionResult> Create([Bind("Title,Description,Priority,ProjectId,DueDate,AssigneeId")] Defect defect, List<IFormFile> attachments)
        {
            if (ModelState.IsValid)
            {
                defect.CreatorId = _userManager.GetUserId(User);
                defect.CreatedAt = DateTime.UtcNow;
                defect.Status = DefectStatus.New;

                _context.Add(defect);
                await _context.SaveChangesAsync();

                // Обработка вложений
                if (attachments != null && attachments.Any())
                {
                    foreach (var file in attachments)
                    {
                        if (file.Length > 0)
                        {
                            var filePath = await _fileService.SaveFileAsync(file, "defects");

                            var attachment = new DefectAttachment
                            {
                                FileName = Path.GetFileName(filePath),
                                OriginalFileName = file.FileName,
                                FilePath = filePath,
                                ContentType = file.ContentType,
                                FileSize = file.Length,
                                DefectId = defect.Id,
                                UploadedById = _userManager.GetUserId(User)
                            };

                            _context.DefectAttachments.Add(attachment);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            await LoadViewData();
            return View(defect);
        }

        // GET: Defects/Edit/5
        [Authorize(Roles = "Admin,Manager,Engineer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var defect = await _context.Defects.FindAsync(id);
            if (defect == null)
            {
                return NotFound();
            }

            await LoadViewData();
            return View(defect);
        }

        // POST: Defects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Engineer")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,Priority,ProjectId,DueDate,AssigneeId")] Defect defect)
        {
            if (id != defect.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(defect);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DefectExists(defect.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            await LoadViewData();
            return View(defect);
        }

        // POST: Defects/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Engineer,Viewer")]
        public async Task<IActionResult> AddComment(int defectId, string commentText)
        {
            if (string.IsNullOrEmpty(commentText))
            {
                return RedirectToAction("Details", new { id = defectId });
            }

            var comment = new DefectComment
            {
                Text = commentText,
                DefectId = defectId,
                AuthorId = _userManager.GetUserId(User),
                CreatedAt = DateTime.UtcNow
            };

            _context.DefectComments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = defectId });
        }

        // POST: Defects/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Engineer")]
        public async Task<IActionResult> ChangeStatus(int defectId, DefectStatus newStatus)
        {
            var defect = await _context.Defects.FindAsync(defectId);
            if (defect == null)
            {
                return NotFound();
            }

            defect.Status = newStatus;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = defectId });
        }

        private bool DefectExists(int id)
        {
            return _context.Defects.Any(e => e.Id == id);
        }

        private async Task LoadViewData()
        {
            ViewData["ProjectId"] = await _context.Projects.Select(p => new { p.Id, p.Name }).ToListAsync();
            ViewData["AssigneeId"] = await _userManager.Users.Select(u => new { u.Id, u.UserName }).ToListAsync();
        }
    }
}