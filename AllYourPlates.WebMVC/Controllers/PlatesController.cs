using AllYourPlates.Services;
using AllYourPlates.Services.Payloads;
using AllYourPlates.Utilities;
using AllYourPlates.WebMVC.DataAccess;
using AllYourPlates.WebMVC.Models;
using AllYourPlates.WebMVC.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SixLabors.ImageSharp;


namespace AllYourPlates.WebMVC.Controllers
{
    public class PlateController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPlateOrchestrator _plateOrchestrator;
        private readonly IConfiguration _configuration;
        //private readonly IPlateService _plateService;
        private readonly IOptions<ApplicationOptions> _applicationOptions;

        public PlateController(ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            //IPlateService plateService,
            IPlateOrchestrator plateOrchestrator,
            IConfiguration configuration,
            IOptions<ApplicationOptions> applicationOptions)
        {
            _userManager = userManager;
            //_plateService = plateService;
            _plateOrchestrator = plateOrchestrator;
            _configuration = configuration;
            _applicationOptions = applicationOptions;
        }

        // GET: Plates
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            //var data = await _plateService.GetAllPlatesAsync(user);
            var data = await _plateOrchestrator.GetAllPlatesAsync(user);
            var plates = new List<PlateViewModel>();

            //var _imagesRoot = new DirectoryInfo($"{_applicationOptions.Value.DataPath}/Plates");
            var _imagesRoot = "/Plates";

            plates.AddRange(data.Select(p => new PlateViewModel
            {
                PlateId = p.PlateId,
                Timestamp = p.Timestamp,
                ImageUrl = $"{_imagesRoot}/{p.PlateId.ToString()}.jpeg", //TODO this needs to be abstracted out
                Thumbnail = $"{_imagesRoot}/{p.PlateId.ToString()}_thmb.jpeg", 
                Description = p.Description
            }));


            if (TempData["NewPlates"] != null && TempData["NewPlates"] is string newPlatesJson)
            {
                var newPlates = JsonConvert.DeserializeObject<List<Plate>>(newPlatesJson);
                foreach (var newPlate in newPlates)
                {
                    var p = plates.Single(p => p.PlateId == newPlate.PlateId);
                    p.Description = "loading...";
                    p.Thumbnail = "/img/plate_placeholder.png";
                }
            }
            return View(plates);
        }

        //TODO uncomment and fix
        // GET: Plates/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var plate = await _context.Plate
            //    .FirstOrDefaultAsync(m => m.PlateId == id);
            //if (plate == null)
            //{
            //    return NotFound();
            //}

            //return View(plate);
            throw new NotImplementedException();
        }

        // GET: Plates/Create
        public IActionResult Create()
        {
            return View(new CreatePlateViewModel(){});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlateId,Timestamp,Description,PlateFiles")] CreatePlateViewModel plateVM)
        {
            var user = await _userManager.GetUserAsync(User);
            var newPlates = new List<PlateServicePayload>();

            if (plateVM.PlateFiles != null && plateVM.PlateFiles.Count > 0)
            {
                newPlates.AddRange(plateVM.PlateFiles.Select(file => new PlateServicePayload
                {
                    PlateId = Guid.NewGuid(),
                    User = user,
                    File = file
                }));

                //await _plateService.AddAsync(newPlates);
                await _plateOrchestrator.AddAsync(newPlates);
            }

            if (ModelState.IsValid)
            {
                TempData["NewPlates"] = JsonConvert.SerializeObject(newPlates.Select(x => new { x.PlateId }));
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        //TODO uncomment and fix
        // GET: Plates/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var plate = await _context.Plate.FindAsync(id);
            //if (plate == null)
            //{
            //    return NotFound();
            //}
            //return View(plate);
            throw new NotImplementedException();
        }

        //TODO uncomment and fix
        // POST: Plates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("PlateId,Timestamp,Description")] Plate plate)
        {
            //if (id != plate.PlateId)
            //{
            //    return NotFound();
            //}

            //if (ModelState.IsValid)
            //{
            //    try
            //    {
            //        _context.Update(plate);
            //        await _context.SaveChangesAsync();
            //    }
            //    catch (DbUpdateConcurrencyException)
            //    {
            //        if (!PlateExists(plate.PlateId))
            //        {
            //            return NotFound();
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }
            //    return RedirectToAction(nameof(Index));
            //}
            //return View(plate);
            throw new NotImplementedException();
        }

        //TODO uncomment and fix
        // GET: Plates/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var plate = await _plateService.GetPlateAsync((Guid) id);
            var plate = await _plateOrchestrator.GetPlateAsync((Guid) id);
            //var plate = await _context.Plate
            //    .FirstOrDefaultAsync(m => m.PlateId == id);
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
            //var plate = await _plateService.GetPlateAsync(id);
            var plate = await _plateOrchestrator.GetPlateAsync(id);
            if (plate != null)
            {
                //await _plateService.DeletePlateAsync(id);
                await _plateOrchestrator.DeletePlateAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        //TODO uncomment and fix
        private bool PlateExists(Guid id)
        {
            //return _context.Plate.Any(e => e.PlateId == id);
            throw new NotImplementedException();
        }
    }
}

