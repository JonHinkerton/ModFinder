﻿using ModFinder.UI;
using ModFinder.Util;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace ModFinder.Mod
{
  /// <summary>
  /// Manages mod installation.
  /// </summary>
  public static class ModInstaller
  {
    public static async Task<InstallResult> Install(ModViewModel viewModel, bool isUpdate)
    {
      switch (viewModel.ModId.Type)
      {
        case ModType.UMM:
          if (!isUpdate && await ModCache.TryRestoreMod(viewModel.ModId))
          {
            return new(InstallState.Installed);
          }

          if (viewModel.CanInstall)
          {
            return await InstallFromRemoteZip(viewModel, isUpdate);
          }

          break;
        case ModType.Owlcat:
          if (!isUpdate && await ModCache.TryRestoreMod(viewModel.ModId))
          {
            return new(InstallState.Installed);
          }

          if (viewModel.CanInstall)
          {
            return await InstallFromRemoteZip(viewModel, isUpdate);
          }

          break;
        case ModType.Portrait:
          //cache system needs to be adapted to Owl/Portrait but i dont have knowledge of that code, better if Bubbles or Wolfie does it.
          /*
            return new(InstallState.Installed);
          }*/

          if (viewModel.CanInstall)
          {
            return await InstallFromRemoteZip(viewModel, isUpdate);
          }

          break;
      }

      if (viewModel.ModId.Type != ModType.UMM)
      {
        return new($"Currently {viewModel.ModId.Type} mods are not supported.");
      }


      if (!isUpdate && await ModCache.TryRestoreMod(viewModel.ModId))
      {
        return new(InstallState.Installed);
      }

      if (viewModel.CanInstall)
      {
        return await InstallFromRemoteZip(viewModel, isUpdate);
      }

      return new("Unknown mod source");
    }

    private static async Task<InstallResult> InstallFromRemoteZip(ModViewModel viewModel, bool isUpdate)
    {

      var file = Path.GetTempFileName();

      string url;

      if (viewModel.Manifest.Service.IsNexus())
      {
        var expectedZipName = $"{viewModel.Manifest.Id.Id}-{viewModel.Latest.Version}.zip";
        //example: https://github.com/Pathfinder-WOTR-Modding-Community/WrathModsMirror/releases/download/BubbleBuffs%2F5.0.0/BubbleBuffs-5.0.0.zip
        url = $"{viewModel.Manifest.Service.Nexus.DownloadMirror}/releases/download/{viewModel.Manifest.Id.Id}%2F{viewModel.Latest.Version}/{expectedZipName}";
      }
      else
      {
        url = viewModel.Latest.Url;
      }
      Logger.Log.Info($"Fetching zip from {url}");

      await HttpHelper.DownloadFileAsync(url, file);

      return await InstallFromZip(file, viewModel, isUpdate);
    }

    public static string GetModPath(ModType type)
    {
      switch (type)
      {
        case ModType.UMM:
          return Main.UMMInstallPath;
        case ModType.Owlcat:
          return Path.Combine(Main.RTDataDir, "Modifications");
        case ModType.Portrait:
          return Path.Combine(Main.RTDataDir, "Portraits");
        default:
          throw new Exception("Unrecognized Mod Type");
      }
    }

    private static ModType GetModTypeFromZIP(ZipArchive zip)
    {
      if (zip.Entries.Any(e => e.Name.Equals("Info.json", StringComparison.CurrentCultureIgnoreCase)))
        return ModType.UMM;
      if (zip.Entries.Any(e => e.Name.Equals("OwlcatModificationManifest.json", StringComparison.CurrentCultureIgnoreCase)))
        return ModType.Owlcat;
      else return ModType.Portrait;
    }

    public const string modFinderPrefix = "ModFinderPortrait_";

    public static async Task<InstallResult> InstallFromZip(
      string path, ModViewModel viewModel = null, bool isUpdate = false)
    {
      using var zip = ZipFile.OpenRead(path);
      InstallModManifest info;
      ModType modType = viewModel == null ? GetModTypeFromZIP(zip) : viewModel.ModId.Type;

      // If the mod is not in the first level folder in the zip we need to reach in and grab it
      string rootInZip = null;

      switch (modType)
      {
        case ModType.UMM:
          {
            var manifestEntry =
              zip.Entries.FirstOrDefault(e => e.Name.Equals("Info.json", StringComparison.OrdinalIgnoreCase));


            if (manifestEntry is null)
            {
              return new("Unable to find manifest.");
            }

            if (manifestEntry.FullName != manifestEntry.Name)
            {
              int root = manifestEntry.FullName.Length - manifestEntry.Name.Length;
              rootInZip = manifestEntry.FullName[..root];
            }

            var UMMManifest = IOTool.Read<UMMModInfo>(manifestEntry.Open());
            info = new InstallModManifest(UMMManifest.Id, UMMManifest.Version);

            var manifest = viewModel?.Manifest ?? ModManifest.ForLocal(UMMManifest);
            if (!ModDatabase.Instance.TryGet(manifest.Id, out viewModel))
            {
              viewModel = new(manifest);
              ModDatabase.Instance.Add(viewModel);
            }

            break;
          }
        case ModType.Owlcat:
          {
            var manifestEntry =
              zip.Entries.FirstOrDefault(e =>
                e.Name.Equals("OwlcatModificationManifest.json", StringComparison.OrdinalIgnoreCase));

            if (manifestEntry.FullName != manifestEntry.Name)
            {
              int root = manifestEntry.FullName.Length - manifestEntry.Name.Length;
              rootInZip = manifestEntry.FullName[..root];
            }

            if (manifestEntry is null)
            {
              return new("Unable to find manifest.");
            }

            var OwlcatManifest = IOTool.Read<OwlcatModInfo>(manifestEntry.Open());
            info = new InstallModManifest(OwlcatManifest.UniqueName, OwlcatManifest.Version);

            var manifest = viewModel?.Manifest ?? ModManifest.ForLocal(OwlcatManifest);
            if (!ModDatabase.Instance.TryGet(manifest.Id, out viewModel))
            {
              viewModel = new(manifest);
              ModDatabase.Instance.Add(viewModel);
            }

            Main.OwlcatMods.Add(OwlcatManifest.UniqueName);

            break;
          }
        case ModType.Portrait:
          {
            var name = Guid.NewGuid().ToString();
            info = new InstallModManifest(name, null);
            break;
          }
        default:
          {
            throw new Exception("Unable to determine mod type or invalid modtype");
          }
      }


      if (viewModel is not null && viewModel.ModId.Id != info.ModId)
      {
        return new($"ModId mismatch. Found {viewModel.ModId.Id} but expected {info.ModId}");
      }

      // Cache the current version
      if (isUpdate)
      {
        await ModCache.Cache(viewModel);
      }


      var destination = GetModPath(modType);
      //  if (manifestEntry.FullName == manifestEntry.Name) ZIP can have different name from mod ID
      {
        Logger.Log.Verbose($"Creating mod directory. \"{destination}\"");
        // Handle mods without a folder in the zip
        destination = modType == ModType.Portrait ? destination : Path.Combine(destination, info.ModId);
        Logger.Log.Verbose($"Finished creating mod directory. \"{destination}\"");
      }

      static void WriteToDirectory(ZipArchiveEntry entry, string destDirectory, int stripLeading)
      {
        Logger.Log.Verbose($"Starting to WriteToDirectory, destination \"{destDirectory}\"");
        string destFileName = Path.GetFullPath(Path.Combine(destDirectory, entry.FullName[stripLeading..]));
        Logger.Log.Verbose($"destination file name is  \"{destDirectory}\"");
        string fullDestDirPath = Path.GetFullPath(destDirectory + Path.DirectorySeparatorChar);
        Logger.Log.Verbose($"Full destination path is  \"{destDirectory}\"");
        if (!destFileName.StartsWith(fullDestDirPath))
        {
          throw new System.InvalidOperationException("Entry is outside the target dir: " + destFileName);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
        entry.ExtractToFile(destFileName, true);
      }

      static void ExtractInParts(ZipArchive zip, string destination)
      {
        foreach (var part in zip.Entries)
        {
          if (part.Name.ToString() != "")
          {
            //Logger.Log.Verbose(part.FullName.ToString());
            var extPath = Path.Combine(destination, part.FullName);
            //Logger.Log.Verbose(extPath.ToString());
            try
            {
              part.ExtractToFile(extPath, true);
            }
            catch (DirectoryNotFoundException ex)
            {
              var tempPath = extPath.Replace(part.Name, "");
              //Logger.Log.Verbose(tempPath.ToString());
              Directory.CreateDirectory(tempPath);
            }
          }
        }
        return;
      }

      //Non-portrait mods just extract to the destination directory
      if (modType != ModType.Portrait)
      {
        try {
          await Task.Run(() =>
          {
            if (rootInZip != null)
            {
              Directory.CreateDirectory(destination);
              foreach (var entry in zip.Entries.Where(e => e.FullName.Length > rootInZip.Length && e.FullName.StartsWith(rootInZip)))
              {
                string entryDest = Path.Combine(destination, entry.FullName[rootInZip.Length..]);
                if (entry.FullName.EndsWith("/"))
                  Directory.CreateDirectory(entryDest);
                else
                  WriteToDirectory(entry, destination, rootInZip.Length);
              }
            }
            else
            {
              Logger.Log.Verbose(destination);
              zip.ExtractToDirectory(destination, true);
            }
          });
        }
        catch (IOException ex)
        {
          Logger.Log.Verbose(ex.ToString());
          ExtractInParts(zip, destination);
        }
      }
      else
      {
        var enumeratedFolders = Directory.EnumerateDirectories(Path.Combine(Main.RTDataDir, "Portraits"));
        var PortraitFolder = Path.Combine(Main.RTDataDir, "Portraits");
        var tmpFolder = Path.Combine(Environment.GetEnvironmentVariable("TMP"), Guid.NewGuid().ToString());
        zip.ExtractToDirectory(tmpFolder);
        if (Directory.EnumerateDirectories(tmpFolder).Count() <= 1)
        {
          tmpFolder = Path.Combine(tmpFolder, "Portraits");
        }

        //var folderToEnumerate = zip.Entries.Count > 1 ? zip.Entries : zip.Entries.FirstOrDefault(a => a.Name == "Portraits");
        foreach (var portraitFolder in Directory.EnumerateDirectories(tmpFolder))
        {
          var builtString = modFinderPrefix + Guid.NewGuid();
          var earMark = new PortraitEarmark(path.Split('\\').Last()); //Put modid here
          while (Directory.Exists(builtString))
          {
            builtString = modFinderPrefix + Guid.NewGuid();
          }
          var newPortraitFolderPath = Path.Combine(PortraitFolder, builtString);
          Directory.Move(portraitFolder, newPortraitFolderPath);
          ModFinder.Util.IOTool.Write(earMark, Path.Combine(newPortraitFolderPath, "Earmark.json"));
        }
        Directory.Delete(tmpFolder);
      }

      if (viewModel != null)
      {
        viewModel.InstalledVersion = ModVersion.Parse(info.Version);
        viewModel.InstallState = InstallState.Installed;
      }

      Logger.Log.Info($"{viewModel?.Name} successfully installed with version {viewModel?.InstalledVersion}.");
      return new(InstallState.Installed);
    }
  }

  public class InstallResult
  {
    public readonly InstallState State;
    public readonly string Error;

    public InstallResult(InstallState state)
    {
      State = state;
    }

    public InstallResult(string error)
    {
      State = InstallState.None;
      Error = error;
    }
  }
}