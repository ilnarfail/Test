namespace FileExplorer.Models
{

    public class AllModel
    {
        public PathDto[] FullPath { get; set; }
        
        public DirectoryModel[] Directories { get; set; }
    }

    public class PathDto
    {
        public string Name { get; set; }

        public string Path { get; set; }
    }
    
    public class DirectoryModel : PathDto
    {
        public long Size { get; set; }
        
        public bool IsFile { get; set; }
    }
}