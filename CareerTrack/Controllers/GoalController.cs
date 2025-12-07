using CareerTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoalController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GoalController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Goal
        [HttpGet]
        public IActionResult GetGoals()
        {
            var goals = _context.Goals.ToList();
            return Ok(goals);
        }

        // GET: api/Goal/5
        [HttpGet("{id}")]
        public IActionResult GetGoal(int id)
        {
            var goal = _context.Goals.FindAsync(id);
            if (goal == null)
                return NotFound();
            return Ok(goal);
        }

        // POST: api/Goal
        [HttpPost]
        public IActionResult CreateGoal([FromBody] Goal goal)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Goals.Add(goal);
            _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGoal), new { id = goal.Id }, goal);
        }

        // PUT: api/Goal/5
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
                _context.SaveChangesAsync();
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

        // DELETE: api/Goal/5
        [HttpDelete("{id}")]
        public IActionResult DeleteGoal(int id)
        {
            var goal = _context.Goals.Find(id);
            if (goal == null)
                return NotFound();

            _context.Goals.Remove(goal);
            _context.SaveChangesAsync();
            return NoContent();
        }

        private bool GoalExists(int id)
        {
            return _context.Goals.Any(e => e.Id == id);
        }
    }
}
