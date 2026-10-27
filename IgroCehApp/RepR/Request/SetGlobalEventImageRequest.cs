using API.Helpers;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class SetGlobalEventImageRequest
    {
        public string GlobalEventId { get; set; }
        public IFormFile Image { get; set; }
    }

    public class SetGlobalEventImageValidator : Validator<SetGlobalEventImageRequest>
    {
        public SetGlobalEventImageValidator()
        {
            RuleFor(r => r.GlobalEventId).NotEmpty();
            RuleFor(r => r.Image).Cascade(CascadeMode.Stop)
                .Must(image => image is { Length: > 0 }).WithMessage("The image is empty")
                .Must(image => image.ContentType != null && UploadedImages.Extensions.ContainsKey(image.ContentType))
                .WithMessage("Image must be PNG, JPEG, WebP or GIF")
                .Must(image => image.Length <= UploadedImages.MaxSize)
                .WithMessage("Image must be up to 10 MB");
        }
    }
}
