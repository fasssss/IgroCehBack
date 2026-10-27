using Application.Consts;
using Application.DTO;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class PublishReviewRepliesRequest
    {
        public string EventRecordId { get; set; }
        public List<ReviewReplyObject> Replies { get; set; }
    }

    public class PublishReviewRepliesValidator : Validator<PublishReviewRepliesRequest>
    {
        public PublishReviewRepliesValidator()
        {
            RuleFor(r => r.Replies).Must(replies => replies != null && replies.Any(reply => !string.IsNullOrWhiteSpace(reply.Text)))
                .WithMessage("Answer at least one section");
            RuleForEach(r => r.Replies).ChildRules(reply =>
            {
                reply.RuleFor(r => r.Text).MaximumLength(ReviewLimits.ReplyMaxLength);
            });
        }
    }
}
