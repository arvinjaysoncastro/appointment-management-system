using AppointmentManagementSystem.Application.DTOs;
using AppointmentManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentManagementSystem.API.Controllers;

[ApiController]
[Route("appointments")]
public sealed class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _service;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentService service,
        ILogger<AppointmentsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all appointments.");

        var result = await _service.GetAppointmentsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting appointment {AppointmentId}.", id);

        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Appointment {AppointmentId} was not found.", id);
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating appointment {Title} from {Start} to {End}",
            request.Title,
            request.Start,
            request.End);

        var created = await _service.CreateAsync(request, cancellationToken);
        _logger.LogInformation("Created appointment {AppointmentId}", created.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating appointment {AppointmentId} to {Title} from {Start} to {End}",
            id,
            request.Title,
            request.Start,
            request.End);

        await _service.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting appointment {AppointmentId}", id);

        await _service.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Deleted appointment {AppointmentId}", id);

        return NoContent();
    }
}