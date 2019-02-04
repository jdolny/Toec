using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Toec_Common.Entity
{
    [Table("policy_history")]
    public class EntityPolicyHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("policy_history_id")]
        public int Id { get; set; }

        [Column("last_run_time_utc")]
        public DateTime LastRunTime { get; set; }

        [Column("policy_guid")]
        public string PolicyGUID { get; set; }

        [Column("policy_hash")]
        public string PolicyHash { get; set; }

        [Column("username")]
        public string Username { get; set; }
    }
}