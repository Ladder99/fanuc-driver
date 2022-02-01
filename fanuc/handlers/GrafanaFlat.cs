using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.LineProtocol;
using l99.driver.@base;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.handlers
{
    public class GrafanaFlat: Handler
    {
        public GrafanaFlat(Machine machine) : base(machine)
        {
            
        }

        private double z_pos_mm = 0;
        private double y_pos_mm = 0;
        private double x_pos_mm = 0;
        
        private int z_feed_mm_s = 0;
        private int y_feed_mm_s = 0;
        private int x_feed_mm_s = 0;

        private int spindle_rpm = 0;
        
        public override async Task<dynamic?> OnDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            if (veneer.Name == "axis_data")
            {
                spindle_rpm = veneer.LastArrivedValue.acts;

                var axis_name = veneer.Marker[1].name;

                switch (axis_name)
                {
                    case "X":
                        x_feed_mm_s = veneer.LastArrivedValue.actf;
                        x_pos_mm = veneer.LastArrivedValue.pos.absolute;
                        break;
                    
                    case "Y":
                        y_feed_mm_s = veneer.LastArrivedValue.actf;
                        y_pos_mm = veneer.LastArrivedValue.pos.absolute;
                        break;

                    case "Z":
                        z_feed_mm_s = veneer.LastArrivedValue.actf;
                        z_pos_mm = veneer.LastArrivedValue.pos.absolute;
                        break;
                }
            }

            return null;
        }

        public override async Task<dynamic?> OnCollectorSweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
        {
            await machine.Broker.PublishAsync("fanuc/grafana/pos", JObject.FromObject(new
            {
                z_pos_mm,
                y_pos_mm,
                x_pos_mm
            }).ToString(Formatting.None));

            await machine.Broker.PublishAsync("fanuc/grafana/fs", JObject.FromObject(new
            {
                z_feed_mm_s,
                y_feed_mm_s,
                x_feed_mm_s,
                spindle_rpm
            }).ToString(Formatting.None));
            
            return null;
        }
    }
}