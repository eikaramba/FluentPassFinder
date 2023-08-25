﻿using FluentPassFinder.Contracts;
using FluentPassFinder.ViewModels;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    internal class OpenContextMenuAction : ActionBase, IStaticAction
    {
        private readonly Lazy<SearchWindowViewModel> lazySearchWindowViewModel;

        public OpenContextMenuAction(Lazy<SearchWindowViewModel> lazySearchWindowViewModel)
        {
            this.lazySearchWindowViewModel = lazySearchWindowViewModel;
        }

        public override int DefaultSortingIndex => -1;
        public override string ActionType => ActionTypeConsts.OpenContextMenu;
        public override string DisplayName => "Open context menu for selected entry";

        public override void RunAction(EntrySearchResult searchResult)
        {
            lazySearchWindowViewModel.Value.IsContextMenuOpen = true;
        }
    }
}
