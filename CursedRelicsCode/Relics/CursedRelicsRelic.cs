using BaseLib.Abstracts;
using BaseLib.Extensions;
using CursedRelics.CursedRelicsCode.Extensions;

namespace CursedRelics.CursedRelicsCode.Relics;

/// <summary>
/// Base class for every relic in this mod. Extends BaseLib's <see cref="CustomRelicModel"/>,
/// which handles ID registration, save/load, and patch integration automatically.
/// </summary>
public abstract class CursedRelicsRelic : CustomRelicModel
{
    // Small icon — res://CursedRelics/images/relics/{id}.png
    public override string PackedIconPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();

    // Outline — res://CursedRelics/images/relics/{id}_outline.png
    // Same helper as the regular icon; the "_outline" suffix in the filename
    // tells BaseLib's resource loader to look for the outline variant.
    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();

    // Large icon — res://CursedRelics/images/relics/{id}.png
    protected override string BigIconPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}