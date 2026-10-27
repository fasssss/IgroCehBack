using Application.Consts;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class DecideGlobalEventClaimRequest
    {
        public string ClaimId { get; set; }
        public bool? IsApproved { get; set; }
        // the reason of a rejection for the player, may be empty
        public string Comment { get; set; }
    }

    public class DecideGlobalEventClaimValidator : Validator<DecideGlobalEventClaimRequest>
    {
        public DecideGlobalEventClaimValidator()
        {
            RuleFor(r => r.IsApproved).NotNull();
            RuleFor(r => r.Comment).Must(comment => comment == null || comment.Trim().Length <= GlobalEventLimits.ClaimCommentMaxLength);
        }
    }
}
