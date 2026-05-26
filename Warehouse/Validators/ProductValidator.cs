using FluentValidation;
using Warehouse.Models;

namespace Warehouse.Validators
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Barcode)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(50);

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
            });
        }
    }
}