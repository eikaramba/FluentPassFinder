﻿using System.Windows.Input;

namespace FluentPassFinder.Contracts
{
    public interface IAction : ICommand
    {
        void RunAction(EntrySearchResult searchResult);

        ActionType ActionType { get; }
    }
}