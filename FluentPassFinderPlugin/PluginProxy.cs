﻿using FluentPassFinderContracts;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.Util;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Drawing;
using System.Windows.Threading;

namespace FluentPassFinderPlugin
{
    internal class PluginProxy : IPluginProxy
    {
        private readonly IPluginHost pluginHost;
        private readonly MainForm mainWindow;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly Dispatcher pluginHostDispatcher;
        private readonly Settings settings;

        public PluginProxy(IPluginHost pluginHost)
        {
            this.pluginHost = pluginHost;
            pluginHostDispatcher = Dispatcher.CurrentDispatcher;
            mainWindow = pluginHost.MainWindow;

            jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            jsonSerializerSettings.Formatting = Formatting.Indented;
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            settings = LoadOrCreateDefaultSettings();
        }

        public void CopyToClipboard(string strToCopy, bool bSprCompile, bool bIsEntryInfo, PwEntry peEntryInfo)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                if (ClipboardUtil.Copy(strToCopy, bSprCompile, bIsEntryInfo, peEntryInfo,
                                        mainWindow.DocumentManager.SafeFindContainerOf(peEntryInfo),
                                        IntPtr.Zero))
                {
                    mainWindow.StartClipboardCountdown();
                }
            });
        }

        public string GetPlaceholderValue(string placeholder, PwEntry entry, PwDatabase database, bool resolveAll)
        {
            return pluginHostDispatcher.Invoke(() => SprEngine.Compile(placeholder, new SprContext(entry, database, resolveAll ? SprCompileFlags.All : SprCompileFlags.Deref, true, false)));
        }

        public Image GetBuildInIcon(PwIcon buildInIconId)
        {
            return pluginHostDispatcher.Invoke(() => mainWindow.ClientIcons.Images[(int)buildInIconId]);
        }

        public void PerformAutoType(PwEntry entry, PwDatabase database, string sequence = null)
        {
            pluginHostDispatcher.Invoke(() => AutoType.PerformIntoCurrentWindow(entry, database, sequence));
        }

        public PwDatabase[] Databases => mainWindow?.DocumentManager?.GetOpenDatabases().ToArray();

        public Settings Settings => settings;

        private Settings LoadOrCreateDefaultSettings()
        {
            var configString = pluginHost.CustomConfig.GetString(nameof(FluentPassFinderPlugin));
            Settings loadedSettings = null;
            if (configString == null)
            {
                loadedSettings = CreateDefaultSettings();
            }
            else
            {
                try
                {
                    loadedSettings = JsonConvert.DeserializeObject<Settings>(configString, jsonSerializerSettings);
                }
                catch
                {
                    loadedSettings = CreateDefaultSettings();
                }
            }
            return loadedSettings;
        }

        private Settings CreateDefaultSettings()
        {
            var defaultSettings = new Settings()
            {
                SearchOptions = new SearchOptions()
                {
                    IncludeTitleField = true,
                    IncludeNotesField = true,
                    IncludeUrlField = true,
                    IncludeCustomFields = true,
                    IncludeTags = true,
                    ExcludeExpiredEntries = true,
                    ExcludeGroupsBySearchSetting = true,
                    ResolveFieldReferences = true,
                },
                PluginTotpPlaceholder = "{TOTP}",
                PluginTotpFieldConfig = "totpsettings_stringname",
                GlobalHotkeyCurrentScreen = "Ctrl+Alt+S",
                GlobalHotkeyPrimaryScreen = "Ctrl+Alt+F",
                MainAction = ActionTypeConsts.OpenContextMenu,
                ShiftAction = string.Format(ActionTypeConsts.CopyActionPattern, PwDefs.UserNameField),
                ControlAction = string.Format(ActionTypeConsts.CopyActionPattern, PwDefs.PasswordField),
                AltAction = string.Format(ActionTypeConsts.CopyActionPattern, ActionTypeConsts.Totp),
                ActionSorting = new System.Collections.Generic.Dictionary<string, int>
                {
                    { string.Format(ActionTypeConsts.CopyActionPattern, PwDefs.UserNameField), 1 },
                    { string.Format(ActionTypeConsts.CopyActionPattern, PwDefs.PasswordField), 2 },

                    { string.Format(ActionTypeConsts.AutoTypeActionPattern, PwDefs.UserNameField), 101 },
                    { string.Format(ActionTypeConsts.AutoTypeActionPattern, PwDefs.PasswordField), 102 },
                },
                ShowActionsForCustomFields = true,
                ExcludeActionsForFields = new System.Collections.Generic.List<string>()
                {
                    "_etm_template_uuid"
                },
                Theme = "Dark"
            };
            pluginHost.CustomConfig.SetString(nameof(FluentPassFinderPlugin), JsonConvert.SerializeObject(defaultSettings, jsonSerializerSettings));

            return defaultSettings;
        }

        public void OpenEntryUrl(PwEntry entry)
        {
            pluginHostDispatcher.Invoke(() => WinUtil.OpenEntryUrl(entry));
        }

        public void SelectEntry(PwEntry entry, PwDatabase database)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                // Select db / group
                mainWindow.UpdateUI(false, mainWindow.DocumentManager.FindDocument(database), true, entry.ParentGroup, true, null, false);

                // Select entry
                mainWindow.SelectEntries(new PwObjectList<PwEntry> { entry }, true, true);
                mainWindow.EnsureVisibleEntry(entry.Uuid);

                // Trigger another ui update, otherwise sometimes icons are wrong
                mainWindow.UpdateUI(false, null, false, null, false, null, false);

                // Bring window into foreground
                mainWindow.EnsureVisibleForegroundWindow(true, true);
            });
        }

        public string GetStringFromCustomConfig(string configId, string defaultValue)
        {
            return pluginHostDispatcher.Invoke(() => pluginHost.CustomConfig.GetString(configId, defaultValue));
        }
    }
}
