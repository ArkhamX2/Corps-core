public class Image
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ImageType Type { get; set; }
    public required string ImageData { get; set; }
}
public enum ImageType
{
    Menu,
    Board,
    CardBackground,
    CardIcon,
    UserIcon,
}