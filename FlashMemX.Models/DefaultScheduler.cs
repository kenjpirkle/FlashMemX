using System.Collections.Generic;
using NodaTime;

namespace FlashMemX.Core
{
    public class DefaultScheduler
    {
        private readonly IList<CardMetadata> reviewingCardQueue;
        private readonly IList<CardMetadata> newCardQueue;

        private CardMetadata currentMetadata;

        public DefaultScheduler(Database database)
        {
            long ms = MidnightTonightInMilliseconds();
            reviewingCardQueue = database.GetCardMetadataDueBefore(ms);
            newCardQueue = database.GetNewCardMetadata(new UniqueCardOrderer());
        }

        private long MidnightTonightInMilliseconds()
        {
            long now = SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds();

            var nowDateTime = new ZonedDateTime(Instant.FromUnixTimeMilliseconds(now),
                                                DateTimeZoneProviders.Bcl.GetSystemDefault());

            return new LocalDateTime(nowDateTime.Year, nowDateTime.Month, nowDateTime.Day, 23, 59, 59)
                                        .InZoneLeniently(DateTimeZoneProviders.Bcl.GetSystemDefault())
                                        .ToInstant()
                                        .ToUnixTimeMilliseconds();
        }

        public bool CardAvailable()
        {
            return reviewingCardQueue.Count + newCardQueue.Count > 0;
        }

        public CardMetadata GetNext()
        {
            if(reviewingCardQueue.Count > 0)
            {
                int mostOverdueIndex = CalculatePercentageOverdues();

                if(reviewingCardQueue[mostOverdueIndex].PercentageOverdue < 1.1f
                   && newCardQueue.Count > 0)
                    currentMetadata = GetNewCardMetadata();
                else
                    currentMetadata = GetCardMetadata(mostOverdueIndex);
            }
            else
                currentMetadata = GetNewCardMetadata();

            return currentMetadata;
        }

        public void GradeCard(ReviewQuality reviewQuality)
        {
            if(reviewQuality.IsWorseThan(ReviewQuality.Difficult))
            {

            }
        }

        public IList<CardMetadata> GetMetadata()
        {
            var metaData = new List<CardMetadata>(reviewingCardQueue.Count + newCardQueue.Count);
            metaData.AddRange(reviewingCardQueue);
            metaData.AddRange(newCardQueue);

            return metaData;
        }

        private int CalculatePercentageOverdues()
        {
            if(reviewingCardQueue.Count == 1)
                return 0;
            else
            {
                long now = SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds();
                long lastRev;
                long actualNextRev;

                int mostOverdueIndex = 0;
                var reviewingCard = reviewingCardQueue[0];

                lastRev = reviewingCard.LastReview;
                actualNextRev = reviewingCard.ActualReview;
                reviewingCard.PercentageOverdue = (now - lastRev) / (actualNextRev - lastRev);

                for(int i = 0; i < reviewingCardQueue.Count; ++i)
                {
                    reviewingCard = reviewingCardQueue[i];
                    lastRev = reviewingCard.LastReview;
                    actualNextRev = reviewingCard.ActualReview;
                    reviewingCard.PercentageOverdue = (now - lastRev) / (actualNextRev - lastRev);

                    if(reviewingCard.PercentageOverdue > reviewingCardQueue[mostOverdueIndex].PercentageOverdue)
                        mostOverdueIndex = i;
                }

                return mostOverdueIndex;
            }
        }

        private CardMetadata GetNewCardMetadata()
        {
            CardMetadata metaData = newCardQueue.FromBack(0);
            newCardQueue.RemoveAt(newCardQueue.Count - 1);
            return metaData;
        }

        private CardMetadata GetCardMetadata(int index)
        {
            CardMetadata metaData = reviewingCardQueue[index];
            reviewingCardQueue[index] = reviewingCardQueue.FromBack(0);
            reviewingCardQueue.RemoveAt(reviewingCardQueue.Count - 1);
            return metaData;
        }
    }
}
