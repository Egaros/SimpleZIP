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
using Windows.ApplicationModel.Resources;

namespace SimpleZIP_UI.I18N
{
    internal static class Resources
    {
        private static readonly ResourceLoader Loader;

        static Resources()
        {
            Loader = new ResourceLoader();
        }

        /// <summary>
        /// Gets the string with the specified name.
        /// </summary>
        /// <param name="name">The name of the string to get.</param>
        /// <returns>Resource string.</returns>
        internal static string GetString(string name)
        {
            return Loader.GetString(name);
        }

        /// <summary>
        /// Gets the string with the specified name and replaces the format items 
        /// with the string representation of the corresponding object in the specified array.
        /// </summary>
        /// <param name="name">The name of the string to get.</param>
        /// <param name="objects">Objects to be replaced with the format items.</param>
        /// <returns>Resource string.</returns>
        internal static string GetString(string name, params object[] objects)
        {
            var value = Loader.GetString(name);
            return string.Format(value, objects);
        }
    }
}
