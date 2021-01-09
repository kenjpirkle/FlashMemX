namespace FlashMemX.Core
{
    public enum ReviewQuality
    {
        Fail,
        SoftFail,
        Again,
        Difficult,
        Good,
        Easy,
        Know
    }

    public static class ReviewQualityExtensions
    {
        public static bool IsWorseThan(this ReviewQuality reviewQuality, ReviewQuality otherReviewQuality)
        {
            return (int)reviewQuality < (int)otherReviewQuality;
        }
    }
}