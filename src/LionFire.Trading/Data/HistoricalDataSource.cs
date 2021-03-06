﻿#define TRACE_SOURCE_SYMBOLS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public class HistoricalDataSource : IHistoricalDataSource
    {

        #region Identity

        public string SourceName { get; set; }

        #endregion

        #region Accessors

        public IEnumerable<string> SymbolsAvailable
        {
            get
            {
#if TRACE_SOURCE_SYMBOLS
                Console.WriteLine(" - Data source: {SourceName}");
#endif
                foreach (var dir in Directory.GetDirectories(RootDir))
                {
                    foreach (var tfDir in Directory.GetDirectories(dir))
                    {
                        var tf = TimeFrame.TryParse(Path.GetFileName(tfDir));
                        if (tf != null)
                        {
#if TRACE_SOURCE_SYMBOLS
                            Console.WriteLine("   - {Path.GetFileName(dir)}");
#endif
                            yield return Path.GetFileName(dir);
                        }
                    }
                }
            }
        }

        public IEnumerable<string> GetTimeFramesAvailable(string symbol)
        {
            var dir = SymbolDir(symbol);
            if (!Directory.Exists(dir)) return Enumerable.Empty<string>();

            var results = new HashSet<string>();

            foreach (var childDir in Directory.GetDirectories(dir))
            {
                var tf = TimeFrame.TryParse(Path.GetFileName(childDir));
                if (tf == null) continue;
                if (!results.Contains(childDir)) results.Add(childDir);
            }
            return results;
        }

        MarketSeries IDataSource.GetMarketSeries(string key, DateTime? startDate, DateTime? endDate)
        {
            string symbolCode;
            TimeFrame timeFrame;
            MarketSeriesUtilities.DecodeKey(key, out symbolCode, out timeFrame);
            return this.GetMarketSeries(symbolCode, timeFrame, startDate, endDate);
        }

        MarketSeries IDataSource.GetMarketSeries(string symbolCode, TimeFrame timeFrame, DateTime? startDate, DateTime? endDate)
        {
            return this.GetMarketSeries(symbolCode, timeFrame, startDate, endDate);
        }
        public MarketSeries GetMarketSeries(string symbolCode, TimeFrame timeFrame, DateTime? startDate = null, DateTime? endDate = null)
        {
            string file = null;

            if (!Directory.Exists(RootDir))
            {
                throw new ArgumentException($"Historical data source directory missing: {RootDir}");
            }

            file = Path.Combine(RootDir, symbolCode + "_" + timeFrame.Name.ToUpper() + "_UTC+0_00.csv");

            if (!File.Exists(file))
            {
                file = null;

                var dir = SymbolTimeFrameDir(symbolCode, timeFrame);
                if (!Directory.Exists(dir))
                {
                    return null;
                }

                long longestLength = -1;
                string longestPath = null;
                foreach (var path in Directory.GetFiles(dir, "*.csv"))
                {
                    var length = new FileInfo(path).Length;
                    if (length > longestLength)
                    {
                        longestLength = length;
                        longestPath = path;
                    }
                }

                if (longestPath == null) return null;

                file = longestPath;
            }

            return MarketSeries.ImportFromFile(symbolCode, timeFrame, file, startDate, endDate);
        }

        #endregion

        #region Implementation

        public string RootDir { get; set; }

        public string SymbolDir(string symbol)
        {
            return Path.Combine(RootDir, symbol);
        }
        public string SymbolTimeFrameDir(string symbol, string timeFrame)
        {
            return Path.Combine(SymbolDir(symbol), timeFrame);
        }

        #endregion

        //public IMarketSeriesAccessor this[string code] {
        //    get {
        //        if (!this.SymbolsAvailable.Contains(code)) return null;
        //        return null;
        //    }
        //}

        //IMarketSeries IMarketSeriesAccessor.this[TimeFrame timeFrame] {
        //    get {
        //        throw new NotImplementedException();
        //    }
        //}

    }
    //public interface IMarketSeriesAccessor
    //{

    //    IMarketSeries this[TimeFrame timeFrame] { get; }

    //}

}
