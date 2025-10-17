using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.ToolBox.Generics;

namespace DoenaSoft.WatchHistory.Implementations;

internal static class SerializerHelper
{
    internal static T Deserialize<T>(IIOServices ioServices, string fileName) where T : class, new()
    {
        using var fs = ioServices.GetFileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
        
        return XmlSerializer<T>.Deserialize(fs);
    }

    internal static void Serialize<T>(IIOServices ioServices, string fileName, T instance) where T : class, new()
    {
        using var fs = ioServices.GetFileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read);
      
        XmlSerializer<T>.Serialize(fs, instance);
    }
}