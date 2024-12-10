using API.Helpers;
using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    public class CreateGame: Endpoint<CreateGameRequest, Results<Ok, BadRequest<string>>>
    {
        private readonly IGameApplicationService _gameApplicationService;
        private readonly WebSocketHelper _webSocketHelper;

        public CreateGame(IGameApplicationService gameApplicationService, WebSocketHelper webSocketHelper)
        {
            _gameApplicationService = gameApplicationService;
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Post("/api/createGame");
            AllowFileUploads();
        }

        public override async Task<Results<Ok, BadRequest<string>>> ExecuteAsync(CreateGameRequest request, CancellationToken ct)
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
                });
            }

            return TypedResults.Ok();
        }
    }
}
