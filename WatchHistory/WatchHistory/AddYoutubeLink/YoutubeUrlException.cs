namespace DoenaSoft.WatchHistory.AddYoutubeLink
{
    using System;

    internal class YoutubeUrlException : ArgumentException
    {
        public YoutubeUrlException(string message) : base(message)
        {
        }
    }
}
