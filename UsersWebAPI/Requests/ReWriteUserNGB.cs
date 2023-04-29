using System.ComponentModel.DataAnnotations;

namespace UsersWebAPI.Requests;

public class ReWriteUserNGB : IAuthData
{
   [Required] public string AuthLogin { get; set; }
   [Required] public string AuthPassword { get; set; }
   
   [Required, RegularExpression(@"^[a-zA-Z0-9]+$")] public string ReLogin { get; set; }

   [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ]+$")] public string? Name { get; set; }

   public DateTime? Birthday { get; set; }
    
   [RegularExpression(@"[012]")] public int? Gender { get; set; }
}