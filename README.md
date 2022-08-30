# ModFinder

A tool for browsing and managing Pathfinder: Wrath of the Righteous mods and their dependencies.

![Screenshot](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder/blob/main/screenshots/main.png)

### [![Download zip](https://custom-icon-badges.herokuapp.com/badge/-Download-blue?style=for-the-badge&logo=download&logoColor=white "Download zip")](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder/releases/latest/download/ModFinder.zip) Latest Release

## Features

* Browse mods hosted on Nexus and GitHub
* Detects out of date mods
* Automatically installs mods hosted on GitHub
* Detects missing dependencies, enabling one-click install (mods on GitHub) or download link (mods on Nexus)
* Uninstall mods
* And more

## For Users

Download the [latest release](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder/releases/latest/download/ModFinder.zip), extract the folder, and run `ModFinder.exe`!

Tips:

* Searching checks mod name and author by default
* You can search specifically name, author, or [tag](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder/blob/main/ModFinderClient/Mod/Tag.cs):
    * `a:bub` to include authors with "bub" in their name, or `-n:bub` to exclude them
    * `n:super` to include mods with "super" in the name, or `-n:super` to exclude them
    * `t:game` to include tags with "game" in their name, or `-n:game` to exclude them
* Version updates are checked every 30 minutes, but you need to restart the app to get the latest version data
    * If the latest version is for a more recent game version (e.g beta) it will install it even if it doesn't work with your local version

See [Troubleshooting](#troubleshooting).
    
### Missing a mod?

Ask the mod developer to add it or file an [issue](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder/issues/new).

## For Mod Devs

Currently ModFinder only supports UMM mods hosted on Nexus or GitHub.

To add (or change details about) your mod:

1. Update [internal_manifest.json](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder/blob/main/ManifestUpdater/Resources/internal_manifest.json)
    * Don't include any version data or description, this is automatically updated roughly every 2 hours
    * You can submit a PR or file an issue
    
That's it! The manifest format is documented [in the code](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder/blob/main/ModFinderClient/Mod/ModManifest.cs).

Make sure to apply the appropriate [tags](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder/blob/main/ModFinderClient/Mod/Tag.cs) so users can find your mod.
 
Assumptions for GitHub:

* The first release asset is a zip file containing the mod (i.e. what a user would drag into UMM)
    * You can specify a `ReleaseFilter`, look at [MewsiferConsole](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder/blob/main/ManifestUpdater/Resources/internal_manifest.json) for an example
* Your releases are tagged with a version string in the format `1.2.3e`. Prefixes are ignored.
    * If there's a mismatch between your GitHub tag version and `Info.json` version it will think the mod is always out of date

If necessary you can host your own `ModManifest` JSON file by adding a direct download link to `ExternalManifestUrls` in [master_manifest.json](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder/blob/main/ManifestUpdater/Resources/master_manifest.json). Keep in mind, this will not be automatically updated so it is up to you to populate description and version info.

### Want your mod removed from the list?

File an [issue](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder/issues/new) or submit a PR.

## Troubleshooting

* Make sure you have [.NET Runtime 5.0](https://dotnet.microsoft.com/en-us/download/dotnet/5.0) installed
    * In the future it will likely migrate to .NET Runtime 6.0 since 5.0 is EOL
* If you have a non-standard installation directory it may fail to find your Wrath installation. Currently there is no fix, expect improvements in the future (it's pretty effective at finding it right now, but some cases do fail).
* Try [running it as an Administrator](https://www.itechtics.com/run-programs-administrator/)
* Make sure your anti-virus is not blocking it
    * We is very sus

### Problems with a mod?

If there's an issue installing, downloading, displaying information about, or anything else regarding a specific mod file an [issue](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder/issues/new). That includes mods that no longer work and are abandoned.

### Other issues?

You guessed it: file an [issue](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder/issues/new).

## Acknowledgements

* Barley for starting this project in the first place ([ModFinder_WOTR](https://github.com/BarleyFlour/ModFinder_WOTR)) and working with Bubbles to complete like 80% before I decided to finish it
* Bubbles for his excellent work on the UI styling and Barley for handling the GitHub action setup
* The modding community on [Discord](https://discord.com/invite/wotr), an invaluable and supportive resource for help modding.
* All the Owlcat modders who came before me, wrote documents, and open sourced their code.

## Interested in modding?

* Check out the [OwlcatModdingWiki](https://github.com/WittleWolfie/OwlcatModdingWiki/wiki).
* Join us on [Discord](https://discord.com/invite/wotr).
