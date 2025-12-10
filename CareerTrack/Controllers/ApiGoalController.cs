using CareerTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiGoalController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApiGoalController(AppDbContext context)
        {
            _context = context;
        }

        // GET: goal/goals
        [HttpGet]
        [Route("goals")]
        public IActionResult GetGoals()
        {
            var goals = _context.Goals.ToList();
            return Ok(goals);
        }

        // GET: goal/5
        [HttpGet("{id}")]
        public IActionResult GetGoal(int id)
        {
            var goal = _context.Goals.Find(id);
            if (goal == null)
                return NotFound();
            return Ok(goal);
        }

        // POST: goal
        [HttpPost]
        public IActionResult CreateGoal([FromBody] Goal goal)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            goal.startDate = DateTime.SpecifyKind(goal.startDate, DateTimeKind.Utc);
            goal.targetDate = DateTime.SpecifyKind(goal.targetDate, DateTimeKind.Utc);

            if (goal.endDate.HasValue)
            {
                goal.endDate = DateTime.SpecifyKind(goal.endDate.Value, DateTimeKind.Utc);
            }

            _context.Goals.Add(goal);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetGoal), new { id = goal.Id }, goal);
        }

        // PUT: goal/5
        [HttpPut("{id}")]
        public IActionResult UpdateGoal(int id, [FromBody] Goal goal)
        {
            if (id != goal.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(goal).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GoalExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }


        // DELETE: goal/5
        [HttpDelete("{id}")]
        public IActionResult DeleteGoal(int id)
        {
            var goal = _context.Goals.Find(id);
            if (goal == null)
                return NotFound();

            _context.Goals.Remove(goal);
            _context.SaveChanges();
            return NoContent();
        }

        private bool GoalExists(int id)
        {
            return _context.Goals.Any(e => e.Id == id);
        }
    }
}
