using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCOP.Database.Models;

[Table("image_hashes")]
public class ImageHash : IEquatable<ImageHash>
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("hash")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public byte[] Hash { get; set; }


    [Column("gid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long GuildIdDb { get; set; }
    [NotMapped]
    public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

    [Column("mid")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long MessageIdDb { get; set; }
    [NotMapped]
    public ulong MessageId { get => (ulong)this.MessageIdDb; set => this.MessageIdDb = (long)value; }

    public virtual UserMessage Message { get; set; } = null!;



    public bool Equals(ImageHash? other)
        => other is not null && this.Hash.SequenceEqual(other.Hash);

    public override bool Equals(object? obj)
        => this.Equals(obj as ImageHash);

    public override int GetHashCode()
        => this.Id.GetHashCode();
}
