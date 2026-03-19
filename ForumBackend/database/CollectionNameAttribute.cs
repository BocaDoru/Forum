
namespace database;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CollectionNameAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}
