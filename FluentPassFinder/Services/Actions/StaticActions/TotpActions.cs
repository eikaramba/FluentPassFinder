﻿using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    internal class CopyTotpAction : ActionBase, IStaticAction
    {
        public override int DefaultSortingIndex => 100;
        public override string ActionType => FluentPassFinderContracts.ActionType.Copy_Totp.ToString();
        public override string DisplayName => "Copy 'TOTP'";

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var pluginTotpPlaceholder = Settings.PluginTotpPlaceholder;
            var totp = pluginProxy.GetPlaceholderValue(pluginTotpPlaceholder, searchResult.Entry, searchResult.Database, true);

            if (string.IsNullOrEmpty(totp) || totp == pluginTotpPlaceholder)
            {
                totp = pluginProxy.GetPlaceholderValue(Consts.NativeTotpPlacholder, searchResult.Entry, searchResult.Database, true);
            }

            pluginProxy.CopyToClipboard(totp, true, true, searchResult.Entry);
        }
    }

    internal class AutoTypeTotpAction : ActionBase, IStaticAction
    {
        public override int DefaultSortingIndex => 200;
        public override string ActionType => FluentPassFinderContracts.ActionType.AutoType_Totp.ToString();
        public override string DisplayName => "Auto type 'TOTP'";

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var pluginTotpPlaceholder = Settings.PluginTotpPlaceholder;
            var totp = pluginProxy.GetPlaceholderValue(pluginTotpPlaceholder, searchResult.Entry, searchResult.Database, true);

            if (string.IsNullOrEmpty(totp) || totp == pluginTotpPlaceholder)
            {
                totp = pluginProxy.GetPlaceholderValue(Consts.NativeTotpPlacholder, searchResult.Entry, searchResult.Database, true);
            }

            pluginProxy.PerformAutoType(searchResult.Entry, searchResult.Database, totp + Consts.AutoTypeEnterPlaceholder);
        }
    }
}
