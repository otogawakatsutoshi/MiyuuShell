using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls;

namespace MiyuuShell;

public partial class FileBrowserPage : ContentPage
{
    private string currentPath;
    private readonly string rootPath;

    public FileBrowserPage()
    {
        InitializeComponent();
        rootPath = GetAppDataDirectory();
        currentPath = rootPath;
        LoadFiles();
    }

    private string GetAppDataDirectory()
    {
#if ANDROID
        return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#elif IOS
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif MACCATALYST
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif WINDOWS
        
        return FileSystem.AppDataDirectory;
#else
        throw new PlatformNotSupportedException();
#endif
    }

    private void LoadFiles()
    {
        var items = new List<FileItem>();
        if (currentPath != rootPath)
        {
            items.Add(new FileItem { Name = "..", Type = "Parent Directory" });
        }

        var directories = Directory.GetDirectories(currentPath)
                                    .Select(d => new FileItem { Name = Path.GetFileName(d), Type = "Folder" });
        var files = Directory.GetFiles(currentPath)
                                .Select(f => new FileItem { Name = Path.GetFileName(f), Type = "File" });

        items.AddRange(directories);
        items.AddRange(files);

        FileListView.ItemsSource = items;
    }

    private void OnCreateFileClicked(object sender, EventArgs e)
    {
        string filePath = Path.Combine(currentPath, $"NewFile_{DateTime.Now.Ticks}.txt");
        File.WriteAllText(filePath, "Sample content");
        LoadFiles();
    }

    private void OnCreateFolderClicked(object sender, EventArgs e)
    {
        string folderPath = Path.Combine(currentPath, $"NewFolder_{DateTime.Now.Ticks}");
        Directory.CreateDirectory(folderPath);
        LoadFiles();
    }

    private void OnFileSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is FileItem item)
        {
            if (item.Type == "Parent Directory")
            {
                currentPath = Directory.GetParent(currentPath)?.FullName ?? rootPath;
            }
            else if (item.Type == "Folder")
            {
                currentPath = Path.Combine(currentPath, item.Name);
            }
            LoadFiles();
        }
    }
}

public class FileItem
{
    public required string Name { get; set; }
    public required string Type { get; set; }
}
