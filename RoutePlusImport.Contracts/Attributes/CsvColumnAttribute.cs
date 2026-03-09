namespace RoutePlusImport.Contracts.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CsvColumnAttribute : Attribute
    {
        public string Name { get; }
        public int Order { get; set; } = int.MaxValue;
        public bool Ignore { get; set; }

        public CsvColumnAttribute(string name)
        {
            Name = name;
        }
    }
}
