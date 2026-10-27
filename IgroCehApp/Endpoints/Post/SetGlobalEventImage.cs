using API.Authorization;
using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Post
{
    // the image of a global event is uploaded right after it is created and can be replaced at any time
    public class SetGlobalEventImage : Endpoint<SetGlobalEventImageRequest, Results<Ok<SetGlobalEventImageResponse>, NotFound>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public SetGlobalEventImage(IGlobalEventApplicationService globalEventApplicationService, IWebHostEnvironment hostingEnvironment)
        {
            _globalEventApplicationService = globalEventApplicationService;
            _hostingEnvironment = hostingEnvironment;
        }

        public override void Configure()
        {
            Post("/api/setGlobalEventImage");
            AllowFileUploads();
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok<SetGlobalEventImageResponse>, NotFound>> ExecuteAsync(SetGlobalEventImageRequest request, CancellationToken ct)
        {
            if (await _globalEventApplicationService.GetAsync(request.GlobalEventId) == null)
            {
                return TypedResults.NotFound();
            }

            var fileName = Guid.NewGuid().ToString("N") + UploadedImages.Extensions[request.Image.ContentType];
            var uploadFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "Upload", "GlobalEvents");
            Directory.CreateDirectory(uploadFolder);
            await using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.CreateNew))
            {
                await request.Image.CopyToAsync(fileStream, ct);
            }

            var imageUrl = $"/Upload/GlobalEvents/{fileName}";
            if (await _globalEventApplicationService.SetImageAsync(request.GlobalEventId, imageUrl) != ChangeGlobalEventResult.Changed)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new SetGlobalEventImageResponse { ImageUrl = imageUrl });
        }
    }
}
