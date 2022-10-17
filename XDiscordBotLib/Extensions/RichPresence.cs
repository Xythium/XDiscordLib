﻿using System;
using System.Runtime.InteropServices;

namespace XDiscordBotLib.Extensions;

public class DiscordRichPresence
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ReadyCallback();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DisconnectedCallback(int errorCode, string message);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ErrorCallback(int errorCode, string message);

    public struct EventHandlers
    {
        public ReadyCallback readyCallback;
        public DisconnectedCallback disconnectedCallback;
        public ErrorCallback errorCallback;
    }

    // Values explanation and example: https://discordapp.com/developers/docs/rich-presence/how-to#updating-presence-update-presence-payload-fields
    [Serializable]
    public struct RichPresence
    {
        public string state; /* max 128 bytes */
        public string details; /* max 128 bytes */
        public long startTimestamp;
        public long endTimestamp;
        public string largeImageKey; /* max 32 bytes */
        public string largeImageText; /* max 128 bytes */
        public string smallImageKey; /* max 32 bytes */
        public string smallImageText; /* max 128 bytes */
        public string partyId; /* max 128 bytes */
        public int partySize;
        public int partyMax;
        public string matchSecret; /* max 128 bytes */
        public string joinSecret; /* max 128 bytes */
        public string spectateSecret; /* max 128 bytes */
        public bool instance;
    }

    [DllImport("Extensions/discord-rpc-w32.dll", EntryPoint = "Discord_Initialize", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Initialize(string applicationId, ref EventHandlers handlers, bool autoRegister, string optionalSteamId);

    [DllImport("Extensions/discord-rpc-w32.dll", EntryPoint = "Discord_UpdatePresence", CallingConvention = CallingConvention.Cdecl)]
    public static extern void UpdatePresence(ref RichPresence presence);

    [DllImport("Extensions/discord-rpc-w32.dll", EntryPoint = "Discord_RunCallbacks", CallingConvention = CallingConvention.Cdecl)]
    public static extern void RunCallbacks();

    [DllImport("Extensions/discord-rpc-w32.dll", EntryPoint = "Discord_Shutdown", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Shutdown();
}