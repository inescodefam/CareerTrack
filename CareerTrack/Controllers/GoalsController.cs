/// 1. SINGLE RESPONSIBILITY PRINCIPLE 
// 5. DEPENDENCY INVERSION PRINCIPLE - ovisit o abstrakcijama kroz DI

using CareerTrack.Models;
using CareerTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers
{
    public class GoalsController : Controller
    {
        //private readonly AppDbContext _context; /// direktna ovisnost o EF Core-u
        private readonly IGoalService _goalService;
        private readonly IUserContextService _userContext;
        private readonly IProgressService _progressService;
        private readonly IGoalExportService _exportService;


        public GoalsController(AppDbContext context,
             IGoalService goalService,
            IUserContextService userContext,
            IProgressService progressService,
            IGoalExportService exportService)
        {
            //_context = context;

            _goalService = goalService;
            _userContext = userContext;
            _progressService = progressService;
            _exportService = exportService;
        }

        // GET: GoalsController
        [Authorize]
        public IActionResult Index()
        {
            //var goals = _context.Goals.ToList();
            var userId = _userContext.GetCurrentUserId();
            var goals = _goalService.GetUserGoals(userId);

            return View(goals);
        }

        // GET: GoalsController/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userContext.GetCurrentUserId();
            /// no no, ne smijemo sprezati s bazom
            //var goal = _context.Goals.FirstOrDefault(m => m.Id == id);
            var goal = _goalService.GetGoalById(id.Value, userId);

            if (goal == null) return NotFound();

            var progress = _progressService.GetProgress(id.Value, userId);
            var history = _progressService.GetProgressHistory(id.Value, userId);

            //var viewModel = new GoalDetailsViewModel // todo implement view model
            //{
            //    Goal = goal,
            //    Progress = progress,
            //    ProgressHistory = history.ToList()
            //};

            return View(goal);
        }


        // GET: GoalsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: GoalsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Name,Description,targetDate,startDate,endDate")] Goal goal)
        {
            if (ModelState.IsValid)
            {
                // S Single Responsibility Principle (SRP) prekršaj
                // zašto kontroler rukuje datumima
                //goal.startDate = DateTime.SpecifyKind(goal.startDate, DateTimeKind.Utc);
                //goal.targetDate = DateTime.SpecifyKind(goal.targetDate, DateTimeKind.Utc);

                //if (goal.endDate.HasValue)
                //{
                //    // why ??? stupid AI
                //    goal.endDate = DateTime.SpecifyKind(goal.endDate.Value, DateTimeKind.Utc);
                //}

                //// s neba pa u bazu :O ??!!
                //_context.Add(goal);
                //_context.SaveChanges();

                var userId = _userContext.GetCurrentUserId();
                _goalService.CreateGoal(goal, userId);

                return RedirectToAction(nameof(Index));
            }
            return View(goal);
        }

        // GET: GoalsController/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            //var goal = _context.Goals.Find(id);
            var userId = _userContext.GetCurrentUserId();
            var goal = _goalService.GetGoalById(id.Value, userId);

            if (goal == null)
                return NotFound();

            return View(goal);
        }

        // POST: GoalsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Name,Description,targetDate,startDate,endDate")] Goal goal)
        {
            if (id != goal.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {

                    // goal.startDate = DateTime.SpecifyKind(goal.startDate, DateTimeKind.Utc);    SRP
                    //  goal.targetDate = DateTime.SpecifyKind(goal.targetDate, DateTimeKind.Utc);   SRP

                    //if (goal.endDate.HasValue) /// SRP
                    //{
                    //    goal.endDate = DateTime.SpecifyKind(goal.endDate.Value, DateTimeKind.Utc);
                    //}

                    // _context.Update(goal);
                    // _context.SaveChanges();

                    var userId = _userContext.GetCurrentUserId();
                    _goalService.UpdateGoal(goal, userId);
                }
                catch (DbUpdateConcurrencyException)
                {
                   var userId = _userContext.GetCurrentUserId();
                    if (_goalService.GetGoalById(goal.Id, userId) == null)
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(goal);
        }

        // GET: GoalsController/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            //  var goal = _context.Goals.FirstOrDefault(m => m.Id == id); Dipendency Inversion Principle

            var userId = _userContext.GetCurrentUserId();
            var goal = _goalService.GetGoalById(id.Value, userId);
            if (goal == null) return NotFound();


            return View(goal);
        }

        // POST: GoalsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            //var goal = _context.Goals.Find(id); //DIP
            //if (goal != null)
            //{
            //    _context.Goals.Remove(goal);
            //    _context.SaveChanges();
            //}

            var userId = _userContext.GetCurrentUserId();
            _goalService.DeleteGoal(id, userId);

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProgress(int id, int progressPercentage, string? notes)
        {
            var userId = _userContext.GetCurrentUserId();
            _progressService.UpdateProgress(id, userId, progressPercentage, notes);
            return RedirectToAction(nameof(Details), new { id });
        }

        //[HttpGet]
        //public IActionResult Timeline()
        //{
        //    var userId = _userContext.GetCurrentUserId();
        //    var timeline = _timelineService.GenerateTimeline(userId);
        //    return View(timeline);
        //}

        [HttpGet]
        public IActionResult Print(int id, string format = "PDF")
        {
            var userId = _userContext.GetCurrentUserId();

            try
            {
                var fileBytes = _exportService.ExportGoal(id, userId, format);
                var exporter = _exportService.GetAvailableFormats().Contains(format)
                    ? format.ToLower() : "pdf";

                var contentType = format.ToUpper() switch
                {
                    "PDF" => "application/pdf",
                    "EXCEL" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    _ => "application/octet-stream"
                };

                return File(fileBytes, contentType, $"goal-{id}.{exporter}");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to generate {format}: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
