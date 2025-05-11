using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Entities;
using WebApp.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using WebApp.RequiredAuthAtts;

namespace WebApp.Controllers
{
    /// <summary>
    /// Controller responsible for managing team members
    /// </summary>
    [RequireAuth]
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private const string DefaultImagePath = "/images/users/user-template-male-green.svg";
        private const string UploadsFolder = "images/users";

        public MembersController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        /// <summary>
        /// Displays the list of team members
        /// </summary>
        [HttpGet("admin/members")]
        public async Task<IActionResult> Index()
        {
            var members = await GetMembersAsync();
            var membersViewModel = new MembersViewModel
            {
                Members = members,
                AddMemberFormData = new AddMemberViewModel()
            };

            return View(membersViewModel);
        }

        /// <summary>
        /// Displays the add member form
        /// </summary>
        [HttpGet("admin/members/add")]
        public IActionResult Add()
        {
            return View(new AddMemberViewModel());
        }

        /// <summary>
        /// Adds a new team member
        /// </summary>
        [HttpPost("admin/members/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddMemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var members = await GetMembersAsync();
                var membersViewModel = new MembersViewModel
                {
                    Members = members,
                    AddMemberFormData = model
                };

                return View("Index", membersViewModel);
            }

            string imagePath = DefaultImagePath;

            if (model.MemberImage != null && model.MemberImage.Length > 0)
            {
                imagePath = await SaveMemberImageAsync(model.MemberImage);
            }

            var member = new Member
            {
                MemberImage = imagePath,
                MemberFirstName = model.MemberFirstName,
                MemberLastName = model.MemberLastName,
                MemberEmail = model.MemberEmail,
                MemberPhone = model.MemberPhone,
                MemberJobTitle = model.MemberJobTitle,
                MemberAddress = model.MemberAddress,
                MemberBirthDate = model.MemberBirthDate ?? DateOnly.MinValue
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the edit member form
        /// </summary>
        [HttpGet("admin/members/edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            var viewModel = new EditMemberViewModel
            {
                MemberFirstName = member.MemberFirstName,
                MemberLastName = member.MemberLastName,
                MemberEmail = member.MemberEmail,
                MemberPhone = member.MemberPhone,
                MemberJobTitle = member.MemberJobTitle,
                MemberAddress = member.MemberAddress,
                MemberBirthDate = member.MemberBirthDate
            };

            return View(viewModel);
        }

        /// <summary>
        /// Updates an existing team member
        /// </summary>
        [HttpPost("admin/members/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditMemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            if (model.MemberImage != null && model.MemberImage.Length > 0)
            {
                member.MemberImage = await SaveMemberImageAsync(model.MemberImage);
            }

            member.MemberFirstName = model.MemberFirstName;
            member.MemberLastName = model.MemberLastName;
            member.MemberEmail = model.MemberEmail;
            member.MemberPhone = model.MemberPhone;
            member.MemberJobTitle = model.MemberJobTitle;
            member.MemberAddress = model.MemberAddress;
            member.MemberBirthDate = model.MemberBirthDate ?? DateOnly.MinValue;

            _context.Members.Update(member);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        /// <summary>
        /// Deletes a team member
        /// </summary>
        [HttpPost("admin/members/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Gets member details by ID
        /// </summary>
        [HttpGet("admin/members/get/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            var result = new
            {
                id = member.Id,
                memberImage = member.MemberImage,
                memberFirstName = member.MemberFirstName,
                memberLastName = member.MemberLastName,
                memberEmail = member.MemberEmail,
                memberPhone = member.MemberPhone,
                memberJobTitle = member.MemberJobTitle,
                memberAddress = member.MemberAddress,
                memberBirthDate = member.MemberBirthDate.ToString("yyyy-MM-dd")
            };

            return Json(result);
        }

        #region Private Methods

        private async Task<IEnumerable<MemberViewModel>> GetMembersAsync()
        {
            return await _context.Members
                .Select(m => new MemberViewModel
                {
                    Id = m.Id.ToString(),
                    MemberImage = m.MemberImage,
                    MemberFirstName = m.MemberFirstName,
                    MemberLastName = m.MemberLastName,
                    MemberEmail = m.MemberEmail,
                    MemberPhone = m.MemberPhone,
                    MemberJobTitle = m.MemberJobTitle,
                    MemberAddress = m.MemberAddress,
                    MemberBirthDate = m.MemberBirthDate.ToString("yyyy-MM-dd")
                })
                .ToListAsync();
        }

        private async Task<string> SaveMemberImageAsync(IFormFile image)
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