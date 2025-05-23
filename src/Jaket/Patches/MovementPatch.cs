namespace Jaket.ObsoletePatches;

using HarmonyLib;
using ULTRAKILL.Cheats;
using UnityEngine;

using Jaket.Content;
using Jaket.Input;
using Jaket.Net;
using Jaket.Net.Types;
using Jaket.UI;
using Jaket.UI.Dialogs;
using Jaket.UI.Fragments;

[HarmonyPatch(typeof(NewMovement))]
public class MovementPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("Start")]
    static void Spawn(NewMovement __instance)
    {
        // add some randomness to the spawn position so players don't stack on top of each other at the start of the level
        if (LobbyController.Online) __instance.transform.position += new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(NewMovement.GetHurt))]
    static void Death(NewMovement __instance, int damage, bool invincible)
    {
        // sometimes fake death messages are sent to the chat
        if (invincible && __instance.gameObject.layer == 15) return;
        if (Invincibility.Enabled) return;

        if (__instance.hp > 0 && __instance.hp - damage <= 0)
        {
            // player death message
            LobbyController.Lobby?.SendChatString("#/d");

            Chat.Instance.Field.gameObject.SetActive(false);
            Emotes.Instance.Play(0xFF);
        }
    }
}

[HarmonyPatch]
public class CommonPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CheatsManager), nameof(CheatsManager.HandleCheatBind))]
    static bool Cheats() => !UI.AnyDialog && !NewMovement.Instance.dead && Emotes.Current == 0xFF;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CheatsController), nameof(CheatsController.Update))]
    static bool CheatsMenu() => Cheats();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Noclip), "UpdateTick")]
    static bool CheatsNoclip() => Cheats();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Grenade), "Update")]
    static bool RocketRide() => Cheats();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CameraFrustumTargeter), "CurrentTarget", MethodType.Setter)]
    static void AutoAim(ref Collider value)
    {
        if (value != null && value.TryGetComponent<RemotePlayer>(out var player) && player.Team.Ally()) value = null;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WeaponWheel), "OnEnable")]
    static void Wheel(WeaponWheel __instance)
    {
        if (EmoteWheel.Shown) __instance.gameObject.SetActive(false);
    }
}
