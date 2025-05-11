using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.Data; 
using WebApp.Entities;
using WebApp.RequiredAuthAtts;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace WebApp.Controllers
{
    [RequireAuth]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProjectsController> _logger;
        private const string DefaultImagePath = "/images/projects/project-template.svg";
        private const string UploadsFolder = "images/projects";

        public ProjectsController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<ProjectsController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        [Route("admin/projects")]
        public IActionResult Index()
        {
            var viewModel = new ProjectsViewModel
            {
                Projects = GetProjectsFromDb(),
                AddProjectFormData = new AddProjectViewModel
                {
                    Clients = GetClientsFromDb(),
                    Members = GetMembersFromDb(),
                    Statuses = GetStatusesFromDb()
                },
                EditProjectFormData = new EditProjectViewModel
                {
                    Clients = GetClientsFromDb(),
                    Members = GetMembersFromDb(),
                    Statuses = GetStatusesFromDb()
                }
            };

            return View(viewModel);
        }

        [HttpPost("admin/projects/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            string imagePath = DefaultImagePath;

            if (model.ProjectImage != null && model.ProjectImage.Length > 0)
            {
                imagePath = await SaveProjectImageAsync(model.ProjectImage);
            }

            var project = new Project
            {
                ProjectName = model.ProjectName,
                ProjectImage = imagePath,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Budget = model.Budget,
                ClientId = model.ClientId,
                StatusId = model.StatusId
            };

            _context.Projects.Add(project);

            if (model.MemberId != Guid.Empty)
            {
                var member = await _context.Members.FindAsync(model.MemberId);
                if (member != null)
                {
                    project.Members.Add(member);
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost("admin/projects/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("admin/projects/get/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var result = new
            {
                id = project.Id,
                projectName = project.ProjectName,
                clientId = project.ClientId,
                memberId = project.Members.FirstOrDefault()?.Id,
                budget = project.Budget,
                statusId = project.StatusId,
                startDate = project.StartDate.ToString("yyyy-MM-dd"),
                endDate = project.EndDate.ToString("yyyy-MM-dd"),
                description = project.Description,
                projectImage = project.ProjectImage
            };

            return Json(result);
        }

        [HttpPost("admin/projects/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProjectViewModel model)
        {
            try
            {
                _logger.LogInformation($"Received edit request for project {model.Id}");

                var project = await _context.Projects
                    .Include(p => p.Members)
                    .FirstOrDefaultAsync(p => p.Id == model.Id);

                if (project == null)
                {
                    _logger.LogWarning($"Project {model.Id} not found");
                    return NotFound();
                }

                if (!string.IsNullOrWhiteSpace(model.ProjectName))
                {
                    project.ProjectName = model.ProjectName;
                }

                if (model.ClientId != Guid.Empty)
                {
                    project.ClientId = model.ClientId;
                }

                if (!string.IsNullOrWhiteSpace(model.Description))
                {
                    project.Description = model.Description;
                }

                if (model.StartDate != default(DateTime))
                {
                    project.StartDate = model.StartDate;
                }

                if (model.EndDate != default(DateTime))
                {
                    project.EndDate = model.EndDate;
                }

                if (model.Budget > 0)
                {
                    project.Budget = model.Budget;
                }

                if (model.StatusId != Guid.Empty)
                {
                    project.StatusId = model.StatusId;
                }

                if (model.ProjectImage != null && model.ProjectImage.Length > 0)
                {
                    project.ProjectImage = await SaveProjectImageAsync(model.ProjectImage);
                }

                if (model.MemberId != Guid.Empty)
                {
                    var member = await _context.Members.FindAsync(model.MemberId);
                    if (member != null)
                    {
                        project.Members.Clear();
                        project.Members.Add(member);
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating project {model.Id}");
                return Json(new { success = false, errors = new[] { "An error occurred while updating the project." } });
            }
        }

        #region Private Methods

        private IEnumerable<ProjectViewModel> GetProjectsFromDb()
        {
            return _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Members)
                .Include(p => p.Status)
                .Select(p => new ProjectViewModel
                {
                    Id = p.Id.ToString(),
                    ProjectName = p.ProjectName,
                    ClientName = p.Client.ClientName,
                    ProjectImage = p.ProjectImage,
                    Description = p.Description,
                    TimeLeft = p.EndDate <= DateTime.Now
                        ? "0 days left"
                        : ((p.EndDate - DateTime.Now).Days > 30
                            ? "30+ days left"
                            : $"{((p.EndDate - DateTime.Now).Days)} days left"),
                    Members = p.Members.Select(m => m.MemberImage).ToList()
                })
                .ToList();
        }

        private IEnumerable<SelectListItem> GetClientsFromDb()
        {
            return _context.Clients
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.ClientName
                })
                .ToList();
        }

        private IEnumerable<SelectListItem> GetMembersFromDb()
        {
            return _context.Members
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.MemberFirstName + " " + m.MemberLastName
                })
                .ToList();
        }

        private IEnumerable<SelectListItem> GetStatusesFromDb()
        {
            return _context.Statuses
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToList();
        }

        private async Task<string> SaveProjectImageAsync(IFormFile image)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, UploadsFolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return $"/{UploadsFolder}/{uniqueFileName}";
        }

        #endregion
    }
}
