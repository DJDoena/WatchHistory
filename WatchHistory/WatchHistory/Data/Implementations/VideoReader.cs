namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.Threading;
    using System.Timers;
    using System.Windows;
    using System.Windows.Media;
    using DoenaSoft.AbstractionLayer.IOServices;
    using DoenaSoft.WatchHistory.Implementations;
    using WatchHistory.Data;

    sealed class VideoReader : IDisposable
    {
        private readonly IIOServices _IOServices;

        private readonly FileEntry _fileEntry;

        private Boolean Elapsed;

        private System.Timers.Timer Timer;

        private MediaPlayer MediaPlayer;

        private String FullName => _fileEntry.FullName;

        public VideoReader(IIOServices ioServices, FileEntry entry)
        {
            _IOServices = ioServices;
            _fileEntry = entry;

            Timer = new System.Timers.Timer(10000)
            {
                AutoReset = false
            };

            Timer.Elapsed += OnTimerElapsed;

            MediaPlayer = new MediaPlayer();
        }

        internal UInt32 GetLength()
        {
            UInt32 seconds = 0;

            if ((FullName.EndsWith(Constants.DvdProfilerFileExtension) == false) && (_IOServices.File.Exists(FullName)))
            {
                seconds = GetLengthFromFile();
            }

            return seconds;
        }

        private UInt32 GetLengthFromFile()
        {
            UInt32 videoLength = GetLengthFromMeta();

            if (videoLength > 0)
            {
                return videoLength;
            }

            Duration duration = GetDuration();

            videoLength = duration.HasTimeSpan ? (uint)(duration.TimeSpan.TotalSeconds) : 0;

            return videoLength;
        }

        private UInt32 GetLengthFromMeta()
        {
            String xmlFile = FullName + ".xml";

            if (_IOServices.File.Exists(xmlFile))
            {
                try
                {
                    VideoInfo info = SerializerHelper.Deserialize<VideoInfo>(_IOServices, xmlFile);

                    return info.Duration;
                }
                catch
                { }
            }

            return 0;
        }

        private Duration GetDuration()
        {
            MediaPlayer.Open(new Uri(FullName, UriKind.Absolute));

            Elapsed = false;

            Timer.Start();

            while ((MediaPlayer.NaturalDuration.HasTimeSpan == false) && (Elapsed == false))
            {
                Thread.Sleep(100);
            }

            Timer.Stop();

            Duration duration = MediaPlayer.NaturalDuration;

            MediaPlayer.Close();

            return duration;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Elapsed = true;
        }

        public void Dispose()
        {
            if (Timer != null)
            {
                Timer.Elapsed -= OnTimerElapsed;

                Timer = null;
            }
        }
    }
}