namespace DoenaSoft.WatchHistory.YoutubeLink
{
    using WatchHistory.Data;

    internal interface IYoutubeManager
    {
        YoutubeVideoInfo GetInfo(string youtubeId);
    }
}