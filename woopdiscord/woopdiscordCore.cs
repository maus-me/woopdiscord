using System;
using System.Linq;
using JetBrains.Annotations;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using DiscordRPC;

namespace woopdiscord;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class woopdiscordCore : ModSystem
{
    private const string ApplicationId = "1416935488639598622";
    private const string LargeImageKey = "game_icon";
    private const string LargeImageText = "Vintage Story";
    private const string SmallImageKey = "gear_icon";
    private const int UpdateMsInterval = 5000;

    private ICoreAPI? _api;
    private DiscordRpcClient? _client;

    private DateTime _startTime;

    private static ICoreClientAPI? Capi { get; set; }

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
        Capi = api as ICoreClientAPI;
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        _api = api;
        _startTime = DateTime.UtcNow;
        InitializeDiscordRpc();
        _api.Event.RegisterGameTickListener(UpdatePresence, UpdateMsInterval);
    }

    private void InitializeDiscordRpc()
    {
        try
        {
            _client = new DiscordRpcClient(ApplicationId);
            _client.Initialize();
            _api?.Logger.Debug("[DiscordRPC] Rich Presence Initialized");
        }
        catch (Exception ex)
        {
            _api?.Logger.Error("[DiscordRPC] Failed to initialize: " + ex.Message);
        }
    }

    private void UpdatePresence(float obj)
    {
        if (_client?.IsInitialized != true) return;

        try
        {
            var presence = new RichPresence
            {
                Details = $"{GetServer()}",
                State = "Players ",
                Party = GetPlayerCount(),
                Assets = new Assets
                {
                    LargeImageKey = LargeImageKey,
                    LargeImageText = LargeImageText,
                    SmallImageKey = SmallImageKey,
                    SmallImageText = $"Total Deaths: {GetPlayerDeaths()}"
                },
                Timestamps = new Timestamps(_startTime)
            };
            _client?.SetPresence(presence);
        }
        catch (Exception ex)
        {
            _api?.Logger.Error($"[DiscordRPC] Failed to update presence: {ex.Message}");
        }
    }

    private string GetPlayerMode()
    {
        return _api?.World?.AllOnlinePlayers?.FirstOrDefault()?.WorldData.CurrentGameMode switch
        {
            EnumGameMode.Creative => "Creative",
            EnumGameMode.Survival => "Survival",
            EnumGameMode.Spectator => "Spectator",
            EnumGameMode.Guest => "Guest",
            _ => "Unknown"
        };
    }

    private string GetServer()
    {
        try
        {
            var capi = Capi;
            if (capi == null) return "Unknown";

            // If playing singleplayer (integrated server), no external IP is used
            if (capi.IsSinglePlayer) return "Singleplayer";


            try
            {
                // If we are on a multiplayer server, try to get the external IP

                /* TODO: Expand this out to actually dynamically pull the server info the server list api
                 * https://masterserver.vintagestory.at/api/v1/servers/list
                 */

                var ip = "vs.woopland.com";

                return $"IP: {ip}"; // Replace with actual server address retrieval logic
            }
            catch (Exception e)
            {
                // Fallback: we are on a multiplayer server but couldn't read the address
                return "Multiplayer";
            }
        }
        catch
        {
            return "Unknown";
        }
    }

    private Party? GetPlayerCount()
    {
        try
        {
            var capi = Capi ?? _api as ICoreClientAPI;
            if (capi == null) return null;

            // Don't show a Discord party for singleplayer sessions
            if (capi.IsSinglePlayer) return null;

            var onlinePlayers = _api?.World?.AllOnlinePlayers;
            int size = onlinePlayers?.Length ?? 0;
            if (size <= 0) return null;

            // Build a Party object for Discord Rich Presence
            var serverId = GetServer();
            var id = string.IsNullOrWhiteSpace(serverId) ? "vs-unknown" : $"vs-{serverId}";
            return new Party
            {
                ID = id,
                Size = size,
                // Use 64 as a sensible default max; ensure Max >= Size
                Max = Math.Max(size, 64)
            };
        }
        catch
        {
            return null;
        }
    }

    private int GetPlayerDeaths()
    {
        var player = _api?.World?.AllOnlinePlayers?.FirstOrDefault();
        if (player == null) return 0;

        return player.WorldData.Deaths;
    }

    public override void Dispose()
    {
        Capi = null;
        base.Dispose();
    }
}