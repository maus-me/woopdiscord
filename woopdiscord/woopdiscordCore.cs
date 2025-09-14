using woopdiscord.Config;
using HarmonyLib;
using JetBrains.Annotations;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.Common;

namespace woopdiscord;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class woopdiscordCore : ModSystem
{
    public static ILogger Logger { get; private set; }
    public static string ModId { get; private set; }
    public static ICoreClientAPI Capi { get; private set; }
    public static Harmony HarmonyInstance { get; private set; }
    public static ModConfig Config => ConfigLoader.Config;

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
        Capi = api as ICoreClientAPI;
        Logger = Mod.Logger;
        ModId = Mod.Info.ModID;
        HarmonyInstance = new Harmony(ModId);
        HarmonyInstance.PatchAll();
    }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        Logger.Notification("Hello from template mod: " + api.Side);
        Logger.StoryEvent("Templates lost..."); // Sample story event (shown when loading a world)
        Logger.Event("Templates loaded..."); // Sample event (shown when loading in dev mode or in logs)
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        Logger.Notification("Hello from template mod client side: " + Lang.Get("woopdiscord:hello"));
    }


    public override void Dispose()
    {
        HarmonyInstance?.UnpatchAll(ModId);
        HarmonyInstance = null;
        Logger = null;
        ModId = null;
        Capi = null;
        base.Dispose();
    }
}