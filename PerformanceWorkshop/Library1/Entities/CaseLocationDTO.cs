namespace Library1.Entities;

public class CaseLocationDTO
{
    public CaseLocationDTO(string cgi)
    {
        Cgi = cgi;
    }

    public string Cgi { get; init; }

    public string? UniqueLookupCi { get; set; }
    public int? RawCi { get; set; }
}
