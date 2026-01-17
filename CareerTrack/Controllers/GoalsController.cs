/// 1. SINGLE RESPONSIBILITY PRINCIPLE 
// 5. DEPENDENCY INVERSION PRINCIPLE - ovisit o abstrakcijama kroz DI

using CareerTrack.Decorators;
using CareerTrack.Handlers;
using CareerTrack.Interfaces;
using CareerTrack.Models;
using CareerTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers
{

    public class GoalsController : Controller
    {
        private const string ActionName = "Index";
        private readonly IGoalService _goalService;
        private readonly IUserContextService _userContext;
        private readonly IProgressService _progressService;
        private readonly IGoalExportService _exportService;
        private readonly IGoalHandler _handlerChain;
        private readonly IGoalFactory _goalFactory;

        public GoalsController(AppDbContext context,
             IGoalService goalService,
            IUserContextService userContext,
            IProgressService progressService,
            IGoalExportService exportService,
            IGoalFactory goalFactory)
        {
            _goalService = goalService;
            _userContext = userContext;
            _progressService = progressService;
            _exportService = exportService;
            _goalFactory = goalFactory;


            var validationHandler = new GoalValidationHandler();
            var authorizationHandler = new GoalAuthorizationHandler(context);
            var businessRuleHandler = new GoalBusinessRuleHandler(context);

            _handlerChain = validationHandler;
            validationHandler
                .SetNext(authorizationHandler)
                .SetNext(businessRuleHandler);
        }

        // GET: GoalsController
        [Authorize]
        public IActionResult Index()
        {
            var userId = _userContext.GetCurrentUserId();
            var goals = _goalService.GetUserGoals(userId);

            return View(goals);
        }

        // GET: GoalsController/Details/5
        public IActionResult Details(int? id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id == null) return NotFound();

            var userId = _userContext.GetCurrentUserId();
            var goal = _goalService.GetGoalById(id.Value, userId);

            if (goal == null) return NotFound();

            var progress = _progressService.GetProgress(goal.Id, userId);
            var history = _progressService.GetProgressHistory(goal.Id, userId);

            ViewBag.Progress = progress;
            ViewBag.ProgressHistory = history;

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
                var userId = _userContext.GetCurrentUserId();
                _goalService.CreateGoal(goal, userId);

                return RedirectToAction(ActionName);
            }
            return View(goal);
        }

        // GET: GoalsController/Edit/5
        public IActionResult Edit(int? id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id == null)
                return NotFound();

            var goal = _goalService.GetGoalById(id.Value, _userContext.GetCurrentUserId());

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
                return RedirectToAction(ActionName);
            }
            return View(goal);
        }

        // GET: GoalsController/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var goal = _goalService.GetGoalById(id.Value, _userContext.GetCurrentUserId());
            if (goal == null) return NotFound();

            return View(goal);
        }

        // POST: GoalsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _userContext.GetCurrentUserId();
            _goalService.DeleteGoal(id, userId);

            return RedirectToAction(ActionName);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProgress(int id, int progressPercentage, string? notes)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = _userContext.GetCurrentUserId();
            _progressService.UpdateProgress(id, userId, progressPercentage, notes);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public IActionResult Print(int id, string format = "PDF")
        {
            var userId = _userContext.GetCurrentUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

        // liscov  any goalProgressBAse subclass can be passed here
        public void ShowProgress(IGoalProgress progress)
        {
            if (!ModelState.IsValid)
                BadRequest(ModelState);
            Console.WriteLine(progress.GetProgressDescription());
        }


        [HttpGet]
        public IActionResult Notifications()
        {
            IGoalNotification goalNotify = new GoalNotification("Learn Design Patterns");

            ViewBag.Demo1 = goalNotify.GetDescription();
            goalNotify.SendReminder();
            goalNotify.StatusNotification();

            IGoalNotification goalWithReminder = new ReminderDecorator(goalNotify);

            ViewBag.Demo2 = goalWithReminder.GetDescription();
            goalWithReminder.SendReminder();
            goalWithReminder.StatusNotification();


            IGoalNotification goalWithBoth = new NotificationDecorator(
                new ReminderDecorator(goalNotify)
            );

            ViewBag.Demo3 = goalWithBoth.GetDescription();
            goalWithBoth.SendReminder();
            goalWithBoth.StatusNotification();

            return View();
        }

        [HttpPost]
        public IActionResult CreateGoalVariant(string goalType, string name, DateTime targetDate)
        {
            var userId = _userContext.GetCurrentUserId();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var goal = _goalFactory.CreateGoal(goalType, name, targetDate);

            _goalService.CreateGoal(goal, userId);

            return RedirectToAction(ActionName);
        }



        [HttpPost]
        public IActionResult CreateValidGoal(Goal goal)
        {
            var currentUser = _userContext.GetCurrentUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var request = new GoalRequest
            {
                Goal = goal,
                UserId = currentUser,
                Action = "Create"
            };

            var result = _handlerChain.Handle(request);

            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error);

                if (!string.IsNullOrEmpty(result.Message))
                    ModelState.AddModelError("", result.Message);

                return View(goal);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(ActionName);
        }

        [HttpPost]
        public IActionResult ValidGoalDeleteDelete(int id)
        {
            var currentUser = _userContext.GetCurrentUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var goal = _goalService.GetGoalById(id, currentUser);

            var request = new GoalRequest
            {
                Goal = goal,
                UserId = currentUser,
                Action = "Delete"
            };

            var result = _handlerChain.Handle(request);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(ActionName);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(ActionName);
        }
    }
}

