namespace NTTaxi.Libraries.Models.Alis;

public class SchemaJson
{
    public UserAli? User { get; set; }
    public List<CookieAli>? CookieAli { get; set; }

}

public class UserAli
{
    public string username { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}

public class CookieAli
{
    public string key { get; set; } = string.Empty;
    public string value { get; set; } = string.Empty;
}