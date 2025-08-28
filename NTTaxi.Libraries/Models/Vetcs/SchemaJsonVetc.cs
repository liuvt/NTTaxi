
namespace NTTaxi.Libraries.Models.Vetcs
{
    public class UserVetc
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }

    public class SchemaJsonVetc
    {
        public string Provider { get; set; } = string.Empty;
        public int accountid { get; set; }
        public UserVetc? User { get; set; }
    }

    public class RootObjectVetc
    {
        public SchemaJsonVetc BLU { get; set; } = new();
        public SchemaJsonVetc VLG { get; set; } = new();
        public SchemaJsonVetc STG { get; set; } = new();
    }
}
