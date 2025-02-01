namespace backend_csharp.Models.Database.DTOs;

/// <summary>
///     This class is a data transfer object for a user that just holds the id (nullable), name and password
/// </summary>
public class SimpleUser
{
    public Guid? Id {get; set;} = null;

    public string Username {get; set;} = "";

    public string Password {get; set;} = "";
}
