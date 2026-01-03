using Microsoft.AspNetCore.Mvc;
using ramieChristmaseu.Models;

namespace ramieChristmaseu.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class XmasController : ControllerBase
    {
        private static readonly List<Resolution> _data = new()
        {
        new Resolution { Id = 1, Title = "Walk 20 minutes daily", IsDone = false, CreatedAt = DateTime.UtcNow },
        new Resolution { Id = 2, Title = "Run 20 minutes daily", IsDone = true, CreatedAt = DateTime.UtcNow }
     };

        private static int _nextId = 3;

        // A) GET all (filter + search)
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? isDone, [FromQuery] string? title)
        {
            if (isDone != null && !bool.TryParse(isDone, out _))
            {
                return BadRequest(ApiError.BadRequest(
                    "Validation failed.",
                    "isDone must be true or false"
                ));
            }

            IEnumerable<Resolution> query = _data;

            if (bool.TryParse(isDone, out var done))
                query = query.Where(r => r.IsDone == done);

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(r => r.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            return Ok(new
            {
                items = query.Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.IsDone
                })
            });
        }

        // B) GET by ID
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            if (id <= 0)
                return BadRequest(ApiError.BadRequest("Validation failed.", "id must be greater than zero"));

            var item = _data.FirstOrDefault(r => r.Id == id);
            if (item == null)
                return NotFound(ApiError.NotFound("Resolution not found.", $"id: {id}"));

            return Ok(new
            {
                item.Id,
                item.Title,
                item.IsDone,
                createdAt = item.CreatedAt
            });
        }

        // C) CREATE
        [HttpPost]
        public IActionResult Create([FromBody] Resolution? body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.Title))
            {
                return BadRequest(ApiError.BadRequest(
                    "Validation failed.",
                    "title is required"
                ));
            }

            var resolution = new Resolution
            {
                Id = _nextId++,
                Title = body.Title.Trim(),
                IsDone = false,
                CreatedAt = DateTime.UtcNow
            };

            _data.Add(resolution);

            return CreatedAtAction(nameof(GetById), new { id = resolution.Id }, new
            {
                resolution.Id,
                resolution.Title,
                resolution.IsDone,
                createdAt = resolution.CreatedAt
            });
        }

        // D) UPDATE (full replace)
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Resolution? body)
        {
            if (id <= 0)
                return BadRequest(ApiError.BadRequest("Validation failed.", "route id must be greater than zero"));

            if (body == null || body.Id == 0)
                return BadRequest(ApiError.BadRequest("Validation failed.", "body id is required"));

            if (id != body.Id)
            {
                return BadRequest(new
                {
                    error = "BadRequest",
                    message = "Route id does not match body id.",
                    details = new[]
                    {
                    $"route id: {id}",
                    $"body id: {body.Id}"
                }
                });
            }

            if (string.IsNullOrWhiteSpace(body.Title))
                return BadRequest(ApiError.BadRequest("Validation failed.", "title is required"));

            var existing = _data.FirstOrDefault(r => r.Id == id);
            if (existing == null)
                return NotFound(ApiError.NotFound("Resolution not found.", $"id: {id}"));

            existing.Title = body.Title.Trim();
            existing.IsDone = body.IsDone;
            existing.UpdatedAt = DateTime.UtcNow;

            return Ok(new
            {
                existing.Id,
                existing.Title,
                existing.IsDone,
                updatedAt = existing.UpdatedAt
            });
        }

        // E) DELETE
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
                return BadRequest(ApiError.BadRequest("Validation failed.", "id must be greater than zero"));

            var existing = _data.FirstOrDefault(r => r.Id == id);
            if (existing == null)
                return NotFound(ApiError.NotFound("Resolution not found.", $"id: {id}"));

            _data.Remove(existing);
            return NoContent();
        }
    }
}
