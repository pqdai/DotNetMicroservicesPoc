namespace DashboardService.Api.Queries
{
    using DashboardService.Api.Queries.Dtos;

    public class GetTotalSalesQuery
    {
        public string? ProductCode { get; set; }
        public DateTime SalesDateFrom { get; set; }
        public DateTime SalesDateTo { get; set; }
    }

    public class GetTotalSalesResult
    {
        public SalesTotalDto Total { get; set; } = new();
        public IDictionary<string, SalesTotalDto> PerProductTotal { get; set; } = new Dictionary<string, SalesTotalDto>();
    }

    public class GetAgentsSalesQuery
    {
        public DateTime SalesDateFrom { get; set; }
        public DateTime SalesDateTo { get; set; }
    }

    public class GetAgentsSalesResult
    {
        public IDictionary<string, SalesTotalDto> PerAgentTotal { get; set; } = new Dictionary<string, SalesTotalDto>();
    }

    public class GetSalesTrendsQuery
    {
        public DateTime SalesDateFrom { get; set; }
        public DateTime SalesDateTo { get; set; }
        public TimeUnit Unit { get; set; }
    }

    public class GetSalesTrendsResult
    {
        public IList<PeriodSalesDto> PeriodsSales { get; set; } = new List<PeriodSalesDto>();
    }
}

namespace DashboardService.Api.Queries.Dtos
{
    public class SalesTotalDto
    {
        public decimal PremiumAmount { get; set; }
        public long PoliciesCount { get; set; }
    }

    public class PeriodSalesDto
    {
        public string Period { get; set; } = string.Empty;
        public SalesTotalDto Sales { get; set; } = new();
    }

    public enum TimeUnit
    {
        Day = 0,
        Week = 1,
        Month = 2,
        Year = 3
    }
}
