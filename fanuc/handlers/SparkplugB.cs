using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using l99.driver.@base;
using l99.driver.fanuc.handlers.spb;
using MoreLinq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.handlers
{
    public class SparkplugB: Handler
    {
        private string SPB_BROKER_IP = "127.0.0.1";
        private int SPB_BROKER_PORT = 1883;
        
        private Protocol _protocol;
        private bool _last_connection_success = false;
        
        public SparkplugB(Machine machine) : base(machine)
        {
            
        }
        
        public override async Task InitializeAsync()
        {
            _protocol = new Protocol("127.0.0.1", 1883, "fanuc", IPGlobalProperties.GetIPGlobalProperties().HostName, this.machine.Id);
            
            _protocol.add_node_metric("Properties/Hardware Make", "arm");
            _protocol.add_node_metric("Properties/Hardware Model", "l99");
            _protocol.add_node_metric("Properties/OS", "windows");
            _protocol.add_node_metric("Properties/Version", 3.1, MetricTypeEnum.DOUBLE);
            
            _protocol.give_node_birth();
        }
        
        public override async Task<dynamic?> OnDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            if (veneer.Name == "connect")
                _last_connection_success = veneer.LastArrivedValue.success == true;
            
            if (_protocol.DeviceState == Protocol.DeviceStateEnum.ALIVE)
                return null;

            return null;
        }
        
        public override async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            /*
            if (veneer.Name == "connect")
            {
                if (veneer.LastChangedValue.success == true)
                {
                    _isDeviceAlive = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(nextSequence() + " > " + string.Format(_topicFormat, "DBIRTH"));
                }
                else
                {
                    _isDeviceAlive = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(nextSequence() + " > " + string.Format(_topicFormat, "DDEATH"));
                }
            }

            if (veneer.Name == "axis_data" && _isDeviceAlive == true)
            {
                _ddata.Add(new
                {
                    name= $"{veneer.Name}/{veneer.SliceKey}",
                    timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    dataType = "Float",
                    value = (float) veneer.LastArrivedValue.pos.absolute
                });
            }
            */
            
            return null;
        }
        
        public override async Task<dynamic?> OnCollectorSweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
        {
            _protocol.dequeue_node_metrics();
            
            switch (_protocol.DeviceState)
            {
                case Protocol.DeviceStateEnum.NONE:
                    if(_last_connection_success == true)
                        _protocol.give_device_birth();
                    break;
                
                case Protocol.DeviceStateEnum.ALIVE:
                    if(_last_connection_success == false)
                        _protocol.give_device_death();
                    else
                        _protocol.dequeue_device_metrics();
                    break;
                
                case Protocol.DeviceStateEnum.DEAD:
                    if(_last_connection_success == true)
                        _protocol.give_device_birth();
                    break;
            }
            
            return null;
        }
    }
}

namespace l99.driver.fanuc.handlers.spb
{
    public class Protocol
    {
        private class MetricWrapper
        {
            public bool processed;
            public Metric metric;
        }
        
        public enum DeviceStateEnum
        {
            NONE,
            ALIVE,
            DEAD
        }

        private DeviceStateEnum _device_state = DeviceStateEnum.NONE;
        public DeviceStateEnum DeviceState
        {
            get => _device_state;
        }
        
        private string _broker_ip;
        private int _broker_port;
        private string _namespace;
        private string _group_id;
        private string _edge_node_id;
        private string _device_id;
        private string _topicFormatNode = $"{{0}}/{{1}}/{{3}}/{{4}}";
        private string _topicFormatDevice = $"{{0}}/{{1}}/{{3}}/{{4}}/{{5}}";

        private int _seq;
        private int _bdSeq;
        private IMqttClient _client;
        
        public Protocol(string broker_ip, int broker_port, string group_id, string edge_node_id, string device_id, string @namespace = "spBv1.0")
        {
            _broker_ip = broker_ip;
            _broker_port = broker_port;
            _group_id = group_id;
            _edge_node_id = edge_node_id;
            _device_id = device_id;
            _namespace = @namespace;
        }

        public string formatTopic(MessageTypeEnum messageType)
        {
            if (messageType == MessageTypeEnum.STATE)
            {
                return "STATE/unknown_scada_host_id";
            }
            else if (new MessageTypeEnum[] {MessageTypeEnum.DBIRTH,MessageTypeEnum.DDEATH,MessageTypeEnum.DDATA,MessageTypeEnum.DCMD}.Contains(messageType))
            {
                return string.Format(_topicFormatNode, _namespace, _group_id, messageType.ToString(), _edge_node_id, _device_id); 
            }
            else if (new MessageTypeEnum[] {MessageTypeEnum.NBIRTH,MessageTypeEnum.NDEATH,MessageTypeEnum.NDATA,MessageTypeEnum.NCMD}.Contains(messageType))
            {
                return string.Format(_topicFormatNode, _namespace, _group_id, messageType.ToString(), _edge_node_id);
            }

            return null;
        }

        private Dictionary<string, MetricWrapper> _node_metrics = new Dictionary<string, MetricWrapper>();
        private Dictionary<string, MetricWrapper> _device_metrics = new Dictionary<string, MetricWrapper>();
        
        public void add_node_metric(string name, dynamic value, MetricTypeEnum type = MetricTypeEnum.STRING)
        {
            if (_node_metrics.ContainsKey(name))
            {
                _node_metrics[name].metric.timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                _node_metrics[name].metric.value = value;
                _node_metrics[name].processed = false;
            }
            else
            {
                _node_metrics.Add(name, new MetricWrapper()
                {
                    processed = false,
                    metric = new Metric()
                    {
                        timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                        value = value,
                        name = name,
                        dataType = type
                    }
                });
            }
        }
        
        public void add_device_metric(string name, dynamic value, MetricTypeEnum type = MetricTypeEnum.STRING)
        {
            if (_device_metrics.ContainsKey(name))
            {
                _device_metrics[name].metric.timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                _device_metrics[name].metric.value = value;
                _device_metrics[name].processed = false;
            }
            else
            {
                _device_metrics.Add(name, new MetricWrapper()
                {
                    processed = false,
                    metric = new Metric()
                    {
                        timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                        value = value,
                        name = name,
                        dataType = type
                    }
                });
            }
        }
        
        private int seqNext()
        {
            if (_seq > 255)
                _seq = 0;

            int ns = _seq;
            _seq++;
            return ns;
        }

        private int seqCurrent()
        {
            return _seq - 1;
        }

        public async Task give_node_birth()
        { 
            create_client();
        }

        private async Task create_client()
        {
            ++_bdSeq;
            add_node_metric("bdSeq", _bdSeq, MetricTypeEnum.UINT64);
            var factory = new MqttFactory();
            var lwt = new MqttApplicationMessageBuilder()
                .WithTopic(formatTopic(MessageTypeEnum.NDEATH))
                .WithPayload(_bdSeq.ToString())
                .Build();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_broker_ip, _broker_port)
                .WithWillMessage(lwt)
                .Build();
            _client = factory.CreateMqttClient();
            await _client.ConnectAsync(options);
            await dequeue_node_metrics(MessageTypeEnum.NBIRTH);
        }

        public async Task give_device_birth()
        {
            await dequeue_device_metrics(MessageTypeEnum.DBIRTH);
        }

        public async Task give_device_death()
        {
            await dequeue_device_metrics(MessageTypeEnum.DDEATH);
        }

        public async Task dequeue_node_metrics(MessageTypeEnum msgType = MessageTypeEnum.NDATA)
        {
            var topic = formatTopic(msgType);

            var metrics = _node_metrics
                .Where(kv => kv.Value.processed == false)
                .Select(kv => kv.Value.metric);
            
            dynamic payload = new
            {
                timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                metrics,
                seq = seqNext()
            };
            
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(JObject.FromObject(payload).ToString())
                .Build();
                
            await _client.PublishAsync(msg, CancellationToken.None);
            
            _node_metrics.ForEach(kv => kv.Value.processed = true);
        }
        
        public async Task dequeue_device_metrics(MessageTypeEnum msgType = MessageTypeEnum.DDATA)
        {
            var topic = formatTopic(msgType);

            var metrics = _device_metrics
                .Where(kv => kv.Value.processed == false)
                .Select(kv => kv.Value.metric);
            
            dynamic payload = new
            {
                timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                metrics,
                seq = seqNext()
            };
            
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(JObject.FromObject(payload).ToString())
                .Build();
                
            await _client.PublishAsync(msg, CancellationToken.None);
            
            _device_metrics.ForEach(kv => kv.Value.processed = true);
        }
    }
    
    [AttributeUsage(validOn: AttributeTargets.Field, AllowMultiple = true)]
    public class RequiredAttribute: Attribute
    {
        public RequiredAttribute(MessageTypeEnum messageType)
        {
        }
    }
    
    public enum MessageTypeEnum
    {
        NBIRTH,
        NDEATH,
        DBIRTH,
        DDEATH,
        NDATA,
        DDATA,
        NCMD,
        DCMD,
        STATE
    }
    
    public enum MetricTypeEnum
    {
        UNKNOWN = 0,
        INT8,
        INT16,
        INT32,
        INT64,
        UINT8,
        UINT16,
        UINT32,
        UINT64,
        FLOAT,
        DOUBLE,
        BOOLEAN,
        STRING,
        DATETIME,
        TEXT,
        UUID,
        DATASET,
        BYTES,
        FILE,
        TEMPLATE = 19
    }
    
    public enum PropertyValueTypeEnum
    {
        UNKNOWN = 0,
        INT8,
        INT16,
        INT32,
        INT64,
        UINT8,
        UINT16,
        UINT32,
        UINT64,
        FLOAT,
        DOUBLE,
        BOOLEAN,
        STRING,
        DATETIME,
        TEXT = 14,
        PROPERTYSET = 20,
        PROPERTYSETLIST = 21
    }
    
    public enum DataSetTypeEnum
    {
        UNKNOWN = 0,
        INT8,
        INT16,
        INT32,
        INT64,
        UINT8,
        UINT16,
        UINT32,
        UINT64,
        FLOAT,
        DOUBLE,
        BOOLEAN,
        STRING,
        DATETIME,
        TEXT = 14
    }
    
    public enum TemplateParameterTypeEnum
    {
        UNKNOWN = 0,
        INT8,
        INT16,
        INT32,
        INT64,
        UINT8,
        UINT16,
        UINT32,
        UINT64,
        FLOAT,
        DOUBLE,
        BOOLEAN,
        STRING,
        DATETIME,
        TEXT = 14
    }

    public class Payload
    {
        public long timestamp;
        public Metric[] metrics;
        public long seq;
        //public string uuid;
        //public byte[] body;
    }

    public class Metric
    {
        [Required(MessageTypeEnum.NBIRTH)]
        [Required(MessageTypeEnum.DBIRTH)]
            public string name;
        
        //public long alias;
    
        public long timestamp;
        
        [Required(MessageTypeEnum.NBIRTH)]
        [Required(MessageTypeEnum.DBIRTH)]
            public MetricTypeEnum dataType;
        
        //public bool is_historical;
        
        //public bool is_transient;
        
        //public bool is_null;
        
        //public Metadata metadata;
        
        //public PropertySet properties;
        
        [Required(MessageTypeEnum.NBIRTH)]
        [Required(MessageTypeEnum.DBIRTH)]
            public dynamic value;
    }

    public class Metadata
    {
        public bool is_multi_part;
        public string content_type;
        public long size;
        public long seq;
        public string file_name;
        public string file_type;
        public string md5;
        public string description;
    }

    public class PropertySet
    {
        public string[] keys;
        public PropertyValue[] values;
    }

    public class PropertyValue
    {
        public PropertyValueTypeEnum type;
        public bool is_null;
        public dynamic value;
    }

    public class PropertySetList
    {
        public PropertySet[] propertyset;
    }

    public class DataSet
    {
        public long num_of_columns;
        public string[] columns;
        public DataSetTypeEnum[] types;
        public DataSetRow[] rows;
        
        public class DataSetRow
        {
            public DataSetValue[] elements;
        }
    
        public class DataSetValue
        {
            public dynamic value;
        }
    }

    public class Template
    {
        public string version;
        public dynamic[] metrics;
        public Parameter[] parameters;
        public string template_ref;
        public bool is_definition;
        
        public class Parameter
        {
            public string name;
            public TemplateParameterTypeEnum type;
            public dynamic value;
        }
    }
}