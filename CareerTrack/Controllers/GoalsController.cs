using CareerTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers
{
    public class GoalsController : Controller
    {
        private readonly AppDbContext _context;

        public GoalsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: GoalsController
        [Authorize]
        public IActionResult Index()
        {
            var goals = _context.Goals.ToList();
            return View(goals);
        }

        // GET: GoalsController/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            var goal = _context.Goals.FirstOrDefault(m => m.Id == id);
            if (goal == null)
                return NotFound();

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
                goal.startDate = DateTime.SpecifyKind(goal.startDate, DateTimeKind.Utc);
                goal.targetDate = DateTime.SpecifyKind(goal.targetDate, DateTimeKind.Utc);

                if (goal.endDate.HasValue)
                {
                    goal.endDate = DateTime.SpecifyKind(goal.endDate.Value, DateTimeKind.Utc);
                }

                _context.Add(goal);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(goal);
        }

        // GET: GoalsController/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var goal = _context.Goals.Find(id);
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

                    goal.startDate = DateTime.SpecifyKind(goal.startDate, DateTimeKind.Utc);
                    goal.targetDate = DateTime.SpecifyKind(goal.targetDate, DateTimeKind.Utc);

                    if (goal.endDate.HasValue)
                    {
                        goal.endDate = DateTime.SpecifyKind(goal.endDate.Value, DateTimeKind.Utc);
                    }

                    _context.Update(goal);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GoalExists(goal.Id))
                        return NotFound();
                    else
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

            var goal = _context.Goals.FirstOrDefault(m => m.Id == id);
            if (goal == null)
                return NotFound();

            return View(goal);
        }

        // POST: GoalsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var goal = _context.Goals.Find(id);
            if (goal != null)
            {
                _context.Goals.Remove(goal);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool GoalExists(int id)
        {
            return _context.Goals.Any(e => e.Id == id);
        }
    }
}
