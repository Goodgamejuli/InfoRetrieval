namespace backend_csharp.Models.Database.DTOs
{
/// <summary>
/// DTO for an User that just holds the id (nullable), name and password
/// </summary>
    public class SimpleUserDto
    {
        public Guid? Id {get; set;} = null;

        public string Username {get; set;} = "";

        public string Password {get; set;} = "";
    }
}
