using AppointmentManagementSystem.Application.DTOs;
using FluentValidation;

namespace AppointmentManagementSystem.Application.Validators;

public sealed class CreateAppointmentValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Start)
            .NotEmpty();

        RuleFor(x => x.End)
            .GreaterThan(x => x.Start)
            .WithMessage("End must be after start.");
    }
}