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
        IEnumerable<CaseEventDTO> caseEvents,
        IEnumerable<CaseLocationDTO> caseLocations
    )
    {
        var locationsByCgi = new Dictionary<string, CaseLocationDTO>();
        foreach (var caseLocation in caseLocations)
        {
            locationsByCgi[caseLocation.Cgi] = caseLocation;
        }

        CaseLocationDTO? LookupCellByCGI(string? cgi)
        {
            return cgi == null ? null : locationsByCgi[cgi];
        }

        var locations = caseEvents
            .Select(a =>
            {
                var cell = LookupCellByCGI(a.StartCGI);
                return new TopCellInfo(
                    SiteName: a.StartSite,
                    Cgi: a.StartCGI,
                    RawId: !string.IsNullOrEmpty(cell?.UniqueLookupCi) ? cell.UniqueLookupCi : cell?.RawCi?.ToString() ?? "",
                    Postcode: a.StartPostcode
                );
            })
            .Concat(
                caseEvents.Select(a =>
                {
                    var cell = LookupCellByCGI(a.EndCGI);
                    return new TopCellInfo(
                        SiteName: a.EndSite,
                        Cgi: a.EndCGI,
                        RawId: !string.IsNullOrEmpty(cell?.UniqueLookupCi) ? cell.UniqueLookupCi : (cell?.RawCi?.ToString() ?? ""),
                        Postcode: a.EndPostcode
                    );
                })
            )
            .ToList();

        var cells = locations
            .Where(l => !string.IsNullOrWhiteSpace(l.Cgi))
            .GroupBy(a => new { a.Cgi, a.SiteName })
            .Select(g => new { Location = g.First(), Count = g.Count() })
            .OrderByDescending(a => a.Count)
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
