using DoenaSoft.MediaInfoHelper.DataObjects;

namespace DoenaSoft.WatchHistory.AddYoutubeLink
{
    internal interface IYoutubeManager
    {
        YoutubeVideo GetInfo(string youtubeId);
    }
}