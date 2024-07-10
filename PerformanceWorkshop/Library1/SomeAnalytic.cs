using System.ComponentModel;
using Library1.Entities;

namespace Library1;

public class SomeAnalytic
{
    public record TopCellInfo(string? SiteName, string? Cgi, string RawId, string? Postcode);

    public enum CellsToDisplayEnum
    {
        [Description("All")]
        All = 0,

        [Description("10")]
        Ten = 10,

        [Description("20")]
        Twenty = 20,

        [Description("50")]
        Fifty = 50
    }

    public List<TopCellInfo> ComputeTopCells(
        CellsToDisplayEnum cellCount,
        List<CaseEventDTO> caseEvents,
        List<CaseLocationDTO> caseLocations
    )
    {
        var locations = caseEvents
            .Select(a => new TopCellInfo(
                SiteName: a.StartSite,
                Cgi: a.StartCGI,
                RawId: !string.IsNullOrEmpty(caseLocations.FirstOrDefault(x => x.Cgi == a.StartCGI)?.UniqueLookupCi)
                    ? caseLocations.FirstOrDefault(x => x.Cgi == a.StartCGI)?.UniqueLookupCi
                    : (
                        caseLocations.FirstOrDefault(x => x.Cgi == a.StartCGI)?.RawCi.HasValue == true
                            ? caseLocations.FirstOrDefault(x => x.Cgi == a.StartCGI)?.RawCi.Value.ToString()
                            : ""
                    ),
                Postcode: a.StartPostcode
            ))
            .ToList();

        var end = caseEvents
            .Select(a => new TopCellInfo(
                SiteName: a.EndSite,
                Cgi: a.EndCGI,
                RawId: !string.IsNullOrEmpty(caseLocations.FirstOrDefault(x => x.Cgi == a.EndCGI)?.UniqueLookupCi)
                    ? caseLocations.FirstOrDefault(x => x.Cgi == a.EndCGI)?.UniqueLookupCi
                    : (
                        caseLocations.FirstOrDefault(x => x.Cgi == a.EndCGI)?.RawCi.HasValue == true
                            ? caseLocations.FirstOrDefault(x => x.Cgi == a.EndCGI)?.RawCi.Value.ToString()
                            : ""
                    ),
                Postcode: a.EndPostcode
            ))
            .ToList();

        locations.AddRange(end);

        var cells = locations
            .GroupBy(a => new { a.Cgi, a.SiteName })
            .Select(g => new { Location = g.First(), Count = g.Count() })
            .OrderByDescending(a => a.Count)
            .Where(a => !string.IsNullOrWhiteSpace(a.Location.Cgi))
            .Select(g => g.Location);

        if (cellCount == CellsToDisplayEnum.All)
        {
            return cells.ToList();
        }
        else
        {
            return cells.Take((int)cellCount).ToList();
        }
    }
}
