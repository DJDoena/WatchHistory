using DoenaSoft.ToolBox.Generics;
using DoenaSoft.WatchHistory.Data;

namespace DoenaSoft.WatchHistory;

public static class MetaReader
{
    public static Files ReadFiles(string fileName)
        => XmlSerializer<Files>.Deserialize(fileName);
}