using AllYourPlates.WebMVC.Data;
using AllYourPlates.WebMVC.Models;
using AllYourPlates.WebMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace AllYourPlates.WebMVC.Controllers
{
    public class PlateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlateController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Plates
        public async Task<IActionResult> Index()
        {
            return View(await _context.Plate.ToListAsync());
        }

        // GET: Plates/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plate = await _context.Plate
                .FirstOrDefaultAsync(m => m.PlateId == id);
            if (plate == null)
            {
                return NotFound();
            }

            return View(plate);
        }

        // GET: Plates/Create
        public IActionResult Create()
        {
            return View(new PlateViewModel()
            {
                Timestamp = DateTime.Now
            }
            );
        }

        // ...

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlateId,Timestamp,Description,PlateFile")] PlateViewModel plateVM)
        {
            var plate = new Plate
            {
                PlateId = Guid.NewGuid(),
                Timestamp = plateVM.Timestamp,
                Description = plateVM.Description
            };

            if (plateVM.PlateFile != null && plateVM.PlateFile.Length > 0)
            {
                var extension = Path.GetExtension(plateVM.PlateFile.FileName).ToLower();
                var newFileName = Path.ChangeExtension(plate.PlateId.ToString(), ".jpeg");
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/plates", newFileName);

                using (var memoryStream = new MemoryStream())
                {
                    await plateVM.PlateFile.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Check if the file is not a JPEG
                    if (extension != ".jpeg" && extension != ".jpg")
                    {
                        // Load the image using ImageSharp
                        using (var image = Image.Load(memoryStream))
                        {
                            // Save it as a JPEG
                            image.Save(filePath, new JpegEncoder());
                        }
                    }
                    else
                    {
                        // If the file is already a JPEG, save it directly
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await plateVM.PlateFile.CopyToAsync(fileStream);
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(plate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(plate);
        }

        // GET: Plates/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plate = await _context.Plate.FindAsync(id);
            if (plate == null)
            {
                return NotFound();
            }
            return View(plate);
        }

        // POST: Plates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("PlateId,Timestamp,Description")] Plate plate)
        {
            if (id != plate.PlateId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlateExists(plate.PlateId))
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
            return View(plate);
        }

        // GET: Plates/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plate = await _context.Plate
                .FirstOrDefaultAsync(m => m.PlateId == id);
            if (plate == null)
            {
                return NotFound();
            }

            return View(plate);
        }

        // POST: Plates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var plate = await _context.Plate.FindAsync(id);
            if (plate != null)
            {
                _context.Plate.Remove(plate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlateExists(Guid id)
        {
            return _context.Plate.Any(e => e.PlateId == id);
        }
    }
}
