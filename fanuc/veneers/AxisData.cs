#pragma warning disable CS8602, CS8600

using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class AxisData : Veneer
    {
        public AxisData(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            // skip index 7 - power consumption
            if (additionalInputs.Slice(0,6).All(o => o.success == true))
            {
                var currentAxis = input;
                var axesNames = additionalInputs[0];
                var axisDynamic = additionalInputs[1].response.cnc_rddynamic2.rddynamic;
                var figures = additionalInputs[2].response.cnc_getfigure.dec_fig_in;
                var axesLoads = additionalInputs[3];
                var servoTemp = additionalInputs[4];
                var coderTemp = additionalInputs[5];
                var power = additionalInputs[6];
                // ReSharper disable once UnusedVariable
                var obsFocasSupport = additionalInputs[7];
                IEnumerable<dynamic> obsAlarms = additionalInputs[8];
                var prevAxisDynamic = additionalInputs[9];

                var loadFields = axesLoads.response.cnc_rdsvmeter.loadmeter.GetType().GetFields();
                var loadValue = loadFields[currentAxis - 1]
                    .GetValue(axesLoads.response.cnc_rdsvmeter.loadmeter);
                
                var axesFields = axesNames.response.cnc_rdaxisname.axisname.GetType().GetFields();
                var axisValue = axesFields[currentAxis - 1]
                    .GetValue(axesNames.response.cnc_rdaxisname.axisname);
                var axisName = ((char) axisValue.name).AsAscii() +
                                   ((char) axisValue.suff).AsAscii();

                bool overTravel = false;
                bool overheat = false;
                bool servo = false;

                if (obsAlarms != null)
                {
                    var axisAlarms = obsAlarms
                        .Where(a => a.axis_code == currentAxis);

                    // ReSharper disable once PossibleMultipleEnumeration
                    overTravel = axisAlarms.Any(a => a.type == "OT");
                    // ReSharper disable once PossibleMultipleEnumeration
                    overheat = axisAlarms.Any(a => a.type == "OH");
                    // ReSharper disable once PossibleMultipleEnumeration
                    servo = axisAlarms.Any(a => a.type == "SV");
                }

                bool motion = (prevAxisDynamic != null && prevAxisDynamic.success == true) 
                    ? prevAxisDynamic.response.cnc_rddynamic2.rddynamic.pos.absolute != axisDynamic.pos.absolute
                    : false;
                
                var currentValue = new
                {
                    number = currentAxis,
                    name = axisName,
                    feed = axisDynamic.actf,
                    feed_eu = "mm/min",
                    load = loadValue.data / Math.Pow(10.0, loadValue.dec),
                    load_eu = "percent",
                    servo_temp = servoTemp.response.cnc_diagnoss.diag.cdata,
                    servo_temp_eu = "celsius",
                    coder_temp = coderTemp.response.cnc_diagnoss.diag.cdata,
                    coder_temp_eu = "celsius",
                    power = power.response.cnc_diagnoss.diag.ldata,
                    power_eu = "watt",
                    alarms = new
                    {
                        overtravel = overTravel,
                        overheat,
                        servo
                    },
                    position = new
                    {
                        absolute = axisDynamic.pos.absolute / Math.Pow(10.0, figures[currentAxis-1]),
                        machine = axisDynamic.pos.machine / Math.Pow(10.0, figures[currentAxis-1]),
                        relative = axisDynamic.pos.relative / Math.Pow(10.0, figures[currentAxis-1]),
                        distance = axisDynamic.pos.distance / Math.Pow(10.0, figures[currentAxis-1])
                    },
                    motion
                };

                await OnDataArrivedAsync(input, currentValue);
                
                if(currentValue.IsDifferentString((object)lastChangedValue))
                {
                    await OnDataChangedAsync(input, currentValue);
                }
            }
            else
            {
                await onErrorAsync(input);
            }

            return new { veneer = this };
        }
    }
}
#pragma warning restore CS8602, CS8600