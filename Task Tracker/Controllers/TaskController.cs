using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Task_Tracker.Hubs;
using Task_Tracker.Models;
using Task_Tracker.Services;

namespace Task_Tracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _service;
        private readonly IHubContext<TaskHub> _hub;

        public TaskController(ITaskService service, IHubContext<TaskHub> hub)
        {
            _service = service;
            _hub = hub;
        }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var task = await _service.GetByIdAsync(id);
            return task == null ? NotFound() : Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskItem task)
        {
            var created = await _service.CreateAsync(task);
            await _hub.Clients.All.SendAsync("TaskCreated", created);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TaskItem task)
        {
            if (id != task.Id) return BadRequest();
            var updated = await _service.UpdateAsync(task);
            await _hub.Clients.All.SendAsync("TaskUpdated", updated);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            await _hub.Clients.All.SendAsync("TaskDeleted", id);
            return NoContent();
        }
    }
}
