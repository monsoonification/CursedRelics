using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace CursedRelics.CursedRelicsCode;

/// <summary>
/// Entry point for the CursedRelics mod.
///
/// <para><b>What this mod does (design summary):</b></para>
/// <list type="bullet">
///   <item>When a relic is generated from the grab bag (chest, elite, boss, shop),
///         it has a 10% chance to be marked "cursed".</item>
///   <item>Cursed relics show a red tint, a red-tinted title, and an extra tooltip line
///         describing the downside.</item>
///   <item>The relic's positive numeric effect (DynamicVars) is amplified.</item>
///   <item>A downside from a weighted pool is attached (starts with on-pickup effects only).</item>
/// </list>
///
/// <para><b>What this mod does NOT do (v1 scope):</b></para>
/// <list type="bullet">
///   <item>Ancient relic rewards (Neow etc.). Out of scope for now.</item>
///   <item>Combat-start debuffs (like "-1 card draw"). Needs a combat hook; later feature.</item>
///   <item>Custom effects on hardcoded relics. Only DynamicVars-based relics are amplified at v1.</item>
///   <item>Replacing vanilla relics with subclasses. We modify them in-place via patches.</item>
/// </list>
///
/// <para><b>Multiplayer note:</b></para>
/// Because this mod consumes the shared <c>RunRngSet.UpFront</c> stream (to stay deterministic
/// across host and client), all players in a multiplayer run MUST have the mod installed.
/// A client without it will desync the RNG stream and cause weird behavior.
/// </summary>
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    /// <summary>
    /// Unique ID for this mod. Used for:
    /// <list type="bullet">
    ///   <item>The Harmony instance name (so patches can be identified/removed).</item>
    ///   <item>The logger category.</item>
    ///   <item>The Godot resource path prefix (see <see cref="ResPath"/>).</item>
    /// </list>
    /// Do NOT change this after release. It's baked into save files and Harmony IDs.
    /// </summary>
    public const string ModId = "CursedRelics";

    /// <summary>
    /// Base resource path for this mod's assets. Equivalent to "res://CursedRelics".
    /// Use this when constructing paths to images, scenes, or localization files.
    /// </summary>
    public const string ResPath = $"res://{ModId}";

    /// <summary>
    /// Chance for a relic to roll as cursed, from 0.0 to 1.0.
    /// 0.10 = 10% chance.
    /// </summary>
    public const float CurseChance = 0.10f;

    /// <summary>
    /// Multiplier applied to a cursed relic's DynamicVars.
    /// 2.0 = the relic's numeric effects are doubled.
    /// </summary>
    public const float AmplificationMultiplier = 2.0f;

    /// <summary>
    /// Mod-scoped logger. Call <c>MainFile.Logger.Info("...")</c> anywhere in the mod
    /// and it'll appear in the game log with a <c>[CursedRelics]</c> prefix.
    /// Useful for debugging during development. Remove most calls before release.
    /// </summary>
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    /// <summary>
    /// Called once by the game's mod loader when this mod is loaded.
    ///
    /// This is where we set up Harmony patches that hook into the game.
    /// Every patch class (marked <c>[HarmonyPatch]</c>) in this assembly gets applied here.
    /// </summary>
    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Uncomment this ONLY if you add .tscn scenes with C# scripts attached via Godot.
        // For a Harmony-only mod like this one, it's not needed and can cause
        // "Invalid Task ID" errors in the log during shutdown.
        // Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);

        // Create a Harmony instance scoped to this mod's ID, then scan the assembly
        // for every [HarmonyPatch] class and apply the patches.
        Harmony harmony = new(ModId);
        harmony.PatchAll(assembly);

        Logger.Info("CursedRelics initialized. Patching relic generation hooks.");
    }

    // ---------------------------------------------------------------------
    // Phase 1 stubs: Curse roll and state tracking
    // ---------------------------------------------------------------------

    /// <summary>
    /// Determines whether the given relic should be cursed.
    ///
    /// Called from the relic generation patch (RelicGrabBag.PullFromFront / PullFromBack)
    /// after a relic is pulled from the pool.
    ///
    /// Implementation notes:
    /// - Must consume <c>runState.Rng.UpFront</c> so multiplayer host and client
    ///   roll identically.
    /// - Returns false for relics that cannot be amplified (empty CanonicalVars),
    ///   so we don't curse a relic we can't meaningfully boost.
    /// </summary>
    /// <param name="relic">The relic instance returned by the grab bag.</param>
    /// <param name="runState">Current run state, used for RNG and permission checks.</param>
    /// <returns>True if the relic should be marked cursed.</returns>
    public static bool ShouldCurseRelic(RelicModel relic, IRunState runState)
    {
        // TODO Phase 1: roll runState.Rng.UpFront.NextFloat() < CurseChance
        // TODO Phase 3: also return false if relic.CanonicalVars is empty
        return false;
    }

    /// <summary>
    /// Marks a relic as cursed. Stores the flag on the relic so it survives save/load
    /// and transfers to the mutable clone created later by RelicModel.ToMutable().
    ///
    /// Implementation notes:
    /// - Backed by a SavedSpireField&lt;RelicModel, bool&gt; named "cursedrelics_is_cursed".
    /// - Also needs a matching copy in a ToMutable postfix (see patches) so the flag
    ///   survives the canonical-to-mutable transition.
    /// </summary>
    /// <param name="relic">The relic to mark as cursed.</param>
    public static void MarkCursed(RelicModel relic)
    {
        // TODO Phase 1: CursedRelicState.IsCursed[relic] = true;
    }

    /// <summary>
    /// Returns true if the given relic is currently marked as cursed.
    /// Used by UI patches and effect patches.
    /// </summary>
    /// <param name="relic">The relic to check.</param>
    public static bool IsCursed(RelicModel relic)
    {
        // TODO Phase 1: return CursedRelicState.IsCursed[relic];
        return false;
    }

    // ---------------------------------------------------------------------
    // Phase 3 stubs: Effect amplification
    // ---------------------------------------------------------------------

    /// <summary>
    /// Amplifies a cursed relic's positive numeric effect by scaling its DynamicVars.
    ///
    /// Implementation notes:
    /// - Only works on relics with non-empty CanonicalVars.
    ///   Relics with hardcoded effects (empty CanonicalVars) are skipped at the roll step.
    /// - Must run BEFORE the relic's tooltip is rendered so the player sees the boosted value.
    /// - If CalculateVars() re-runs after this and overwrites our changes, we need a
    ///   CalculateVars patch as well (see Phase 3 verification).
    /// </summary>
    /// <param name="relic">The cursed relic whose effects should be amplified.</param>
    public static void AmplifyCursedEffect(RelicModel relic)
    {
        // TODO Phase 3: foreach var in relic.DynamicVars.Values, multiply base by
        //               AmplificationMultiplier
    }

    // ---------------------------------------------------------------------
    // Phase 4 stubs: Downside pool
    // ---------------------------------------------------------------------

    /// <summary>
    /// Rolls a downside from the weighted pool for a cursed relic and stores its ID
    /// on the relic so the same downside shows in the tooltip and applies on pickup.
    ///
    /// Weighted pool (v1, on-pickup only):
    ///   - lose_gold:      weight 30
    ///   - lose_hp:        weight 25
    ///   - lose_max_hp:    weight 15
    ///   (combat-start downsides are a later feature)
    ///
    /// Implementation notes:
    /// - Uses the same deterministic RNG stream as the curse roll.
    /// - Stores the rolled downside ID via SavedSpireField&lt;RelicModel, string&gt;.
    /// </summary>
    /// <param name="relic">The cursed relic.</param>
    /// <param name="runState">Current run state, used for RNG.</param>
    public static void RollDownside(RelicModel relic, IRunState runState)
    {
        // TODO Phase 4: weighted pick + store ID on relic
    }

    /// <summary>
    /// Applies a cursed relic's downside to the player. Called from a RelicCmd.Obtain
    /// postfix once the relic has been added to the player's inventory.
    ///
    /// Implementation notes:
    /// - Reads the stored downside ID from the relic.
    /// - Dispatches to the appropriate effect (e.g. lose 5 gold, lose 2 HP).
    /// - Must be idempotent: don't let the same relic apply its downside twice on reload.
    ///   Use a separate "applied" flag if needed.
    /// </summary>
    /// <param name="relic">The cursed relic that was just obtained.</param>
    public static void ApplyDownside(RelicModel relic)
    {
        // TODO Phase 4: look up stored downside ID and execute effect
    }

    /// <summary>
    /// Returns the localized text describing the relic's downside, for display in the tooltip.
    /// Returns an empty string if the relic isn't cursed or has no downside stored.
    /// </summary>
    /// <param name="relic">The cursed relic.</param>
    public static string GetDownsideText(RelicModel relic)
    {
        // TODO Phase 4: read stored ID, look up LocString, format with vars if needed
        return string.Empty;
    }
}