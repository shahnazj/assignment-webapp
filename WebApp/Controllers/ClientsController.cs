using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Entities;
using WebApp.Models;
using WebApp.Requ;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace WebApp.Controllers
{
    /// <summary>
    /// Controller responsible for managing client operations
    /// </summary>
    [RequireAuth]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private const string DefaultImagePath = "/images/users/user-template-male-green.svg";
        private const string UploadsFolder = "images/users";

        public ClientsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        /// <summary>
        /// Displays the list of clients
        /// </summary>
        [HttpGet("admin/clients")]
        public async Task<IActionResult> Index()
        {
            var clients = await GetClientsAsync();
            var clientsViewModel = new ClientsViewModel
            {
                Clients = clients,
                AddClientFormData = new AddClientViewModel()
            };

            return View(clientsViewModel);
        }

        /// <summary>
        /// Adds a new client
        /// </summary>
        [HttpPost("admin/clients/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddClientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var clients = await GetClientsAsync();
                var clientsViewModel = new ClientsViewModel
                {
                    Clients = clients,
                    AddClientFormData = model
                };

                return View("Index", clientsViewModel);
            }

            string imagePath = DefaultImagePath;

            if (model.ClientImage != null && model.ClientImage.Length > 0)
            {
                imagePath = await SaveClientImageAsync(model.ClientImage);
            }

            var client = new Client
            {
                ClientImage = imagePath,
                ClientName = model.ClientName
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the edit client form
        /// </summary>
        [HttpGet("admin/clients/edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            var viewModel = new EditClientViewModel
            {
                Id = client.Id,
                ClientName = client.ClientName
            };

            return View(viewModel);
        }

        /// <summary>
        /// Updates an existing client
        /// </summary>
        [HttpPost("admin/clients/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditClientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            if (model.ClientImage != null && model.ClientImage.Length > 0)
            {
                client.ClientImage = await SaveClientImageAsync(model.ClientImage);
            }

            client.ClientName = model.ClientName;

            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        /// <summary>
        /// Deletes a client
        /// </summary>
        [HttpPost("admin/clients/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Gets client details by ID
        /// </summary>
        [HttpGet("admin/clients/get/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            var result = new
            {
                id = client.Id,
                clientImage = client.ClientImage,
                clientName = client.ClientName
            };

            return Json(result);
        }

        #region Private Methods

        private async Task<IEnumerable<ClientViewModel>> GetClientsAsync()
        {
            return await _context.Clients
                .Select(c => new ClientViewModel
                {
                    Id = c.Id.ToString(),
                    ClientImage = c.ClientImage,
                    ClientName = c.ClientName
                })
                .ToListAsync();
        }

        private async Task<string> SaveClientImageAsync(IFormFile image)
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