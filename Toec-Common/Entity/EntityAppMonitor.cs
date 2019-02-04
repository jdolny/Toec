using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Toec_Common.Entity
{
    [Table("app_monitor")]
    public class EntityAppMonitor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("app_monitor_id")]
        public int Id { get; set; }

        [Column("application_name")]
        public string Name { get; set; }

        [Column("application_path")]
        public string Path { get; set; }

        [Column("start_date_time_utc")]
        public string StartDateTime { get; set; }

        [Column("end_date_time_utc")]
        public string EndDateTime { get; set; }

        [Column("username")]
        public string UserName { get; set; }

        [Column("pid")]
        public int Pid { get; set; }
    }
}