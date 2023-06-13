using System.Dynamic;
using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class AlarmsSeries : Veneer
{
    private readonly List<SupportMap> _alarmTypeMaps = new()
    {
        new SupportMap
        {
            /*
                0	:	Background P/S	(BG)
                1	:	Foreground P/S	(PS)
                2	:	Overheat alarm	(OH)
                3	:	Sub-CPU error	(SB)
                4	:	Syncronized error	(SN)
                5	:	Parameter switch on	(SW)
                6	:	Overtravel,External data	(OT)
                7	:	PMC error	(PC)
                8	:	External alarm message (1)	(EX)
                9	:	(Not used)	
                10	:	Serious P/S	(SR)
                11	:	(Not used)	
                12	:	Servo alarm	(SV)
                13	:	I/O error	(IO)
                14	:	Power off parameter set	(PW)
                15	:	(Not used)	
                16	:	External alarm message (2)	(EX)
                17	:	External alarm message (3)	(EX)
                18	:	External alarm message (4)	(EX)
                19	:	Macro alarm	(MC)
                20	:	Spindle alarm	(SP)
                -1	:	all type
            */
            Expression = "15i[A-Z]?",
            Map = new Dictionary<short, string>
            {
                {-1, "ALL"}, {0, "BG"}, {1, "PS"}, {2, "OH"}, {3, "SB"}, {4, "SN"}, {5, "SW"}, {6, "OT"}, {7, "PC"},
                {8, "EX"}, {9, "??"}, {10, "SR"}, {11, "??"}, {12, "SV"}, {13, "IO"}, {14, "PW"}, {15, "??"},
                {16, "EX"}, {17, "EX"}, {18, "EX"}, {19, "MC"}, {20, "SP"}
            }
        },
        new SupportMap
        {
            /*
                0	:	P/S100
                1	:	P/S000
                2	:	P/S101
                3	:	P/S alarm except above
                4	:	Overtravel alarm
                5	:	Overheat alarm
                6	:	Servo alarm
                7	:	(Not used)
                8	:	APC alarm
                9	:	Spindle alarm
                10	:	P/S alarm(No.5000,..), Punchpress alarm
                11	:	Laser alarm
                12	:	(Not used)
                13	:	Rigid tap alarm
                14	:	(Not used)
                15	:	External alarm message
                -1	:	all type
            */
            Expression = "16i[A-Z]?|18i[A-Z]?|21i[A-Z]?|0i[A|B|C]",
            Map = new Dictionary<short, string>
            {
                {-1, "ALL"}, {0, "PS"}, {1, "PS"}, {2, "PS"}, {3, "PS"}, {4, "OT"}, {5, "OH"}, {6, "SV"}, {7, "??"},
                {8, "APC"}, {9, "SP"}, {10, "PSPP"}, {11, "LS"}, {12, "??"}, {13, "RT"}, {14, "??"}, {15, "EX"},
                {16, ""}, {17, ""}, {18, ""}, {19, ""}, {20, ""}
            }
        },
        new SupportMap
        {
            /*
                0	:	Parameter switch on	(SW)
                1	:	Power off parameter set	(PW)
                2	:	I/O error	(IO)
                3	:	Foreground P/S	(PS)
                4	:	Overtravel,External data	(OT)
                5	:	Overheat alarm	(OH)
                6	:	Servo alarm	(SV)
                7	:	Data I/O error	(SR)
                8	:	Macro alarm	(MC)
                9	:	Spindle alarm	(SP)
                10	:	Other alarm(DS)	(DS)
                11	:	Alarm concerning Malfunction prevent functions	(IE)
                12	:	Background P/S	(BG)
                13	:	Syncronized error	(SN)
                14	:	(reserved)	
                15	:	External alarm message	(EX)
                16	:	(reserved)	
                17	:	(reserved)	
                18	:	(reserved)	
                19	:	PMC error	(PC)
                20-31	:	(not used)	
                -1	:	All type	
            */
            Expression = "30i[A-Z]?|31i[A-Z]?|32i[A-Z]?|0i[D|F]|PMi[A]?",
            Map = new Dictionary<short, string>
            {
                {-1, "ALL"}, {0, "SW"}, {1, "PW"}, {2, "IO"}, {3, "PS"}, {4, "OT"}, {5, "OH"}, {6, "SV"}, {7, "SR"},
                {8, "MC"}, {9, "SP"}, {10, "DS"}, {11, "IE"}, {12, "BG"}, {13, "SN"}, {14, "??"}, {15, "EX"},
                {16, "??"}, {17, "??"}, {18, "??"}, {19, "PC"}, {20, "??"}
            }
        }
    };

    public AlarmsSeries(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(
        veneers, name, isCompound, isInternal)
    {
    }

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        /*
            nativeInputs
                0: current alarms
                1: previous alarms
            
            additionalInputs
                0: currentPath
                1: axis list
                2: focas support observation
        */
        if (nativeInputs[0].success == true)
        {
            var path = additionalInputs[0];
            var axis = additionalInputs[1];
            var obsFocasSupport = additionalInputs[2];
            var currentInput = nativeInputs[0];
            //var previousInput = nativeInputs[1];

            // check success to use
            //AlarmsWrapper previousAlarmWrapper = GetAlarmsWrapperFromInput(previousInput);
            AlarmsWrapper currentAlarmWrapper = GetAlarmsWrapperFromInput(currentInput);

            //List<dynamic> previousAlarmList = GetAlarmListFromAlarms(previousAlarmWrapper, path, axis, obsFocasSupport);
            List<dynamic> currentAlarmList = GetAlarmListFromAlarms(currentAlarmWrapper, path, axis, obsFocasSupport);

            dynamic currentValue = new ExpandoObject();
            // convert state list to dictionary
            currentValue.alarms = currentAlarmList.ToDictionary(x => x.id, x => x);
            
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

    protected List<dynamic> GetAlarmListFromAlarms(AlarmsWrapper alarms, short path, string[] axis, string[] obsFocasSupport)
    {
        var list = new List<dynamic>();

        if (alarms.Count > 0)
        {
            var fields = alarms.Object.GetType().GetFields();
            for (var x = 0; x <= alarms.Count - 1; x++)
            {
                var alarmObject = fields[x].GetValue(alarms.Object);
                if(alarmObject.alm_no >= 0)
                    list.Add(new 
                        {
                            path,
                            axis_code = alarmObject.axis,
                            axis = alarmObject.axis > 0 ? axis[alarmObject.axis - 1] : "",
                            number = alarmObject.alm_no,
                            message = ((string) alarmObject.alm_msg).AsAscii(),
                            type_code = alarmObject.type,
                            type = GetAlarmTypeFromAlarmCode(obsFocasSupport, alarmObject.type),
                            id = $"{GetAlarmTypeFromAlarmCode(obsFocasSupport, alarmObject.type)}{alarmObject.alm_no:D4}",
                            is_triggered = true
                        });
            }
        }

        return list;
    }

    protected AlarmsWrapper GetAlarmsWrapperFromInput(dynamic input)
    {
        if (input == null)
            return new AlarmsWrapper {Count = 0, Object = null};

        var response = input.response
            .GetType().GetProperty(input.method)
            .GetValue(input.response);
        return new AlarmsWrapper {Count = response.num, Object = response.almmsg};
    }

    protected string GetAlarmTypeFromAlarmCode(string[] obsFocasSupport, short typeCode)
    {
        var alarmType = "";

        var focasSupport = string.Join("", obsFocasSupport);

        foreach (var map in _alarmTypeMaps)
            if (Regex.IsMatch(focasSupport, map.Expression))
            {
                alarmType = map.Map[typeCode];
                break;
            }

        return alarmType;
    }

    protected struct AlarmsWrapper
    {
        public dynamic Count;
        public dynamic Object;
    }

    private struct SupportMap
    {
        public string Expression;
        public Dictionary<short, string> Map;
    }
}