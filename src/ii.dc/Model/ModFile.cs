
namespace ii.dc.Model
{
    internal class Metadata
    {
        public string Author { get; set; }
        public string Version { get; set; }
        public int RequiredInstallerVersion { get; set; }
    }

    internal class Mod
    {
        public Action[] Actions { get; set; }
    }

    internal class Action
    {
        public string Operation { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string File { get; set; }
        public Data Data { get; set; }
    }

    internal class Data
    {
        public string Op { get; set; }
        public string Path { get; set; }
        public string Value { get; set; }
    }
}