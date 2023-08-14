namespace FluentPassFinder.Contracts
{
    public interface IEntryActionService
    {
        void RunAction(EntrySearchResult searchResult, ActionType actionType);
        void RunAction(EntrySearchResult searchResult, IAction action);
        IEnumerable<IAction> Actions { get; }
    }
}
