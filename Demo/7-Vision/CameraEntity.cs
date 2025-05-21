using System.Text.Json.Serialization;

namespace Vision;

public class CameraEntity
{
    public string CameraName { get; set; }
    public Status Status { get; set; }
    public int EggCount { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    Normal = 1,
    EggCracked = 2,
    PackageDamaged = 3,
    CameraProblem = 4
}