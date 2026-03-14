using AppointmentManagementSystem.Application.DTOs;
using AppointmentManagementSystem.Application.Interfaces;
using FluentValidation;
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
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
    {
        try
        {
            var created = await _service.CreateAsync(request);
            _logger.LogInformation("Created appointment {AppointmentId}", created.Id);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateAppointmentRequest request)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, request);
            return Ok(updated);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        _logger.LogInformation("Deleted appointment {AppointmentId}", id);

        return NoContent();
    }
}