using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Toec_Common.Entity
{
    [Table("settings")]
    public class EntitySetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("setting_id")]
        public int Id { get; set; }

        [Column("setting_name")]
        public string Name { get; set; }

        [Column("setting_value")]
        public string Value { get; set; }
    }
}