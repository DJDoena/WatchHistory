namespace DoenaSoft.WatchHistory.Implementations
{
    using System;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using ToolBox.Extensions;

    internal class FileNameHelper
    {
        private readonly Char[] InvalidFileNameChars;

        private static FileNameHelper Instance { get; set; }

        private FileNameHelper(IIOServices ioServices)
        {
            InvalidFileNameChars = ioServices.Path.GetInvalidFileNameChars();
        }

        public static FileNameHelper GetInstance(IIOServices ioServices)
        {
            if (Instance == null)
            {
                Instance = new FileNameHelper(ioServices);
            }

            return (Instance);
        }

        internal String ReplaceInvalidFileNameChars(String title)
            => (new String(title.ForEach(ReplaceInvalidFileNameChars).ToArray()));

        private Char ReplaceInvalidFileNameChars(Char c)
            => (InvalidFileNameChars.Contains(c) ? ' ' : c);
    }
}