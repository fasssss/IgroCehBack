namespace API.Helpers
{
    public static class WebSocketRooms
    {
        public static string Event(string eventId) => $"event{eventId}";
    }

    public static class EventRoomMessages
    {
        public const string AddUserToEvent = "addUserToEvent";
        public const string RemoveUserFromEvent = "removeUserFromEvent";
        public const string UpdateEventStage = "updateEventStage";
        public const string ShuffleUsers = "shuffleUsers";
        public const string SuggestGame = "suggestGame";
        public const string SubmitPassing = "submitPassing";
        public const string DeleteEvent = "deleteEvent";
    }
}
