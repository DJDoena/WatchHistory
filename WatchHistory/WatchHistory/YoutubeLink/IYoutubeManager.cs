namespace DoenaSoft.WatchHistory.YoutubeLink
{
    using MediaInfoHelper.Youtube;

    internal interface IYoutubeManager
    {
        YoutubeVideoInfo GetInfo(string youtubeId);
    }
}