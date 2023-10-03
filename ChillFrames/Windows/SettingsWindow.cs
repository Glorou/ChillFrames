﻿using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using ChillFrames.System;
using ChillFrames.Windows.Tabs;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using KamiLib.Command;
using KamiLib.Interfaces;
using KamiLib.System;
using KamiLib.Utility;

namespace ChillFrames.Windows;

public class SettingsWindow : Window
{
    private readonly IEnumerable<ITabItem> tabs;

    public SettingsWindow() : base("ChillFrames Settings")
    {
        tabs = new ITabItem[]
        {
            new LimiterSettingsTab(),
            new GeneralSettingsTab(),
            new ZoneFilterTab(),
        };
        
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 475),
            MaximumSize = new Vector2(9999,9999)
        };

        Flags |= ImGuiWindowFlags.NoScrollbar;
        Flags |= ImGuiWindowFlags.NoScrollWithMouse;
        
        CommandController.RegisterCommands(this);
    }
    
    public override void Draw()
    {
        if (ImGui.BeginChild("##MainToggleAndStatus", ImGuiHelpers.ScaledVector2(0.0f, 60.0f)))
        {
            var config = ChillFramesSystem.Config;
            
            var value = config.PluginEnable;
            if (ImGui.Checkbox("Enable Framerate Limiter", ref value))
            {
                config.PluginEnable = value;
                config.Save();
            }

            if(ChillFramesSystem.BlockList.Count > 0)
            {
                if (ImGuiComponents.IconButton("##ReleaseLocks", FontAwesomeIcon.Unlock))
                {
                    ChillFramesSystem.BlockList.Clear();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Remove limiter lock");
                }
                
                ImGui.SameLine();
                ImGuiHelpers.SafeTextColoredWrapped(KnownColor.Red.Vector(), $"Limiter is inactive - requested by plugin(s): {string.Join(", ", ChillFramesSystem.BlockList)}");
            }
            else if (FrameLimiterCondition.IsBlacklisted)
            {
                ImGui.TextColored(KnownColor.Red.Vector(), "Limiter Inactive, In Blacklisted Zone");
            }
            else if (!FrameLimiterCondition.DisableFramerateLimit() && config.Limiter.EnableIdleFramerateLimit && config.PluginEnable)
            {
                ImGui.TextColored(KnownColor.Green.Vector(), $"Limiter Active. Target Framerate: {config.Limiter.IdleFramerateTarget}");
            }
            else if (FrameLimiterCondition.DisableFramerateLimit() && config.Limiter.EnableActiveFramerateLimit && config.PluginEnable)
            {
                ImGui.TextColored(KnownColor.Green.Vector(), $"Limiter Active. Target Framerate: {config.Limiter.ActiveFramerateTarget}");
            }
            else
            {
                ImGui.TextColored(KnownColor.Red.Vector(), "Limiter Inactive");
            }
        }
        ImGui.EndChild();

        var region = ImGui.GetContentRegionAvail();
        
        if (ImGui.BeginTabBar("TabBar"))
        {
            foreach (var tab in tabs)
            {
                if (ImGui.BeginTabItem(tab.TabName))
                {
                    if (ImGui.BeginChild("TabChild", new Vector2(0.0f, region.Y - 50.0f), false, ImGuiWindowFlags.AlwaysVerticalScrollbar))
                    {
                        tab.Draw();
                    }
                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }

        PluginVersion.Instance.DrawVersionText();
    }
    
    [BaseCommandHandler("OpenConfigWindow")]
    public void OpenConfigWindow()
    {
        if (!Service.ClientState.IsLoggedIn) return;
        if (Service.ClientState.IsPvP) return;
            
        Toggle();
    }
}