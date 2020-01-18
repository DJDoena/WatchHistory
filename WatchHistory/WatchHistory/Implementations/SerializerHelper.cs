namespace DoenaSoft.WatchHistory.Implementations
{
    using AbstractionLayer.IOServices;
    using ToolBox.Generics;

    internal static class SerializerHelper
    {
        internal static T Deserialize<T>(IIOServices ioServices
            , string fileName)
            where T : class, new()
        {
            using (System.IO.Stream fs = ioServices.GetFileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                return (Serializer<T>.Deserialize(fs));
            }
        }

        internal static void Serialize<T>(IIOServices ioServices
            , string fileName
            , T instance)
            where T : class, new()
        {
            using (System.IO.Stream fs = ioServices.GetFileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read))
            {
                Serializer<T>.Serialize(fs, instance);
            }
        }
    }
}