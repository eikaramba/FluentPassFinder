﻿using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyPasswordAction : ActionBase
    {
        private readonly IPluginHostProxy hostProxy;
        private readonly ISearchWindowInteractionService searchWindowInteractionService;

        public override ActionType ActionType => ActionType.CopyPassword;

        public CopyPasswordAction(IPluginHostProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService)
        {
            this.hostProxy = hostProxy;
            this.searchWindowInteractionService = searchWindowInteractionService;
        }

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            hostProxy.CopyToClipboard(searchResult.Entry.Strings.ReadSafe(PwDefs.PasswordField), true, true, searchResult.Entry);
        }
    }
}
