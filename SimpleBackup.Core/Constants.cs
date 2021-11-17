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
        public const string BackupName = "SIMPLEBACKUP";
        public const string BackupNameSep = "-";
        public const string BackupNameDateTimeFormat = "yyyyMMddTHHmmssZ";
        public const string BackupNameFolderRegex = @"^SIMPLEBACKUP-\d{8}T\d{6}Z$";
        public const string BackupNameFileRegex = @"^SIMPLEBACKUP-\d{8}T\d{6}Z\.\S+$";
        /// <summary>Possible backup types</summary>
        public enum BackupType
        {
            FOLDER,
            ZIP,
            ZIP_NO_COMPRESS,
            TAR,
            TAR_GZ,
        }
        /// <summary>
        /// Possible error types that could happen
        /// while discovering/copy
        /// </summary>
        public enum ErrorTypes
        {
            UNHANDLED,
            NOT_FOUND,
            NOT_COPYABLE_TYPE,
            NO_PERMISSION,
        }
    }
}
