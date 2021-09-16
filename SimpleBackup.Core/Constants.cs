using System;
using System.IO;

namespace SimpleBackup.Core
{
    public static class Constants
    {
        public const string AppName = "Simple Backup";
        public const string ConfigFilename = "config.xml";
        public static readonly string UserHomePath = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "simple-backup"
        );
        public static readonly string ConfigFullPath = Path.Join(
            UserHomePath,
            ConfigFilename
        );
        public const string CopyrightText = @"Copyright(C) 2021  Leo Spratt

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.";
    }
}
