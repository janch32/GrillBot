﻿using System;
using System.Linq;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        /// <summary>
        /// Returns time for unverify in seconds;
        /// </summary>
        /// <param name="time">
        /// Supported time units: m, h, d
        /// </param>
        private int ParseUnverifyTime(string time)
        {
            var timeWithoutSuffix = time.Substring(0, time.Length - 1);

            if (!timeWithoutSuffix.All(o => char.IsDigit(o)))
                throw new ArgumentException("Neplatný časový formát.");

            var convertedTime = Convert.ToInt32(timeWithoutSuffix);

            if (time.EndsWith("m"))
            {
                // Minutes
                if (convertedTime < 30)
                    throw new ArgumentException("Minimální čas pro unverify v minutách je 30 minut.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromMinutes(convertedTime));
            }
            else if (time.EndsWith("h"))
            {
                // Hours
                if (convertedTime <= 0)
                    throw new ArgumentException("Minimální čas pro unverify v hodinách je 1 hodina.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromHours(convertedTime));
            }
            else if (time.EndsWith("d"))
            {
                // Days
                if (convertedTime <= 0)
                    throw new ArgumentException("Minimální čas pro unverify ve dnech je 1 den.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromDays(convertedTime));
            }
            else
            {
                throw new ArgumentException("Nepodporovaný časový formát.");
            }
        }

        private int ConvertTimeSpanToSeconds(TimeSpan timeSpan)
        {
            var totalSecs = timeSpan.TotalSeconds;

            if (totalSecs * 1000 >= int.MaxValue)
                throw new ArgumentException("Maximální čas pro unverify je 24 dní (576 hodin).");

            return Convert.ToInt32(System.Math.Round(timeSpan.TotalSeconds));
        }
    }
}
