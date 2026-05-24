using FluentValidation;
using Warehouse.Models;

namespace Warehouse.Validators
{
    /// <summary>
    /// Defines the validation rules for the Product entity.
    /// Ensures data integrity for required fields, positive monetary values, and valid enums.
    /// </summary>
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.CategoryId)
                .NotEmpty();

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Price)
                .NotNull();

            When(x => x.Price != null, () =>
            {
                RuleFor(x => x.Price.Amount)
                    .GreaterThan(0);

                RuleFor(x => x.Price.Currency)
                    .IsInEnum();
            });

            RuleFor(x => x.Unit)
                .IsInEnum();
        }
    }
}