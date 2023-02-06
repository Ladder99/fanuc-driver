using System.IO;
using System.Xml.Linq;
using l99.driver.@base;
using Scriban;
using Scriban.Runtime;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc;

// ReSharper disable once InconsistentNaming
public class MTCDeviceModelGenerator
{
    private readonly ILogger _logger;
    private readonly Machine _machine;
    private readonly dynamic _transport;

    public MTCDeviceModelGenerator(Machine machine, dynamic transport)
    {
        _logger = LogManager.GetLogger(GetType().FullName);
        _machine = machine;
        _transport = transport;
    }

    public void Generate(dynamic model)
    {
        if (!_transport["generator"]["enabled"])
            return;

        try
        {
            var generator = _transport["generator"];

            Template tp = null!;
            var so = new ScriptObject();
            var tc = new TemplateContext();

            var paths = model.structure.Keys;

            var axes = new Dictionary<string, List<string>>();
            var spindles = new Dictionary<string, List<string>>();

            foreach (var path in model.structure.Keys)
            {
                axes.Add(path, model.structure[path].Item1);
                spindles.Add(path, model.structure[path].Item2);
            }

            so.Import("GenerateAxis",
                new Func<string, string, string, object>((section, path, axis) =>
                {
                    so.SetValue("path", path, true);
                    so.SetValue("axis", axis, true);
                    so.SetValue("spindle", null, true);
                    tp = Template.Parse(section);
                    var x = tp.Render(tc);
                    so.SetValue("path", null, true);
                    so.SetValue("axis", null, true);
                    return x;
                }));

            so.Import("GenerateSpindle",
                new Func<string, string, string, object>((section, path, spindle) =>
                {
                    so.SetValue("path", path, true);
                    so.SetValue("axis", null, true);
                    so.SetValue("spindle", spindle, true);
                    tp = Template.Parse(section);
                    var x = tp.Render(tc);
                    so.SetValue("path", null, true);
                    so.SetValue("spindle", null, true);
                    return x;
                }));

            so.Import("GeneratePath",
                new Func<string, string, object>((section, path) =>
                {
                    so.SetValue("path", path, true);
                    so.SetValue("axis", null, true);
                    so.SetValue("spindle", null, true);
                    tp = Template.Parse(section);
                    var x = tp.Render(tc);
                    so.SetValue("path", null, true);
                    return x;
                }));

            tc.PushGlobal(so);

            so.SetValue("generator", generator, true);
            so.SetValue("device", _transport["device_name"], true);
            so.SetValue("paths", paths, true);
            so.SetValue("axes", axes, true);
            so.SetValue("spindles", spindles, true);
            tp = Template.Parse(generator["root"]);
            var xml = tp.Render(tc);

            tp = Template.Parse(generator["output"]);
            var fileOut = tp.Render(tc);
            File.WriteAllText(fileOut, XDocument.Parse(xml).ToString());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"[{_machine.Id} MTC device model generation failed!");
        }
    }
}