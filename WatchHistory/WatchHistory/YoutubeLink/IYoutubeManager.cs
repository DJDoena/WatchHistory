namespace DoenaSoft.WatchHistory.YoutubeLink
{
    using DoenaSoft.MediaInfoHelper.Youtube;
    using WatchHistory.Data;

    internal interface IYoutubeManager
    {
        YoutubeVideoInfo GetInfo(string youtubeId);
    }
}