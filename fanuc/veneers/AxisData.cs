using System.Dynamic;
using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class AxisData : Veneer
{
    public AxisData(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(veneers,
        name, isCompound, isInternal)
    {
    }

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        // skip power consumption
        if (nativeInputs.Slice(0, 5).All(o => o.success == true))
        {
            var currentAxis = additionalInputs[0];
            var axesNames = nativeInputs[0];
            var axisDynamic = nativeInputs[1]!.response.cnc_rddynamic2.rddynamic;
            var figures = nativeInputs[2]!.response.cnc_getfigure.dec_fig_in;
            var axesLoads = nativeInputs[3];
            var servoTemp = nativeInputs[4];
            var coderTemp = nativeInputs[5];
            var power = nativeInputs[6];
            var obsFocasSupport = additionalInputs[1];
            IEnumerable<dynamic> obsAlarms = additionalInputs[2]!;
            var prevAxisDynamic = nativeInputs[7];

            var loadFields = axesLoads!.response.cnc_rdsvmeter.loadmeter.GetType().GetFields();
            var loadValue = loadFields[currentAxis - 1]
                .GetValue(axesLoads.response.cnc_rdsvmeter.loadmeter);

            var axesFields = axesNames!.response.cnc_rdaxisname.axisname.GetType().GetFields();
            var axisValue = axesFields[currentAxis - 1]
                .GetValue(axesNames.response.cnc_rdaxisname.axisname);
            var axisName = ((char) axisValue.name).AsAscii() +
                           ((char) axisValue.suff).AsAscii();

            var overTravel = false;
            var overheat = false;
            var servo = false;

            if (obsAlarms != null)
            {
                var axisAlarms = obsAlarms
                    .Where(a => a.axis_code == currentAxis && a.is_triggered);

                // ReSharper disable once PossibleMultipleEnumeration
                overTravel = axisAlarms.Any(a => a.type == "OT");
                // ReSharper disable once PossibleMultipleEnumeration
                overheat = axisAlarms.Any(a => a.type == "OH");
                // ReSharper disable once PossibleMultipleEnumeration
                servo = axisAlarms.Any(a => a.type == "SV");
            }

            var motion = prevAxisDynamic != null && prevAxisDynamic!.success == true
                ? prevAxisDynamic!.response.cnc_rddynamic2.rddynamic.pos.absolute != axisDynamic.pos.absolute
                : false;

            dynamic currentValue = new ExpandoObject();
            currentValue.number = currentAxis;
            currentValue.name = axisName;
            currentValue.feed = axisDynamic.actf;
            currentValue.feed_eu = "mm/min";
            currentValue.load = loadValue.data / Math.Pow(10.0, loadValue.dec);
            currentValue.load_eu = "percent";
            currentValue.servo_temp = servoTemp!.response.cnc_diagnoss.diag.cdata;
            currentValue.servo_temp_eu = "celsius";
            currentValue.coder_temp = coderTemp!.response.cnc_diagnoss.diag.cdata;
            currentValue.coder_temp_eu = "celsius";
            currentValue.power = power!.response.cnc_diagnoss.diag.ldata;
            currentValue.power_eu = "watt";
            currentValue.alarms = new ExpandoObject();
            currentValue.alarms.overtravel = overTravel;
            currentValue.alarms.overheat = overheat;
            currentValue.alarms.overtravel = overTravel;
            currentValue.position = new ExpandoObject();
            currentValue.position.absolute = axisDynamic.pos.absolute / Math.Pow(10.0, figures[currentAxis - 1]);
            currentValue.position.machine = axisDynamic.pos.machine / Math.Pow(10.0, figures[currentAxis - 1]);
            currentValue.position.relative = axisDynamic.pos.relative / Math.Pow(10.0, figures[currentAxis - 1]);
            currentValue.position.distance = axisDynamic.pos.distance / Math.Pow(10.0, figures[currentAxis - 1]);
            currentValue.motion = motion;
            
            await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

            if (((object)currentValue).IsDifferentString((object) LastChangedValue))
                await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);
        }
        else
        {
            await OnHandleErrorAsync(nativeInputs, additionalInputs);
        }

        return new {veneer = this};
    }
}