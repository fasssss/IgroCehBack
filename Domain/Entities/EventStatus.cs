
namespace Domain.Entities
{
    public class EventStatus
    {
        public Enums.EventStatusId Id { get; set; }
        public string Name { get; set; }
        public string UserFriendlyName { get; set; }
        public int Order {  get; set; }
        public List<Event> Events { get; set; } = new List<Event>();
    }
}
