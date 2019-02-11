namespace BtcTransmuter.Extension.Email.Triggers
{
    public class ReceivedEmailTriggerParameters
    {
        public string FromEmail { get; set; }
        public string Subject { get; set; }
        public FieldComparer SubjectComparer { get; set; } = FieldComparer.None;
        public string Body { get; set; }
        public FieldComparer BodyComparer { get; set; } = FieldComparer.None;

        public enum FieldComparer
        {
            None,
            Equals,
            Contains
        }
    }
}