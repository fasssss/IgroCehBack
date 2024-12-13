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

        public CreateGame(IGameApplicationService gameApplicationService)
        {
            _gameApplicationService = gameApplicationService;
        }

        public override void Configure()
        {
            Post("/api/createGame");
            AllowFileUploads();
        }

        public override async Task<Results<Ok<CreateGameResponse>, BadRequest<string>>> ExecuteAsync(CreateGameRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            using (var memoryStream = new MemoryStream())
            {
                await request.Image.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var createdGame = await _gameApplicationService.CreateGameAsync(new GameObject
                {
                    Name = request.GameName,
                    ImageContent = imageBytes,
                    ImageType = request.Image.ContentType,
                    CreatorId = stringId,
                });

                return TypedResults.Ok(new CreateGameResponse { GameId = createdGame.Id });
            }
        }
    }
}
