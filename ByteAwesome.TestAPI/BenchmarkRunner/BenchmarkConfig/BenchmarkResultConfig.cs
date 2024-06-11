using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace ByteAwesome.TestAPI
{
    public class CustomConfig : ManualConfig
    {
        public CustomConfig()
        {
            // Add essential columns
            AddColumn(TargetMethodColumn.Method);
            AddColumn(StatisticColumn.Error);
            AddColumn(StatisticColumn.StdDev);
            AddColumn(StatisticColumn.Mean);
            AddColumn(BaselineRatioColumn.RatioMean);
            AddColumn(StatisticColumn.Median);
            AddColumn(StatisticColumn.Min);
            AddColumn(StatisticColumn.Max);
            AddColumn(StatisticColumn.P95); // 95th percentile

            // Add memory diagnoser to measure memory allocation
            AddDiagnoser(MemoryDiagnoser.Default);

            // Order benchmarks by method name
            WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Method));

            // Customize summary style
            WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend));
        }
    }
}
