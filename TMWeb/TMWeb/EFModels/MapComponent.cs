using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels;

public partial class MapComponent
{
    public Guid Id { get; set; }

    public int Type { get; set; }

    public Guid MapId { get; set; }
    [Required]
    public Guid? TargetId { get; set; }
    [Range(0, 100)]
    public double PositionX { get; set; }
    [Range(0, 100)]
    public double PositionY { get; set; }
    [Range(0, 100)]
    public double Height { get; set; }
    [Range(0, 100)]
    public double Width { get; set; }

    public virtual MapConfig Map { get; set; } = null!;
}
