﻿using Grillbot.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Statistics
{
    public class Statistics : IConfigChangeable, IDisposable
    {
        public Dictionary<string, StatisticsData> Data { get; }
        public double AvgReactTime { get; private set; }
        public ChannelStats ChannelStats { get; }

        private IConfiguration Config { get; set; }

        public Statistics(IConfiguration configuration)
        {
            Data = new Dictionary<string, StatisticsData>();
            ChannelStats = new ChannelStats(configuration);
            Config = configuration;
        }

        public async Task Init()
        {
            await ChannelStats.Init();
        }

        public void LogCall(string command, long elapsedTime)
        {
            if (command.StartsWith(Config["CommandPrefix"]))
                command = command.Substring(1);

            if (!Data.ContainsKey(command))
                Data.Add(command, new StatisticsData(command, elapsedTime));
            else
                Data[command].Increment(elapsedTime);
        }

        public List<StatisticsData> GetOrderedData(bool byTime)
        {
            if (byTime)
                return Data.Values.OrderByDescending(o => o.AverageTime).ToList();

            return Data.Values.OrderByDescending(o => o.CallsCount).ToList();
        }

        public void ComputeAvgReact(long elapsedTime)
        {
            AvgReactTime = (AvgReactTime + elapsedTime) / 2.0D;
        }

        public TimeSpan GetAvgReactTime() => TimeSpan.FromMilliseconds(AvgReactTime);

        public void ConfigChanged(IConfiguration newConfig)
        {
            ChannelStats.ConfigChanged(newConfig);
            Config = newConfig;
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ChannelStats.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
