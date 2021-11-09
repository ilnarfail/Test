using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FileExplorer.Models;

namespace FileExplorer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string fullPath, long fileSize)
        {
            AllModel data = null;
            if (fullPath is null)
            {
                data = new AllModel(){ Directories = GetDrivers() };
            }
            else
            {
                var paths = fullPath.Split("\\").Where(x => !string.IsNullOrEmpty(x)).ToArray();
                data = new AllModel()
                {
                    Directories = GetItems(fullPath),
                    FullPath = new PathDto[paths.Length]
                };
                var iterator = 0;
                foreach (var item in paths)
                {
                    var index = fullPath.IndexOf(item, StringComparison.Ordinal);
                    var path = fullPath[..index];
                    data.FullPath[iterator] = new PathDto()
                    {
                        Name = item,
                        Path = path
                    };
                    iterator++;
                }
            }
            return View(data);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        private DirectoryModel[] GetDrivers() =>
            DriveInfo.GetDrives()
                .Where(x => x.IsReady)
                .Select(x => new DirectoryModel()
                {
                    Name = x.Name,
                    Path = x.RootDirectory.FullName,
                    Size = x.TotalSize
                })
                .ToArray();
        
        private DirectoryModel[] GetItems(string path)
        {
            var files = new DirectoryInfo(@path)
                .GetFiles()
                .Where(x => (x.Attributes & FileAttributes.Hidden) == 0)
                .Select(x => new DirectoryModel() {Name = x.Name, Size = x.Length, IsFile = true});

            var directories = new DirectoryInfo(@path)
                .GetDirectories()
                .Where(x => (x.Attributes & FileAttributes.Hidden) == 0)
                .Select(x => new DirectoryModel()
                {
                    Name = x.Name,
                    Path = x.FullName,
                    IsFile = false,
                    Size = GetDirectorySize(x)
                });

            return files.Concat(directories).ToArray();
        }

        private long GetDirectorySize(DirectoryInfo dr) =>
            dr.EnumerateFiles("*", new EnumerationOptions()
            {
                RecurseSubdirectories = true,
                MatchType = MatchType.Win32,
                AttributesToSkip = 0,
                IgnoreInaccessible = true
            }).Sum(x => x.Length);
    }
}