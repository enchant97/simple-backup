# Simple Backup
A dotnet app with CLI and GUI mode for creating backups, designed to be easy to use and cross-platform.

## Features
- Built with net6.0
- Includes both GUI (Using Avalonia) and CLI
- User Config is stored at: `$HOME/.config/simple-backup/config.xml`
- Config is shared between all apps
- Contains SimpleBackup.Core for creating your own backup app
- Create zip, zip uncompressed, tar, tar.gz & folder backups

## About The Repo
- This repo uses 'main' as the develop branch and should be treating unstable or unfinished. If you want a stable release please use the tags/releases.
- The [CHANGELOG](CHANGELOG.md) contains a history of changes that happened with each release.
- The version number is shared with all apps to reduce confusion

## License
Can be found in the [LICENSE](LICENSE) file
