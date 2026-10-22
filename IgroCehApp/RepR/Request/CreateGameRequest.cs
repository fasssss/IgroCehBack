using API.Helpers;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class CreateGameRequest
    {
        public string GameName { get; set; }
        public IFormFile Image { get; set; }
    }

    // the cover is optional, the front sends an empty file when there is none
    public class CreateGameValidator : Validator<CreateGameRequest>
    {
        public CreateGameValidator()
        {
            RuleFor(r => r.Image).Cascade(CascadeMode.Stop)
                .Must(image => image.ContentType != null && UploadedImages.Extensions.ContainsKey(image.ContentType))
                .WithMessage("Image must be PNG, JPEG, WebP or GIF")
                .Must(image => image.Length <= UploadedImages.MaxSize)
                .WithMessage("Image must be up to 10 MB")
                .When(r => r.Image is { Length: > 0 });
        }
    }
}
