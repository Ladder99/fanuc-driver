using System.Collections.Generic;
using System.Text;
using l99.driver.@base;
using NJsonSchema;

namespace l99.driver.fanuc
{
    public class IntermediateModelGenerator
    {
        private Machine _machine;

        private Dictionary<string, (dynamic, string)> _rootItems = new Dictionary<string, (dynamic, string)>();
        private Dictionary<string, (dynamic, string)> _pathItems = new Dictionary<string, (dynamic, string)>();
        private Dictionary<string, (dynamic, string)> _axisItems = new Dictionary<string, (dynamic, string)>();
        private Dictionary<string, (dynamic, string)> _spindleItems = new Dictionary<string, (dynamic, string)>();
        
        private List<short> _paths = new List<short>();
        private Dictionary<short, List<string>> _axes = new Dictionary<short, List<string>>();
        private Dictionary<short, List<string>> _spindles = new Dictionary<short, List<string>>();
        
        
        public bool IsGenerated { get; private set; }

        public string Model
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("model:");
                foreach(var item in _rootItems)
                    sb.AppendLine($"\t{item.Key}");

                sb.AppendLine($"\tpaths:");
                foreach (short path in _paths)
                {
                    sb.AppendLine($"\t\t{path}:");

                    foreach (var path_item in _pathItems)
                    {
                        sb.AppendLine($"\t\t\t{path_item.Key}");
                    }

                    sb.AppendLine($"\t\t\taxes:");
                    foreach (string axis in _axes[path])
                    {
                        sb.AppendLine($"\t\t\t\t{axis}:");

                        foreach (var axis_item in _axisItems)
                        {
                            sb.AppendLine($"\t\t\t\t\t{axis_item.Key}");
                        }
                    }
                    
                    sb.AppendLine($"\t\t\tspindles:");
                    foreach (string spindle in _spindles[path])
                    {
                        sb.AppendLine($"\t\t\t\t{spindle}:");

                        foreach (var spindle_item in _spindleItems)
                        {
                            sb.AppendLine($"\t\t\t\t\t{spindle_item.Key}");
                        }
                    }
                }

                return sb.ToString();
            }
        }
        
        public void Start(Machine machine)
        {
            _machine = machine;
        }
        
        public void AddRootItem(string name, dynamic payload)
        {
            if(!_rootItems.ContainsKey(name))
                _rootItems.Add(name, (payload, JsonSchema.FromSampleJson(payload).ToString()));
        }

        public void AddPath(short path)
        {
            _paths.Add(path);
            _axes.Add(path, new List<string>());
            _spindles.Add(path, new List<string>());
        }
        
        public void AddPathItem(string name, dynamic payload)
        {
            if(!_pathItems.ContainsKey(name))
                _pathItems.Add(name, (payload, JsonSchema.FromSampleJson(payload).ToString()));
        }

        public void AddAxis(short path, string name)
        {
            _axes[path].Add(name);
        }
        
        public void AddAxisItem(string name, dynamic payload)
        {
            if(!_axisItems.ContainsKey(name))
                _axisItems.Add(name, (payload, JsonSchema.FromSampleJson(payload).ToString()));
        }

        public void AddSpindle(short path, string name)
        {
            _spindles[path].Add(name);
        }
        
        public void AddSpindleItem(string name, dynamic payload)
        {
            if(!_spindleItems.ContainsKey(name))
                _spindleItems.Add(name, (payload, JsonSchema.FromSampleJson(payload).ToString()));
        }

        public void Finish()
        {
            IsGenerated = true;
        }
    }
}