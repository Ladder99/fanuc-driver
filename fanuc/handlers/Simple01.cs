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
    public class Simple01: Handler
    {
        private bool dataUpdated = false;
        private int paths = 0;
        private string axes = "";
        private string spindles = "";
        private bool isMachineOn = false;
        private int machineUptime = 0;
        private bool eStopActive = false;
        private bool hasAlarm = false;
        private bool isAuto = false;
        private bool isStopped = false;
        private bool isRunning = false;
        private bool isHold = false;
        private bool axisMoving = false;
        private string programName = "";
        private int piecesProduced = 0;
        private int piecesProducedLife = 0;
        private int piecesRemaining = 0;
        private int cycleTime = 0;
        private int feedOverride = 0;
        private int feedRapidOverride = 0;
        private int spindleOverride = 0;
        private string alarms = "";
        private string messages = "";
        
        public Simple01(Machine machine) : base(machine)
        {
            
        }

        public override async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            // dump paths other than first
            if (veneer.Marker != null && veneer.Marker.GetType().GetProperty("path_no") != null && veneer.Marker.path_no != 1)
                return null;
        
            dataUpdated = true;
            
            switch (veneer.Name)
            {
                case "paths":
                    paths = veneer.LastChangedValue.maxpath_no;
                    break;
                case "axis_names":
                    IEnumerable<dynamic> axis_names = veneer.LastChangedValue.axes;
                    axes = string.Join(",", axis_names.Select(o => o.name + o.suff));
                    break;
                case "spindle_names":
                    IEnumerable<dynamic> spindle_names = veneer.LastChangedValue.spindles;
                    spindles = string.Join(",", spindle_names.Select(o => o.name + o.suff1 + o.suff2));
                    break;
                case "connect":
                    isMachineOn = veneer.LastChangedValue.success;
                    break;
                case "uptime":
                    machineUptime = veneer.LastChangedValue.ldata;
                    break;
                case "stat-info":
                    hasAlarm = veneer.LastChangedValue.status.alarm == 1;
                    eStopActive = veneer.LastChangedValue.status.emergency == 1;
                    isAuto = veneer.LastChangedValue.mode.automatic == 1;
                    isStopped = veneer.LastChangedValue.status.run == 0;
                    isRunning = veneer.LastChangedValue.status.run == 3;
                    isHold = veneer.LastChangedValue.status.run == 1;
                    axisMoving = veneer.LastChangedValue.status.motion == 1;
                    break;
                case "program-name":
                    programName = veneer.LastChangedValue.name;
                    break;
                case "pieces-produced":
                    piecesProduced = veneer.LastChangedValue.ldata;
                    break;
                case "pieces-produced-life":
                    piecesProducedLife = veneer.LastChangedValue.ldata;
                    break;
                case "pieces-remaining":
                    piecesRemaining = veneer.LastChangedValue.ldata;
                    break;
                case "cycle-time":
                    cycleTime = veneer.LastChangedValue.ldata;
                    break;
                case "feedrate-override":
                    feedOverride = 255 - veneer.LastChangedValue.cdata;
                    break;
                case "feedrate-rapid-override":
                    feedRapidOverride = veneer.LastChangedValue.cdata;
                    break;
                case "spindle-override":
                    spindleOverride = veneer.LastChangedValue.cdata;
                    break;
                case "alarms":
                    IEnumerable<dynamic> alms = veneer.LastChangedValue.alarms;
                    alarms = string.Join(",", alms.Select(o => $"{o.alm_no};{o.type};{o.axis};{o.alm_msg}"));
                    break;
                case "message":
                    IEnumerable<dynamic> msgs = veneer.LastChangedValue.msgs;
                    messages = string.Join(",", msgs.Select(o => $"{o.type};{o.data}"));
                    break;
            }
            
            return null;
        }

        public override async Task<dynamic?> OnCollectorSweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
        {
            if (!dataUpdated)
                return null;

            dataUpdated = false;
            
            var topic = $"fanuc/{machine.Id}";
            
            string payload = JObject.FromObject(new
            {
                time = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                paths,
                axes,
                spindles,
                isMachineOn,
                machineUptime,
                eStopActive,
                hasAlarm,
                isAuto,
                isStopped,
                isRunning,
                isHold,
                axisMoving,
                programName,
                piecesProduced,
                piecesProducedLife,
                piecesRemaining,
                cycleTime,
                feedOverride,
                feedRapidOverride,
                spindleOverride,
                alarms,
                messages
            }).ToString(Formatting.None);
            
            await machine.Broker.PublishChangeAsync(topic, payload);

            return null;
        }
    }
}