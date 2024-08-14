namespace ByteAwesome
{
    public interface IParquetMigration
    {
        Task ApplyMigrationAsync();
    }

    public abstract class ParquetMigration : IParquetMigration
    {
        protected string _directoryPath = Path.Combine("ParquetFiles");
        public string FilePath { get; set; }
        public string DirectoryPath => _directoryPath;

        public abstract Task ApplyMigrationAsync();
    }
}