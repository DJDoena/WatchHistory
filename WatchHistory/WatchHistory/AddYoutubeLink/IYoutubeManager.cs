namespace DoenaSoft.WatchHistory.AddYoutubeLink
{
    using MediaInfoHelper.Youtube;

    internal interface IYoutubeManager
    {
        YoutubeVideoInfo GetInfo(string youtubeId);
    }
}