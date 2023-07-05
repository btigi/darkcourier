using ii.dc.Model;
using ii.SimpleZip;
using Marvin.JsonPatch.Dynamic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Dynamic;
using System.IO.Compression;
using System.Reflection;

//args = new string[3];
//args[0] = "install";
//args[1] = @"D:\data\bg";
//args[2] = @"D:\data\mymod\tweaks.json";

if (args.Length < 3 || args.Length > 4)
{
    Console.WriteLine("Expected usage");
    Console.WriteLine("  dc create path_to_bgd.data input_directory");
    Console.WriteLine("  dc extract path_to_bgd.data output_directory archived_path/archived_file.ext");
    Console.WriteLine("  dc extractall path_to_bgd.data output_directory");
    Console.WriteLine("  dc install path_to_bgd.data path_to_mod_file.json");
    return;
}

if (args[0].ToLower() == "create")
{
    var inputDirectory = args[2];
    var outputFile = args[1];
    var sz = new SimpleZipFile();

    sz.Create(inputDirectory, outputFile);
}

if (args[0].ToLower() == "extractall")
{
    var bdgPath = args[1];
    var outputDirectory = args[2];
    ZipFile.ExtractToDirectory(bdgPath, outputDirectory);
}

if (args[0].ToLower() == "extract")
{
    var bdgPath = args[1];
    var outputDirectory = args[2];
    var targetFile = args[3];

    outputDirectory = EnsurePathSeparator(outputDirectory);

    using var zf = ZipFile.Open(bdgPath, ZipArchiveMode.Read);
    foreach (var entry in zf.Entries)
    {
        if (entry.FullName.ToLower() == targetFile)
        {
            var outputFile = outputDirectory + targetFile;
            var directory = Path.GetDirectoryName(outputFile);
            Directory.CreateDirectory(directory);
            entry.ExtractToFile(outputFile);
            break;
        }
    }
}

if (args[0].ToLower() == "install")
{
    var modFile = args[2];
    var modFolder = Path.GetDirectoryName(modFile);
    modFolder = EnsurePathSeparator(modFolder);
    var backupFolder = Path.GetDirectoryName(modFile);
    backupFolder = $@"{EnsurePathSeparator(backupFolder)}backup\";
    var unpackedFolder = args[1];
    unpackedFolder = EnsurePathSeparator(unpackedFolder);

    Directory.CreateDirectory(backupFolder);

    var metadata = new Metadata();
    var mod = new Mod();
    var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(modFile, optional: false, reloadOnChange: false);
    var configuration = builder.Build();
    ConfigurationBinder.Bind(configuration.GetSection("Metadata"), metadata);
    ConfigurationBinder.Bind(configuration.GetSection("Mod"), mod);

    var thisInstallerVersion = Assembly.GetExecutingAssembly().GetName().Version.Major;
    if (metadata.RequiredInstallerVersion > thisInstallerVersion)
    {
        Console.WriteLine($"This is installer version {thisInstallerVersion}. Mod requires installer version {metadata.RequiredInstallerVersion}. Aborting.");
        return;
    }

    foreach (var action in mod.Actions)
    {
        if (action.Operation == "copy")
        {
            File.Copy($"{modFolder}{action.Source}", $"{unpackedFolder}{action.Destination}", true);
            LogFileAdded(backupFolder, $"{unpackedFolder}{action.Destination}");
        }

        if (action.Operation == "patch")
        {
            var fileTextIn = File.ReadAllText(@$"{unpackedFolder}{action.File}");
            var fileObject = JsonConvert.DeserializeObject<ExpandoObject>(fileTextIn);
            var patchDocument = new JsonPatchDocument();

            if (action.Data.Op == "add")
            {
                patchDocument.Add(action.Data.Path, action.Data.Value);
            }
            if (action.Data.Op == "replace")
            {
                patchDocument.Add(action.Data.Path, action.Data.Value);
            }
            if (action.Data.Op == "remove")
            {
                patchDocument.Add(action.Data.Path, action.Data.Value);
            }
            if (action.Data.Op == "copy")
            {
                patchDocument.Add(action.Data.Path, action.Data.Value);
            }
            if (action.Data.Op == "move")
            {
                patchDocument.Add(action.Data.Path, action.Data.Value);
            }
            patchDocument.ApplyTo(fileObject);
            var fileTextOut = JsonConvert.SerializeObject(fileObject, Formatting.Indented);
            File.WriteAllText($@"{unpackedFolder}{action.File}", fileTextOut);

            // Create a backup of the changed file (only once per mod)
            if (fileTextIn != fileTextOut)
            {
                var backupFileLocation = $@"{backupFolder}{action.File}";
                if (!File.Exists(backupFileLocation))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(backupFileLocation));
                    File.WriteAllText(backupFileLocation, fileTextIn);
                    LogFileChanged(backupFolder, backupFileLocation);
                }
            }
        }

        // LogModInstallation("?????", "MOD INSTALLED"); //TODO:
    }
}

if (args[0].ToLower() == "uninstall")
{
    var modFile = args[2];
    var modFolder = Path.GetDirectoryName(modFile);
    modFolder = EnsurePathSeparator(modFolder);
    var backupFolder = Path.GetDirectoryName(modFile);
    backupFolder = $@"{EnsurePathSeparator(backupFolder)}backup\";
    var unpackedFolder = args[1];
    unpackedFolder = EnsurePathSeparator(unpackedFolder);

    var metadata = new Metadata();
    var mod = new Mod();
    var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(modFile, optional: false, reloadOnChange: false);
    var configuration = builder.Build();
    ConfigurationBinder.Bind(configuration.GetSection("Metadata"), metadata);
    ConfigurationBinder.Bind(configuration.GetSection("Mod"), mod);

    if (File.Exists($"{backupFolder}filesadded.log"))
    {
        var lines = File.ReadAllLines($"{backupFolder}filesadded.log");
        foreach (var line in lines.Where(w => File.Exists(w)))
        {
            File.Delete(line);
        }
    }

    if (File.Exists($"{backupFolder}fileschanged.log"))
    {
        var lines = File.ReadAllLines($"{backupFolder}fileschanged.log");
        foreach (var line in lines)
        {
            var source = line;
            var thisLine = line.Replace(backupFolder, "");
            var destination = $"{unpackedFolder}{thisLine}";
            File.Copy(source, destination, true);
        }
    }

    File.Delete($"{backupFolder}filesadded.log");
    File.Delete($"{backupFolder}fileschanged.log");
    Directory.Delete(backupFolder, true);
}


Console.WriteLine("Process complete");
Console.WriteLine("- press any key -");
Console.ReadKey();

string EnsurePathSeparator(string s)
{
    if (!s.EndsWith(Path.DirectorySeparatorChar.ToString()))
    {
        s += Path.DirectorySeparatorChar;
    }
    return s;
}

void LogFileAdded(string backupfolder, string file)
{
    file += System.Environment.NewLine;
    File.AppendAllText($"{backupfolder}filesadded.log", file);
}

void LogFileChanged(string backupfolder, string file)
{
    file += System.Environment.NewLine;
    File.AppendAllText($"{backupfolder}fileschanged.log", file);
}

void LogModInstallation(string folder, string file)
{
    file += System.Environment.NewLine;
    File.AppendAllText($"{folder}mods.log", file);
}