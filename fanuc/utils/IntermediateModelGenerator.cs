using System;
using System.Collections.Generic;
using l99.driver.@base;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NLog;

namespace l99.driver.fanuc
{
    public class IntermediateModelGenerator
    {
        protected ILogger logger;
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
                }
                
                var obs_path = new JArray();
                foreach (var item in _pathItems)
                {
                    obs_path.Add(new JObject(
                        new JProperty("name", item.Key),
                        new JProperty("veneer", item.Value.Item1.veneer.GetType().FullName)
                        //new JProperty("schema", JObject.Parse(item.Value.Item2))
                    ));
                }
                
                var obs_axis = new JArray();
                foreach (var item in _axisItems)
                {
                    obs_axis.Add(new JObject(
                        new JProperty("name", item.Key),
                        new JProperty("veneer", item.Value.Item1.veneer.GetType().FullName)
                        //new JProperty("schema", JObject.Parse(item.Value.Item2))
                    ));
                }
                
                var obs_spindle = new JArray();
                foreach (var item in _spindleItems)
                {
                    obs_spindle.Add(new JObject(
                        new JProperty("name", item.Key),
                        new JProperty("type", item.Value.Item1.veneer.GetType().FullName)
                        //new JProperty("schema", JObject.Parse(item.Value.Item2))
                    ));
                }
                
                JObject model = new JObject();
                model["handler"] = new JObject(
                    new JProperty("name", _machine.Handler.GetType().Name),
                    new JProperty("type", _machine.Handler.GetType().FullName)
                );
                model["strategy"] = new JObject(
                    new JProperty("name", _machine.Strategy.GetType().Name),
                    new JProperty("type", _machine.Strategy.GetType().FullName)
                );
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
                    }

                    var spindle_array = new JArray();
                    //var spindles = new JObject();
                    foreach (string spindle in _spindles[path])
                    {
                        spindle_array.Add(new JObject(
                            new JProperty("name", spindle),
                            new JProperty("observations", new JObject(new JProperty("$ref", "#/observations/spindle")))
                        ));
                    }

                    path_array.Add(new JObject(
                        new JProperty("name", path),
                        new JProperty("observations", new JObject(new JProperty("$ref", "#/observations/path"))),
                        new JProperty("axis", axis_array),
                        new JProperty("spindle", spindle_array)));
                }

                model["structure"]["path"] = path_array;
                
                var j = model.ToString();
                return j;
            }
        }

        public IntermediateModelGenerator()
        {
            logger = LogManager.GetLogger(this.GetType().FullName);
        }
        
        public void Start(Machine machine)
        {
            logger.Trace($"[{machine.Id}] Starting build.");
            _machine = machine;
        }
        
        public void AddRootItem(string name, dynamic payload)
        {
            try
            {
                if (!_rootItems.ContainsKey(name))
                {
                    var json_string = JObject.FromObject(payload.veneer.LastArrivedValue).ToString();
                    var json_schema = string.Empty; //JsonSchema.FromSampleJson(json_string).ToJson();
                    logger.Trace($"[{this._machine.Id}] AddRootItem {json_string}, {json_schema}");
                    _rootItems.Add(name, (payload, json_schema));
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddRootItem.");
            }
        }

        public void AddPath(short path)
        {
            try
            {
                logger.Trace($"[{this._machine.Id}] AddPath {path}");
                _paths.Add(path);
                _axes.Add(path, new List<string>());
                _spindles.Add(path, new List<string>());
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddPath.");
            }
        }
        
        public void AddPathItem(string name, dynamic payload)
        {
            try
            {
                if (!_pathItems.ContainsKey(name))
                {
                    var json_string = JObject.FromObject(payload.veneer.LastArrivedValue).ToString();
                    var json_schema = string.Empty; //JsonSchema.FromSampleJson(json_string).ToJson();
                    logger.Trace($"[{this._machine.Id}] AddPathItem {name}, {json_string}");
                    _pathItems.Add(name, (payload, json_schema));
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddPathItem.");
            }
        }

        public void AddAxis(short path, string name)
        {
            try
            {
                logger.Trace($"[{this._machine.Id}] AddAxis {path}, {name}");
                _axes[path].Add(name);
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddAxis.");
            }
        }
        
        public void AddAxisItem(string name, dynamic payload)
        {
            try
            {
                if (!_axisItems.ContainsKey(name))
                {
                    var json_string = JObject.FromObject(payload.veneer.LastArrivedValue).ToString();
                    var json_schema = string.Empty; //JsonSchema.FromSampleJson(json_string).ToJson();
                    logger.Trace($"[{this._machine.Id}] AddAxisItem {name}, {json_string}");
                    _axisItems.Add(name, (payload, json_schema));
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddAxisItem.");
            }
        }

        public void AddSpindle(short path, string name)
        {
            try
            {
                logger.Trace($"[{this._machine.Id}] AddSpindle {path}, {name}");
                _spindles[path].Add(name);
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddSpindle.");
            }
        }

        public void AddSpindleItem(string name, dynamic payload)
        {
            try
            {
                if (!_spindleItems.ContainsKey(name))
                {
                    var json_string = JObject.FromObject(payload.veneer.LastArrivedValue).ToString();
                    var json_schema = JsonSchema.FromSampleJson(json_string).ToJson();
                    logger.Trace($"[{this._machine.Id}] AddSpindleItem {name}, {json_string}");
                    _spindleItems.Add(name, (payload, json_schema));
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddSpindleItem.");
            }
        }

        public void Finish()
        {
            IsGenerated = true;
            logger.Trace($"[{this._machine.Id}] Finishing build.");
        }
    }
}