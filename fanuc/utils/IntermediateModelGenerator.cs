using System;
using System.Collections.Generic;
using l99.driver.@base;
using Newtonsoft.Json.Linq;
using NLog;

namespace l99.driver.fanuc
{
    public class IntermediateModelGenerator
    {
        protected ILogger logger;
        private Machine _machine;

        private Dictionary<string, (List<string>, List<string>)> _structure =
            new Dictionary<string, (List<string>, List<string>)>();
        
        private Dictionary<string, Type> _rootItems = new Dictionary<string, Type>();
        private Dictionary<string, Type> _pathItems = new Dictionary<string, Type>();
        private Dictionary<string, Type> _axisItems = new Dictionary<string, Type>();
        private Dictionary<string, Type> _spindleItems = new Dictionary<string, Type>();
        
        private List<short> _paths = new List<short>();
        private Dictionary<short, List<string>> _axes = new Dictionary<short, List<string>>();
        private Dictionary<short, List<string>> _spindles = new Dictionary<short, List<string>>();
        
        public bool IsGenerated { get; private set; }

        public dynamic Model
        {
            get
            {
                var obs_root = new JArray();
                foreach (var item in _rootItems)
                {
                    obs_root.Add(new JObject(
                        new JProperty("name", item.Key),
                        new JProperty("veneer", item.Value.FullName)
                    ));
                }
                
                var obs_path = new JArray();
                foreach (var item in _pathItems)
                {
                    obs_path.Add(new JObject(
                        new JProperty("name", item.Key),
                        new JProperty("veneer", item.Value.FullName)
                    ));
                }
                
                var obs_axis = new JArray();
                foreach (var item in _axisItems)
                {
                    obs_axis.Add(new JObject(
                        new JProperty("name", item.Key),
                        new JProperty("veneer", item.Value.FullName)
                    ));
                }
                
                var obs_spindle = new JArray();
                foreach (var item in _spindleItems)
                {
                    obs_spindle.Add(new JObject(
                        new JProperty("name", item.Key),
                        new JProperty("type", item.Value.FullName)
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
                
                return new { structure = _structure, model = model.ToString()};
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
        
        public void AddRootItem(string name, Type type)
        {
            try
            {
                if (!_rootItems.ContainsKey(name))
                {
                    logger.Trace($"[{this._machine.Id}] AddRootItem {name}, {type}");
                    _rootItems.Add(name, type);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddRootItem {name}, {type}");
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
                _structure.Add($"{path}", (new List<string>(), new List<string>()));
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddPath.");
            }
        }
        
        public void AddPathItem(string name, Type type)
        {
            try
            {
                if (!_pathItems.ContainsKey(name))
                {
                    logger.Trace($"[{this._machine.Id}] AddPathItem {name}, {type}");
                    _pathItems.Add(name, type);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddPathItem {name}, {type}");
            }
        }

        public void AddAxis(short path, string name)
        {
            try
            {
                logger.Trace($"[{this._machine.Id}] AddAxis {path}, {name}");
                _axes[path].Add(name);
                _structure[$"{path}"].Item1.Add(name);
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddAxis.");
            }
        }
        
        public void AddAxisItem(string name, Type type)
        {
            try
            {
                if (!_axisItems.ContainsKey(name))
                {
                    logger.Trace($"[{this._machine.Id}] AddAxisItem {name}, {type}");
                    _axisItems.Add(name, type);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddAxisItem {name}, {type}");
            }
        }

        public void AddSpindle(short path, string name)
        {
            try
            {
                logger.Trace($"[{this._machine.Id}] AddSpindle {path}, {name}");
                _spindles[path].Add(name);
                _structure[$"{path}"].Item2.Add(name);
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddSpindle.");
            }
        }

        public void AddSpindleItem(string name, Type type)
        {
            try
            {
                if (!_spindleItems.ContainsKey(name))
                {
                    logger.Trace($"[{this._machine.Id}] AddSpindleItem {name}, {type}");
                    _spindleItems.Add(name, type);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex,$"[{this._machine.Id}] AddSpindleItem {name}, {type}");
            }
        }

        public void Finish()
        {
            IsGenerated = true;
            logger.Trace($"[{this._machine.Id}] Finishing build.");
        }
    }
}