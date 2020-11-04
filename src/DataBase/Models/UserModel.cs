using System.ComponentModel.DataAnnotations;

namespace ePiggyWeb.DataBase.Models
{
    public class UserModel
    {
        [Key]
        public int Id { get; set; }
        [StringLength(255)]
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }

    }

}