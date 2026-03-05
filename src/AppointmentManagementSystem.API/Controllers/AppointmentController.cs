using AppointmentManagementSystem.Application.DTOs;
using AppointmentManagementSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentManagementSystem.API.Controllers;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AppointmentSummaryDto>>> Search(
        [FromQuery] DateTime date,
        CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.SearchAsync(date, cancellationToken);
        return Ok(appointments);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentDetailsDto>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.GetAsync(id, cancellationToken);
        if (appointment is null)
        {
            return NotFound();
        }

        return Ok(appointment);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDetailsDto>> Create(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        await _appointmentService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _appointmentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
