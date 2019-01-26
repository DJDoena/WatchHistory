namespace DoenaSoft.WatchHistory.YoutubeLink
{
    using System;

    internal class YoutubeUrlException : ArgumentException
    {
        public YoutubeUrlException(string message) : base(message)
        {
        }
    }
}
