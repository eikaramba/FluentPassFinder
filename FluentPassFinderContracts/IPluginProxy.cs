﻿using KeePassLib;
using System.Drawing;

namespace FluentPassFinderContracts
{
    public interface IPluginProxy
    {
        void CopyToClipboard(string strToCopy, bool bSprCompile, bool bIsEntryInfo, PwEntry peEntryInfo);
        string GetPlaceholderValue(string placeholder, PwEntry entry, PwDatabase database, bool resolveAll);
        Image GetBuildInIcon(PwIcon nuildInIconId); 
        void PerformAutoType(PwEntry entry, PwDatabase database, string sequence = null);

        PwDatabase[] Databases { get; }
        Settings Settings { get; }
    }
}
