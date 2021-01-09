namespace FlashMemX.Core
{
    public class CardMetadata
    {
        public long Id { get; set; }
        public long DeckId { get; set; }
        public long SchemaId { get; set; }
        public long ContentId { get; set; }
        public long TemplateId { get; set; }
        public int Easiness { get; set; }
        public long NextReview { get; set; }
        public long ActualReview { get; set; }
        public long LastReview { get; set; }
        public int Stage { get; set; }
        public float PercentageOverdue { get; set; }
        public float AverageEasiness { get; set; }
        public float AverageSuccessRate { get; set; }
    }
}
