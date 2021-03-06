﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Devmasters.Core;

namespace HlidacStatu.Lib.Data.External.Zabbix
{


    public class ZabHostAvailability
    {
        private class IgnoreMissingData
        {
            public string hostid { get; set; } = null;
            public DateTime from { get; set; }
            public DateTime to { get; set; }
            public string info { get; set; }
        }

        //static DateTime fixPolicieFrom = new DateTime(2017, 12, 18, 19, 05, 00);
        //static DateTime fixPolicieTo = new DateTime(2017, 12, 19, 11, 10, 00);
        static IgnoreMissingData[] ignoreIt = new IgnoreMissingData[] {
            new IgnoreMissingData(){ from = new DateTime(2018, 3, 18, 0, 17, 00, DateTimeKind.Local), to = new DateTime(2018, 3, 18, 00, 23, 00, DateTimeKind.Local), info="Zabbix restart"},
            new IgnoreMissingData(){ from = new DateTime(2018, 3, 21, 10, 51, 00, DateTimeKind.Local), to = new DateTime(2018, 3, 21, 11, 04, 00, DateTimeKind.Local), info="Zabbix restart"},
            new IgnoreMissingData(){ from = new DateTime(2018, 12, 16, 13, 35, 00, DateTimeKind.Local), to = new DateTime(2018, 12, 16, 15, 14, 00, DateTimeKind.Local), info="Zabbix restart"},
        };


        public ZabHostAvailability(ZabHost host, IEnumerable<ZabHistoryItem> measures, bool fillMissingWithNull = false, DateTime? lastExpectedTime = null)
        {
            if (lastExpectedTime.HasValue == false)
                lastExpectedTime = DateTime.Now.AddMinutes(-1);

            this.Host = host;
            List<ZabAvailability> avail = new List<ZabAvailability>();
            bool stop = false;
            ZabHistoryItem first = measures.First();
            ZabAvailability prev = new ZabAvailability() { Time = first.clock, Response = first.value };
            avail.Add(prev);
            var data = measures.OrderBy(m => m.clock).ToArray();
            for (int i = 1; i < data.Length; i++)
            {
                var curr = data[i];

                if (fillMissingWithNull)
                {
                    var diffInMin = (curr.clock - prev.Time).TotalMinutes;
                    if (diffInMin > 2.5) //velka mezera, dopln null
                    {
                        for (int j = 1; j < diffInMin - 1; j++)
                        {
                            DateTime prevTime = prev.Time.AddMinutes(j);

                            bool skipIt = false;
                            foreach (var ign in ignoreIt)
                            {
                                if ((this.Host.hostid == ign.hostid || string.IsNullOrEmpty(ign.hostid))
                                    && (ign.from > prevTime || prevTime < ign.to)
                                    ) //vypadek na nasi strane
                                {
                                    skipIt = true;
                                    break;
                                }
                            }

                            if (skipIt==false)
                                avail.Add(new ZabAvailability() { Time = prevTime, Response = ZabAvailability.TimeOuted });
                        }
                    }
                }
                avail.Add(new ZabAvailability() { Time = curr.clock, Response = curr.value, DownloadSpeed = null, HttpStatusCode = null });

                prev = avail.Last();

            }
            //check last missing value
            var currLast = data[data.Length - 1];
            if ((lastExpectedTime.Value - currLast.clock).TotalMinutes > 5)
            {
                var diffInMin = (lastExpectedTime.Value - currLast.clock).TotalMinutes;
                if (diffInMin > 2.5) //velka mezera, dopln null
                {
                    for (int j = 1; j < diffInMin - 1; j++)
                    {
                        DateTime prevTime = prev.Time.AddMinutes(j);

                        avail.Add(new ZabAvailability() { Time = prevTime, Response = ZabAvailability.TimeOuted });
                    }
                }

            }


            this.Data = avail.ToArray();
        }
        public ZabHost Host { get; set; }


        public ZabAvailability[] Data { get; set; }

        public long ColSize(DateTime fromDate, DateTime toDate)
        {
            //return (long)(toDate - fromDate).TotalMilliseconds;
            return Data.LongLength;
        }

        ZabAvailabilityStatistics _stat = null;
        public ZabAvailabilityStatistics Statistics()
        {
            if (_stat == null)
                _stat = new ZabAvailabilityStatistics(this.Data);
            return _stat;
        }

        public Statuses WorstStatus(TimeSpan inLastTime)
        {
            if (this.Data == null)
                return Statuses.Unknown;
            if (this.Data.Count() == 0)
                return Statuses.Unknown;

            DateTime fromDate = this.Data.OrderByDescending(o => o.Time).First().Time.AddTicks(-1 * inLastTime.Ticks);

            return this.Data
                .Where(d => d.Time >= fromDate)
                .Where(s => s.Status() < Statuses.Unknown)
                .Max(s => s.Status())
                //?? Statuses.Unknown
                ;
        }



        public IEnumerable<decimal?[]> DataForChart(DateTime fromDate, DateTime toDate, int line)
        {
            var d = this.Data
                .Where(m => m.Time > fromDate)
                .Select((v, index) => new[] { (long)(ToEpochLocalTime(v.Time) ), (int)line, (decimal?)(v.Response) })
                .ToArray();

            return d;
        }
        public static long ToEpochLocalTime(DateTime date)
        {
            return (long)((date - new DateTime(1970, 1, 1)).TotalMilliseconds);
        }
    }
}

