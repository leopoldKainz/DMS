public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedOn { get; set; }
}