using System.Collections.Generic;
using System.Text;
using l99.driver.@base;
using Newtonsoft.Json.Linq;
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
                var obs_root = new JArray();
                foreach (var item in _rootItems)
                {
                    obs_root.Add(new JObject(
                        new JProperty("name", item.Key),
                        new JProperty("veneer", item.Value.Item1.veneer.GetType().FullName)
                        //new JProperty("schema", JObject.Parse(item.Value.Item2))
                    ));
                    /*
                    obs_root[item.Key] = new JObject();
                    obs_root[item.Key]["veneer"] = item.Value.Item1.veneer.GetType().FullName;
                    obs_root[item.Key]["schema"] = JObject.Parse(item.Value.Item2);
                    */
                }
                
                var obs_path = new JArray();
                foreach (var item in _pathItems)
                {
                    obs_path.Add(new JObject(
                        new JProperty("name", item.Key),
                        new JProperty("veneer", item.Value.Item1.veneer.GetType().FullName)
                        //new JProperty("schema", JObject.Parse(item.Value.Item2))
                    ));
                    /*
                    obs_path[item.Key] = new JObject();
                    obs_path[item.Key]["veneer"] = item.Value.Item1.veneer.GetType().FullName;
                    obs_path[item.Key]["schema"] = JObject.Parse(item.Value.Item2);
                    */
                }
                
                var obs_axis = new JArray();
                foreach (var item in _axisItems)
                {
                    obs_axis.Add(new JObject(
                        new JProperty("name", item.Key),
                        new JProperty("veneer", item.Value.Item1.veneer.GetType().FullName)
                        //new JProperty("schema", JObject.Parse(item.Value.Item2))
                    ));
                    /*
                    obs_axis[item.Key] = new JObject();
                    obs_axis[item.Key]["veneer"] = item.Value.Item1.veneer.GetType().FullName;
                    obs_axis[item.Key]["schema"] = JObject.Parse(item.Value.Item2);
                    */
                }
                
                var obs_spindle = new JArray();
                foreach (var item in _spindleItems)
                {
                    obs_spindle.Add(new JObject(
                        new JProperty("name", item.Key),
                        new JProperty("veneer", item.Value.Item1.veneer.GetType().FullName)
                        //new JProperty("schema", JObject.Parse(item.Value.Item2))
                    ));
                    /*
                    obs_spindle[item.Key] = new JObject();
                    obs_spindle[item.Key]["veneer"] = item.Value.Item1.veneer.GetType().FullName;
                    obs_spindle[item.Key]["schema"] = JObject.Parse(item.Value.Item2);
                    */
                }
                
                JObject model = new JObject();
                model["observations"] = new JObject();
                model["observations"]["root"] = obs_root;
                model["observations"]["path"] = obs_path;
                model["observations"]["axis"] = obs_axis;
                model["observations"]["spindle"] = obs_spindle;

                model["structure"] = new JObject();
                model["structure"]["observations"] = new JObject(
                    new JProperty("$ref", "#/observations/root"));

                var path_array = new JArray();
                
                foreach (short path in _paths)
                {
                    var axis_array = new JArray();
                    //var axes = new JObject();
                    foreach (string axis in _axes[path])
                    {
                        axis_array.Add(new JObject(
                            new JProperty("name", axis),
                            new JProperty("observations", new JObject(new JProperty("$ref", "#/observations/axis")))
                        ));
                        /*
                        axes[axis] = new JObject();
                        axes[axis]["observations"] = new JObject(
                            new JProperty("$ref", "#/observations/axis"));
                        */
                    }

                    var spindle_array = new JArray();
                    //var spindles = new JObject();
                    foreach (string spindle in _spindles[path])
                    {
                        spindle_array.Add(new JObject(
                            new JProperty("name", spindle),
                            new JProperty("observations", new JObject(new JProperty("$ref", "#/observations/spindle")))
                        ));
                        /*
                        spindles[spindle] = new JObject();
                        spindles[spindle]["observations"] = new JObject(
                            new JProperty("$ref", "#/observations/spindle"));
                        */
                    }

                    path_array.Add(new JObject(
                        new JProperty("name", path),
                        new JProperty("observations", new JObject(new JProperty("$ref", "#/observations/path"))),
                        new JProperty("axis", axis_array),
                        new JProperty("spindle", spindle_array)));
                    
                   /* model["structure"]["path"][path.ToString()] = new JObject(
                        new JProperty("observations", new JObject(new JProperty("$ref", "#/observations/path"))),
                        new JProperty("axis", axes),
                        new JProperty("spindle", spindles));*/
                }

                model["structure"]["path"] = path_array;
                
                var j = model.ToString();
                return j;
                /*
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

                return sb.ToString();*/
            }
        }
        
        public void Start(Machine machine)
        {
            _machine = machine;
        }
        
        public void AddRootItem(string name, dynamic payload)
        {
            if(!_rootItems.ContainsKey(name))
            {
                var json_string = JObject.FromObject(payload.veneer.LastArrivedValue).ToString();
                var json_schema = string.Empty;//JsonSchema.FromSampleJson(json_string).ToJson();
                _rootItems.Add(name, (payload, json_schema));
            }
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
            {
                var json_string = JObject.FromObject(payload.veneer.LastArrivedValue).ToString();
                var json_schema = string.Empty;//JsonSchema.FromSampleJson(json_string).ToJson();
                _pathItems.Add(name, (payload, json_schema));
            }
        }

        public void AddAxis(short path, string name)
        {
            _axes[path].Add(name);
        }
        
        public void AddAxisItem(string name, dynamic payload)
        {
            if(!_axisItems.ContainsKey(name))
            {
                var json_string = JObject.FromObject(payload.veneer.LastArrivedValue).ToString();
                var json_schema = string.Empty;//JsonSchema.FromSampleJson(json_string).ToJson();
                _axisItems.Add(name, (payload, json_schema));
            }
        }

        public void AddSpindle(short path, string name)
        {
            _spindles[path].Add(name);
        }

        public void AddSpindleItem(string name, dynamic payload)
        {
            if (!_spindleItems.ContainsKey(name))
            {
                var json_string = JObject.FromObject(payload.veneer.LastArrivedValue).ToString();
                var json_schema = JsonSchema.FromSampleJson(json_string).ToJson();
                _spindleItems.Add(name, (payload, json_schema));
            }
        }

        public void Finish()
        {
            IsGenerated = true;
        }
    }
}