namespace HeartMVC.Models
{
    public class DataStatistics
    {
        public int TotalRows { get; set; }
        public int TotalColumns { get; set; }
        public Dictionary<string, ColumnStatistics> ColumnStats { get; set; } = new Dictionary<string, ColumnStatistics>();
        public Dictionary<string, int> MissingValues { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> OutlierCounts { get; set; } = new Dictionary<string, int>();
        public int TotalMissingValues => MissingValues.Values.Sum();
        public int TotalOutliers => OutlierCounts.Values.Sum();
    }

    public class ColumnStatistics
    {
        public double Mean { get; set; }
        public double Median { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string DataType { get; set; } = string.Empty;
        public double StandardDeviation { get; set; }
        public int UniqueValues { get; set; }
    }
}