namespace Kolia.Thumbnail.API.Data.Entities
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }

        DateTimeOffset? DeletionTime { get; set; }
    }
}