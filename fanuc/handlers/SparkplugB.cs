using System.Net.NetworkInformation;
using System.Threading.Tasks;
using l99.driver.@base;
using l99.driver.@base.mqtt.sparkplugb;
using Newtonsoft.Json.Linq;
using NLog;

namespace l99.driver.fanuc.handlers
{
    public class SparkplugB: Handler
    {
        protected ILogger _logger;
        
        private bool SPB_STRICT = true;
        //private string SPB_BROKER_IP = "10.20.30.102";
        //private int SPB_BROKER_PORT = 1883;
        
        private Protocol _protocol;
        private bool _last_connection_success = false;
        
        public SparkplugB(Machine machine) : base(machine)
        {
            _logger = LogManager.GetLogger(this.GetType().FullName);
        }
        
        public override async Task InitializeAsync()
        {
            //_protocol = new Protocol(SPB_BROKER_IP, SPB_BROKER_PORT, "fanuc", IPGlobalProperties.GetIPGlobalProperties().HostName, this.machine.Id);
            _protocol = new Protocol(machine["broker"], "fanuc", IPGlobalProperties.GetIPGlobalProperties().HostName, this.machine.Id);
            
            _protocol.add_node_metric("Properties/Hardware Make", "arm");
            _protocol.add_node_metric("Properties/Hardware Model", "l99");
            _protocol.add_node_metric("Properties/OS", "windows");
            _protocol.add_node_metric("Properties/Version", 3.1, MetricTypeEnum.DOUBLE);
            
            await _protocol.give_node_birth();
        }
        
        public override async Task<dynamic?> OnDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            if (veneer.Name == "connect")
                _last_connection_success = veneer.LastArrivedValue.success == true;
            
            if (_protocol.DeviceState == Protocol.DeviceStateEnum.ALIVE)
                return null;

            if (SPB_STRICT)
            {
                process_strict(veneer);
            }
            else
            {
                var name = $"{veneer.Name}{(veneer.SliceKey == null ? string.Empty : "/" + veneer.SliceKey.ToString())}";
                _protocol.add_device_metric(name, veneer.LastArrivedValue, MetricTypeEnum.UNKNOWN);
            }
            
            return null;
        }
        
        public override async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            if (SPB_STRICT)
            {
                process_strict(veneer);
            }
            else
            {
                var name = $"{veneer.Name}{(veneer.SliceKey == null ? string.Empty : "/" + veneer.SliceKey.ToString())}";
                _protocol.add_device_metric(name, veneer.LastChangedValue, MetricTypeEnum.UNKNOWN);
            }

            return null;
        }

        private void process_strict(Veneer veneer)
        {
           switch (veneer.Name) 
           {
            case "power_on_time_6750":
                add_metric(veneer, veneer.LastArrivedValue.ldata);
                break;
            case "get_path":
                add_metric(veneer, veneer.LastArrivedValue.maxpath_no);
                break; 
            case "sys_info":
                add_metric(veneer, _protocol.object_to_dataset(veneer.LastArrivedValue), MetricTypeEnum.DATASET);
                break;
            case "stat_info":
                add_metric(veneer, _protocol.object_to_dataset(veneer.LastArrivedValue), MetricTypeEnum.DATASET);
                break;
            case "axis_name":
                add_metric(veneer, _protocol.array_to_dataset(veneer.LastArrivedValue.axes), MetricTypeEnum.DATASET);
                break;
            case "spindle_name":
                add_metric(veneer, _protocol.array_to_dataset(veneer.LastArrivedValue.spindles), MetricTypeEnum.DATASET);
                break;
            case "axis_data":
                add_metric(veneer, _protocol.object_to_dataset(new
                {
                    veneer.LastArrivedValue.pos.absolute,
                    veneer.LastArrivedValue.pos.machine,
                    veneer.LastArrivedValue.pos.relative,
                    veneer.LastArrivedValue.pos.distance,
                    veneer.LastArrivedValue.alarm,
                    veneer.LastArrivedValue.actf
                }), MetricTypeEnum.DATASET);
                break;
            case "spindle_data":
                add_metric(veneer, veneer.LastArrivedValue.data);
                break;
            case "gcode_blocks":
                _logger.Trace(JArray.FromObject(veneer.LastArrivedValue).ToString());
                break;
           }
        }

        private void add_metric(Veneer veneer, dynamic value, MetricTypeEnum metric_type = MetricTypeEnum.UNKNOWN)
        {
            var name = $"{veneer.Name}{(veneer.SliceKey == null ? string.Empty : "/" + veneer.SliceKey.ToString())}";
            _protocol.add_device_metric(name, value, metric_type);
        }
        
        public override async Task<dynamic?> OnCollectorSweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
        {
            await _protocol.dequeue_node_metrics();
            
            switch (_protocol.DeviceState)
            {
                case Protocol.DeviceStateEnum.NONE:
                    if(_last_connection_success == true)
                        await _protocol.give_device_birth();
                    break;
                
                case Protocol.DeviceStateEnum.ALIVE:
                    if(_last_connection_success == false)
                        await _protocol.give_device_death();
                    else
                        await _protocol.dequeue_device_metrics();
                    break;
                
                case Protocol.DeviceStateEnum.DEAD:
                    if(_last_connection_success == true)
                        await _protocol.give_device_birth();
                    break;
            }
            
            return null;
        }
    }
}