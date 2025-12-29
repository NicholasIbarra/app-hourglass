using FluentValidation;
using SharedKernel.Queries;

namespace Scheduler.Application.Validators;

public class PaginationQueryValidator : AbstractValidator<PaginationQuery>
{
    public PaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.")
            .LessThan(1000)
            .WithMessage("Page size must be less than 1000.");

        RuleFor(x => x.SortDirection)
            .IsInEnum()
            .When(x => !string.IsNullOrEmpty(x.SortBy))
            .WithMessage("Sort direction must be either 'Asc' or 'Desc'.");
    }
}
