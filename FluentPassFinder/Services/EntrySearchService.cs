﻿using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services
{
    internal class EntrySearchService : IEntrySearchService
    {
        private const char placeholderStartingChar = '{';
        private readonly IPluginProxy pluginProxy;

        public EntrySearchService(IPluginProxy pluginProxy)
        {
            this.pluginProxy = pluginProxy;
        }

        public IEnumerable<EntrySearchResult> SearchEntries(IEnumerable<PwDatabase> databases, string searchQuery, Settings settings)
        {
            var searchOptions = settings.SearchOptions;
            searchQuery = searchQuery.ToLower();
            foreach (PwDatabase db in databases)
            {
                var allGroups = db.RootGroup.GetGroups(true); 
                allGroups.Insert(0, db.RootGroup);

                var includedGroups = allGroups.ToList();
                if (searchOptions.ExcludeGroupsBySearchSetting)
                {
                    includedGroups = allGroups.Where(g => g.GetSearchingEnabledInherited()).ToList();
                }

                var entries = includedGroups.SelectMany(g => g.GetEntries(false)).ToList();
                foreach (var entry in entries)
                {
                    if (searchOptions.ExcludeExpiredEntries && entry.Expires)
                    {
                        continue;
                    }

                    var fieldNamesToSearch = GetFieldNamesToSearch(entry, searchOptions);
                    var isMatch = false;
                    foreach (var fieldName in fieldNamesToSearch)
                    {
                        var fieldValue = entry.Strings.ReadSafe(fieldName);
                        if (searchOptions.ResolveFieldReferences && fieldValue.Contains(placeholderStartingChar))
                        {
                            fieldValue = pluginProxy.GetPlaceholderValue(fieldValue, entry, db);
                        }
                        if (fieldValue.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase))
                        {
                            isMatch = true;
                            break;
                        }
                    }

                    if (!isMatch && searchOptions.IncludeTags)
                    {
                        foreach (var tag in entry.Tags)
                        {
                            if (tag.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase))
                            {
                                isMatch = true;
                                break;
                            }
                        }
                    }

                    if (isMatch)
                    {
                        yield return new EntrySearchResult { Entry = entry, Database = db };
                    }
                }
            }
        }

        private IEnumerable<string> GetFieldNamesToSearch(PwEntry entry, SearchOptions searchOptions)
        {
            var fields = new List<string>();
            if (searchOptions.IncludeTitleFiled)
            {
                fields.Add(PwDefs.TitleField);
            }
            if (searchOptions.IncludeUserNameField)
            {
                fields.Add(PwDefs.UserNameField);
            }
            if (searchOptions.IncludePasswordField)
            {
                fields.Add(PwDefs.PasswordField);
            }
            if (searchOptions.IncludeNotesField)
            {
                fields.Add(PwDefs.NotesField);
            }
            if (searchOptions.IncludeUrlField)
            {
                fields.Add(PwDefs.UrlField);
            }

            if (searchOptions.IncludeCustomFields)
            {
                var customFields = entry.Strings.Where(fieldN => !PwDefs.IsStandardField(fieldN.Key));
                if (searchOptions.IncludeProtectedCustomFields)
                {
                    fields.AddRange(customFields.Select(field => field.Key));
                }
                else
                {
                    foreach (var customField in customFields)
                    {
                        if (!customField.Value.IsProtected)
                        {
                            fields.Add(customField.Key);
                        }
                    }
                }
            }

            return fields;
        }
    }
}
