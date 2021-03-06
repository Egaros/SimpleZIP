﻿// ==++==
// 
// Copyright (C) 2017 Matthias Fussenegger
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// ==--==
using System.Collections.Generic;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression.Reader;

namespace SimpleZIP_UI.Application.Compression.Model
{
    public class ExtractableItem
    {
        /// <summary>
        /// A friendly name of this item.
        /// </summary>
        internal string DisplayName { get; }

        /// <summary>
        /// The archive which can be extracted.
        /// </summary>
        internal StorageFile Archive { get; }

        /// <summary>
        /// Optional list of entries to be extracted. If this is not 
        /// <code>null</code>, then only these entries will be extracted.
        /// </summary>
        internal IReadOnlyList<FileEntry> Entries { get; }

        internal ExtractableItem(string displayName, StorageFile archive,
            IReadOnlyList<FileEntry> entries = null)
        {
            DisplayName = displayName;
            Archive = archive;
            Entries = entries;
        }
    }
}
