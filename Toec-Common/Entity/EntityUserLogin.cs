using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Toec_Common.Entity
{
    [Table("user_logins")]
    public class EntityUserLogin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("user_login_id")]
        public int Id { get; set; }

        [Column("login_date_time_utc")]
        public string LoginDateTime { get; set; }

        [Column("logout_date_time_utc")]
        public string LogoutDateTime { get; set; }

        [Column("username")]
        public string UserName { get; set; }
    }
}