using Application.Consts;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class UploadBadgeRequest
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }
    }

    public class UploadBadgeValidator : Validator<UploadBadgeRequest>
    {
        public UploadBadgeValidator()
        {
            RuleFor(r => r.Name).Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("The badge needs a name")
                .MaximumLength(BadgeLimits.NameMaxLength);
            RuleFor(r => r.Image).Cascade(CascadeMode.Stop)
                .Must(image => image is { Length: > 0 }).WithMessage("The image is empty")
                .Must(image => image.ContentType != null && BadgeLimits.ContentTypes.Contains(image.ContentType))
                .WithMessage("Image must be SVG, PNG or WebP")
                .Must(image => image.Length <= BadgeLimits.ImageMaxSize)
                .WithMessage($"Image must be up to {BadgeLimits.ImageMaxSize / 1024} KB");
        }
    }
}
