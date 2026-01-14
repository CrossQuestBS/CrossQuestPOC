namespace CrossQuestPOC
{
    public struct ModDefinition
    {
        public ModDefinition(string id, string name, string path)
        {
            Id = id;
            Name = name;
            Path = path;
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

    }
}