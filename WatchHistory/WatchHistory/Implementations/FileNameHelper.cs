namespace DoenaSoft.WatchHistory.Implementations
{
    using System;
    using System.Linq;
    using AbstractionLayer.IOServices;

    internal class FileNameHelper
    {
        private readonly Char[] InvalidFileNameChas;

        private static FileNameHelper Instance { get; set; }

        private FileNameHelper(IIOServices ioServices)
        {
            InvalidFileNameChas = ioServices.Path.GetInvalidFileNameChars();
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
            => (new String(title.Select(ReplaceInvalidFileNameChars).ToArray()));

        private Char ReplaceInvalidFileNameChars(Char c)
            => (InvalidFileNameChas.Contains(c) ? ' ' : c);
    }
}