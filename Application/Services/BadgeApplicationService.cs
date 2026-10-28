using Application.ApplicationInterfaces;
using Application.Consts;
using Application.DTO;
using Application.Enum;
using Application.Interfaces;
using Domain.Entities;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Application.Services
{
    public class BadgeApplicationService : IBadgeApplicationService
    {
        private static readonly XNamespace SvgNamespace = "http://www.w3.org/2000/svg";
        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        // an SVG opened by its link runs its scripts on the site of the API; the API forbids them by the headers of the image as well
        private static readonly string[] ForbiddenSvgElements = { "script", "foreignObject" };
        // a link of an SVG may lead only inside it or to an embedded picture
        private static readonly Regex InnerLink = new("^\\s*(#|data:image/(png|jpeg|gif|webp)[;,])", RegexOptions.IgnoreCase);

        private readonly IBadgeRepository _badgeRepository;
        private readonly IGlobalEventRepository _globalEventRepository;

        public BadgeApplicationService(IBadgeRepository badgeRepository, IGlobalEventRepository globalEventRepository)
        {
            _badgeRepository = badgeRepository;
            _globalEventRepository = globalEventRepository;
        }

        // the administrator sees which events give a badge already before picking it for another one
        public async Task<List<BadgeObject>> GetAllAsync()
        {
            var uses = (await _globalEventRepository.CustomToListAsync(_globalEventRepository
                .Where(ge => ge.BadgeId != null)
                .OrderByDescending(ge => ge.CreatedAt)
                .Select(ge => new GlobalEventBadgeUseObject
                {
                    BadgeId = ge.BadgeId,
                    GlobalEventId = ge.Id,
                    GlobalEventName = ge.Name,
                    PassedCount = ge.Participants.Count(p => p.PassedAt != null),
                }))).ToLookup(use => use.BadgeId);
            var badges = await _badgeRepository.CustomToListAsync(_badgeRepository
                .Where(b => true)
                .OrderBy(b => b.CreatedAt)
                .ThenBy(b => b.Id)
                .Select(b => new BadgeObject { Id = b.Id, Name = b.Name, CreatedAt = b.CreatedAt }));
            foreach (var badge in badges)
            {
                badge.Uses = uses[badge.Id].ToList();
            }

            return badges.ToList();
        }

        public async Task<(UploadBadgeResult Result, BadgeObject Badge)> UploadAsync(string userId, string name, string contentType, byte[] content)
        {
            var isImage = contentType switch
            {
                BadgeLimits.Svg => IsSafeSvg(content),
                BadgeLimits.Png => content.AsSpan().StartsWith(PngSignature),
                BadgeLimits.Webp => content.Length > 12 && content.AsSpan(0, 4).SequenceEqual("RIFF"u8) && content.AsSpan(8, 4).SequenceEqual("WEBP"u8),
                _ => false,
            };
            if (!isImage)
            {
                return (UploadBadgeResult.InvalidImage, null);
            }

            var badge = await _badgeRepository.AddAsync(new Badge
            {
                Id = Guid.NewGuid().ToString(),
                Name = name.Trim(),
                Content = content,
                ContentType = contentType,
                CreatedById = userId,
            });
            await _badgeRepository.SaveAsync();

            return (UploadBadgeResult.Uploaded, new BadgeObject { Id = badge.Id, Name = badge.Name, CreatedAt = badge.CreatedAt, Uses = new List<GlobalEventBadgeUseObject>() });
        }

        public async Task<BadgeImageObject> GetImageAsync(string badgeId)
        {
            return (await _badgeRepository.CustomToListAsync(_badgeRepository
                .Where(b => b.Id == badgeId)
                .Select(b => new BadgeImageObject { Content = b.Content, ContentType = b.ContentType }))).FirstOrDefault();
        }

        // an SVG document without scripts, event handlers and links outside of itself; a DTD is refused, so entities can't blow it up
        private static bool IsSafeSvg(byte[] content)
        {
            try
            {
                using var reader = XmlReader.Create(new MemoryStream(content), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
                var root = XDocument.Load(reader).Root;
                return root?.Name == SvgNamespace + "svg" && !root.DescendantsAndSelf().Any(element =>
                    ForbiddenSvgElements.Contains(element.Name.LocalName, StringComparer.OrdinalIgnoreCase)
                    || element.Attributes().Any(attribute =>
                        attribute.Name.LocalName.StartsWith("on", StringComparison.OrdinalIgnoreCase)
                        || attribute.Name.LocalName == "href" && !InnerLink.IsMatch(attribute.Value)
                        // animate and set can put a script link into an attribute
                        || Regex.Replace(attribute.Value, "\\s", "").Contains("javascript:", StringComparison.OrdinalIgnoreCase)));
            }
            catch (XmlException)
            {
                return false;
            }
        }
    }
}
