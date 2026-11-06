namespace Application.DTO
{
    // a link of a PSN account on its way: the online id the user named and the code its About me has to show
    public class PendingPsnLinkObject
    {
        public string OnlineId { get; set; }
        public string Code { get; set; }
    }
}
