namespace DoenaSoft.WatchHistory.Implementations
{
    using System;
    using System.IO;
    using AbstractionLayer.IOServices;
    using ToolBox.Generics;

    internal static class SerializerHelper
    {
        internal static T Deserialize<T>(IIOServices ioServices
            , String fileName)
            where T : class, new()
        {
            using (Stream fs = ioServices.GetFileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (Serializer<T>.Deserialize(fs));
            }
        }

        internal static void Serialize<T>(IIOServices ioServices
            , String fileName
            , T instance)
            where T : class, new()
        {
            using (Stream fs = ioServices.GetFileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                Serializer<T>.Serialize(fs, instance);
            }
        }
    }
}