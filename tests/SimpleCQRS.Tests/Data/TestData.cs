namespace SimpleCQRS.Tests.Data;

public record TestData(int id) : Data<int>(id)
{
    public string DummyData1 { get; set; } = default!;
    public int DummyDate2 { get; set; }
    public DateTimeOffset DummyDate3 { get; set; }
}
