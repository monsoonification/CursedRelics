using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

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
///   <item>Ancient relic rewards (Neow etc.) — out of scope for now.</item>
///   <item>Combat-start debuffs (like "-1 card draw") — needs a combat hook; later feature.</item>
///   <item>Custom effects on hardcoded relics — only DynamicVars-based relics are amplified at v1.</item>
///   <item>Replacing vanilla relics with subclasses — we modify them in-place via patches.</item>
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
    /// Do NOT change this after release — it's baked into save files and Harmony IDs.
    /// </summary>
    public const string ModId = "CursedRelics";

    /// <summary>
    /// Base resource path for this mod's assets. Equivalent to "res://CursedRelics".
    /// Use this when constructing paths to images, scenes, or localization files.
    /// </summary>
    public const string ResPath = $"res://{ModId}";

    /// <summary>
    /// Mod-scoped logger. Call <c>MainFile.Logger.Info("...")</c> anywhere in the mod
    /// and it'll appear in the game log with a <c>[CursedRelics]</c> prefix.
    /// Useful for debugging during development — remove most calls before release.
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

        Logger.Info("CursedRelics initialized — patching relic generation hooks.");
    }
}