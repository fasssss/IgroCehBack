using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    public class CreateGame: Endpoint<CreateGameRequest, Results<Ok<CreateGameResponse>, BadRequest<string>>>
    {
        private readonly IGameApplicationService _gameApplicationService;
        private readonly IHostEnvironment _hostingEnvironment;

        public CreateGame(IGameApplicationService gameApplicationService, IHostEnvironment hostingEnvironment)
        {
            _gameApplicationService = gameApplicationService;
            _hostingEnvironment = hostingEnvironment;
        }

        public override void Configure()
        {
            Post("/api/createGame");
            AllowFileUploads();
        }

        public override async Task<Results<Ok<CreateGameResponse>, BadRequest<string>>> ExecuteAsync(CreateGameRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            string imageUrl = null;
            if(request.Image != null)
            {
                string uploadFolder = Path.Combine(_hostingEnvironment.ContentRootPath, $"Upload/{stringId}/Images/Games");
                Directory.CreateDirectory(uploadFolder);
                var safeGameName = request.Image.FileName.Replace("../", "");
                imageUrl = Path.Combine(uploadFolder, safeGameName);
                using (var memoryStream = new FileStream(imageUrl, FileMode.Create))
                {
                    await request.Image.CopyToAsync(memoryStream);
                }
            }

            var createdGame = await _gameApplicationService.CreateGameAsync(new GameObject
            {
                Name = request.GameName,
                ImageUrl = imageUrl,
                CreatorId = stringId,
            });

            return TypedResults.Ok(new CreateGameResponse { GameId = createdGame.Id });
        }
    }
}
