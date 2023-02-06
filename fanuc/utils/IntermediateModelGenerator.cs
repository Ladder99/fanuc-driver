using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.utils;

public class IntermediateModelGenerator
{
    private readonly Dictionary<short, List<string>> _axes = new();
    private readonly Dictionary<string, Type> _axisItems = new();
    private readonly ILogger _logger;
    private readonly Dictionary<string, Type> _pathItems = new();

    private readonly List<short> _paths = new();

    private readonly Dictionary<string, Type> _rootItems = new();
    private readonly Dictionary<string, Type> _spindleItems = new();
    private readonly Dictionary<short, List<string>> _spindles = new();

    private readonly Dictionary<string, (List<string>, List<string>)> _structure = new();
    private Machine _machine = null!;

    public IntermediateModelGenerator()
    {
        _logger = LogManager.GetLogger(GetType().FullName);
    }

    public bool IsGenerated { get; private set; }

    public dynamic Model
    {
        get
        {
            var obsRoot = new JArray();
            foreach (var item in _rootItems)
                obsRoot.Add(new JObject(
                    new JProperty("name", item.Key),
                    new JProperty("veneer", item.Value.FullName)
                ));

            var obsPath = new JArray();
            foreach (var item in _pathItems)
                obsPath.Add(new JObject(
                    new JProperty("name", item.Key),
                    new JProperty("veneer", item.Value.FullName)
                ));

            var obsAxis = new JArray();
            foreach (var item in _axisItems)
                obsAxis.Add(new JObject(
                    new JProperty("name", item.Key),
                    new JProperty("veneer", item.Value.FullName)
                ));

            var obsSpindle = new JArray();
            foreach (var item in _spindleItems)
                obsSpindle.Add(new JObject(
                    new JProperty("name", item.Key),
                    new JProperty("type", item.Value.FullName)
                ));

            var model = new JObject();
            model["handler"] = new JObject(
                new JProperty("name", _machine.Handler.GetType().Name),
                new JProperty("type", _machine.Handler.GetType().FullName)
            );
            model["strategy"] = new JObject(
                new JProperty("name", _machine.Strategy.GetType().Name),
                new JProperty("type", _machine.Strategy.GetType().FullName)
            );
            model["observations"] = new JObject();
            model["observations"]!["root"] = obsRoot;
            model["observations"]!["path"] = obsPath;
            model["observations"]!["axis"] = obsAxis;
            model["observations"]!["spindle"] = obsSpindle;

            model["structure"] = new JObject();
            model["structure"]!["observations"] = new JObject(
                new JProperty("$ref", "#/observations/root"));

            var pathArray = new JArray();

            foreach (var path in _paths)
            {
                var axisArray = new JArray();
                //var axes = new JObject();
                foreach (var axis in _axes[path])
                    axisArray.Add(new JObject(
                        new JProperty("name", axis),
                        new JProperty("observations", new JObject(new JProperty("$ref", "#/observations/axis")))
                    ));

                var spindleArray = new JArray();
                //var spindles = new JObject();
                foreach (var spindle in _spindles[path])
                    spindleArray.Add(new JObject(
                        new JProperty("name", spindle),
                        new JProperty("observations", new JObject(new JProperty("$ref", "#/observations/spindle")))
                    ));

                pathArray.Add(new JObject(
                    new JProperty("name", path),
                    new JProperty("observations", new JObject(new JProperty("$ref", "#/observations/path"))),
                    new JProperty("axis", axisArray),
                    new JProperty("spindle", spindleArray)));
            }

            model["structure"]!["path"] = pathArray;

            return new {structure = _structure, model = model.ToString()};
        }
    }

    public void Start(Machine machine)
    {
        _logger.Trace($"[{machine.Id}] Starting build.");
        _machine = machine;
    }

    public void AddRootItem(string name, Type type)
    {
        try
        {
            if (!_rootItems.ContainsKey(name))
            {
                _logger.Trace($"[{_machine.Id}] AddRootItem {name}, {type}");
                _rootItems.Add(name, type);
            }
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, $"[{_machine.Id}] AddRootItem {name}, {type}");
        }
    }

    public void AddPath(short path)
    {
        try
        {
            _logger.Trace($"[{_machine.Id}] AddPath {path}");
            _paths.Add(path);
            _axes.Add(path, new List<string>());
            _spindles.Add(path, new List<string>());
            _structure.Add($"{path}", (new List<string>(), new List<string>()));
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, $"[{_machine.Id}] AddPath.");
        }
    }

    public void AddPathItem(string name, Type type)
    {
        try
        {
            if (!_pathItems.ContainsKey(name))
            {
                _logger.Trace($"[{_machine.Id}] AddPathItem {name}, {type}");
                _pathItems.Add(name, type);
            }
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, $"[{_machine.Id}] AddPathItem {name}, {type}");
        }
    }

    public void AddAxis(short path, string name)
    {
        try
        {
            _logger.Trace($"[{_machine.Id}] AddAxis {path}, {name}");
            _axes[path].Add(name);
            _structure[$"{path}"].Item1.Add(name);
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, $"[{_machine.Id}] AddAxis.");
        }
    }

    public void AddAxisItem(string name, Type type)
    {
        try
        {
            if (!_axisItems.ContainsKey(name))
            {
                _logger.Trace($"[{_machine.Id}] AddAxisItem {name}, {type}");
                _axisItems.Add(name, type);
            }
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, $"[{_machine.Id}] AddAxisItem {name}, {type}");
        }
    }

    public void AddSpindle(short path, string name)
    {
        try
        {
            _logger.Trace($"[{_machine.Id}] AddSpindle {path}, {name}");
            _spindles[path].Add(name);
            _structure[$"{path}"].Item2.Add(name);
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, $"[{_machine.Id}] AddSpindle.");
        }
    }

    public void AddSpindleItem(string name, Type type)
    {
        try
        {
            if (!_spindleItems.ContainsKey(name))
            {
                _logger.Trace($"[{_machine.Id}] AddSpindleItem {name}, {type}");
                _spindleItems.Add(name, type);
            }
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, $"[{_machine.Id}] AddSpindleItem {name}, {type}");
        }
    }

    public void Finish()
    {
        IsGenerated = true;
        _logger.Trace($"[{_machine.Id}] Finishing build.");
    }
}