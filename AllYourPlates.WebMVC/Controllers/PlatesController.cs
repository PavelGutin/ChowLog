using AllYourPlates.Services;
using AllYourPlates.WebMVC.DataAccess;
using AllYourPlates.WebMVC.Models;
using AllYourPlates.WebMVC.ViewModels;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;


namespace AllYourPlates.WebMVC.Controllers
{
    public class PlateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ThumbnailProcessingService _thumbnailService;
        private readonly ImageDescriptionService _imageDescriptionService;
        private readonly IConfiguration _configuration;

        public PlateController(ApplicationDbContext context, 
            UserManager<IdentityUser> userManager, 
            ThumbnailProcessingService thumbnailService,
            IConfiguration configuration,
            ImageDescriptionService imageDescriptionService
            )
        {
            _context = context;
            _userManager = userManager;
            _thumbnailService = thumbnailService;
            _configuration = configuration;
            _imageDescriptionService = imageDescriptionService;
        }

        // GET: Plates
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var plates = await _context.Plate
                .Where(p => p.User == user)
                .GroupBy(p => p.Timestamp.Date)
                .Select(g => new GroupedPlatesViewModel
                {
                    Date = g.Key,
                    Plates = g.ToList()
                })
                .ToListAsync();

            return View(plates);
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
        public async Task<IActionResult> Create([Bind("PlateId,Timestamp,Description,PlateFiles")] PlateViewModel plateVM)
        {
            var user = await _userManager.GetUserAsync(User);

            if (plateVM.PlateFiles != null && plateVM.PlateFiles.Count > 0)
            {
                foreach (var plateFile in plateVM.PlateFiles)
                {
                    var plate = new Plate
                    {
                        PlateId = Guid.NewGuid(),
                        Timestamp = plateVM.Timestamp,
                        Description = plateVM.Description,
                        User = user
                    };
                    var extension = Path.GetExtension(plateFile.FileName).ToLower();
                    var newFileName = Path.ChangeExtension(plate.PlateId.ToString(), ".jpeg");
                    var filePath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot/plates", newFileName);

                    using (var memoryStream = new MemoryStream())
                    {

                        await plateFile.CopyToAsync(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        var metadata = ImageMetadataReader.ReadMetadata(memoryStream);

                        var dateTaken = metadata.OfType<ExifSubIfdDirectory>()
                            .FirstOrDefault()?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);

                        if (dateTaken.HasValue)
                        {
                            plate.Timestamp = dateTaken.Value;
                        }

                        if (extension != ".jpeg" && extension != ".jpg")
                        {
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            using (var image = Image.Load(memoryStream))
                            {
                                image.Save(filePath, new JpegEncoder());
                            }
                        }
                        else
                        {
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await plateFile.CopyToAsync(fileStream);
                            }
                        }

                        //TODO: Figure out what to pass to these services. The current approach is hacky and will just grab the GUID from the filename
                        _thumbnailService.EnqueueFile(filePath);
                        _imageDescriptionService.EnqueueFile(filePath);

                    }

                    if (ModelState.IsValid)
                    {
                        _context.Add(plate);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
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
        //public void ConfigureServices(IServiceCollection services)
        //{
        //    services.Configure<FormOptions>(options =>
        //    {
        //        options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
        //    });

        //    // Other service configurations...
        //}
    }
}
