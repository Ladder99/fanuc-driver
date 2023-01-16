using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class AlarmsSeries : Veneer
    {
        public AlarmsSeries(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound,
            isInternal)
        {
            lastChangedValue = new
            {
                alarms = new List<dynamic>() {-1}
            };
        }

        private readonly List<SupportMap> _alarmTypeMaps = new()
        {
            new SupportMap
            {
                Expression = "15i[A-Z]?",
                Map = new Dictionary<short, string>
                {
                    {-1,"ALL"},{0,"BG"},{1,"PS"},{2,"OH"},{3,"SB"},{4,"SN"},{5,"SW"},{6,"OT"},{7,"PC"},{8,"EX"},{9,"??"},{10,"SR"},{11,"??"},{12,"SV"},{13,"IO"},{14,"PW"},{15,"??"},{16,"EX"},{17,"EX"},{18,"EX"},{19,"MC"},{20,"SP"}
                }
            },
            new SupportMap
            {
                Expression = "16i[A-Z]?|18i[A-Z]?|21i[A-Z]?|0i[A|B|C]",
                Map = new Dictionary<short, string>
                {
                    {-1,"ALL"},{0,"PS"},{1,"PS"},{2,"PS"},{3,"PS"},{4,"OT"},{5,"OH"},{6,"SV"},{7,"??"},{8,"APC"},{9,"SP"},{10,"PSPP"},{11,"LS"},{12,"??"},{13,"RT"},{14,"??"},{15,"EX"},{16,""},{17,""},{18,""},{19,""},{20,""}
                }
            },
            new SupportMap
            {
                Expression = "30i[A-Z]?|31i[A-Z]?|32i[A-Z]?|0i[D|F]|PMi[A]?",
                Map = new Dictionary<short, string>
                {
                    {-1,"ALL"},{0,"SW"},{1,"PW"},{2,"IO"},{3,"PS"},{4,"OT"},{5,"OH"},{6,"SV"},{7,"SR"},{8,"MC"},{9,"SP"},{10,"DS"},{11,"IE"},{12,"BG"},{13,"SN"},{14,"??"},{15,"EX"},{16,"??"},{17,"??"},{18,"??"},{19,"PC"},{20,"??"}
                }
            }
        };
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if(input.success == true)
            {
                var path = additionalInputs[0];
                var axis = additionalInputs[1];
                var obsFocasSupport = additionalInputs[2];
                var previousInput = additionalInputs[3];

                AlarmResponse previousResponse = GetAlarmCountAndObjectFromInput(previousInput);
                AlarmResponse response = GetAlarmCountAndObjectFromInput(input);

                // TODO: addedd vs removed alarms
                //  who keeps state?
                List<dynamic> previousAlarmList = GetAlarmListFromResponse(previousResponse, path, axis, obsFocasSupport);
                List<dynamic> alarmList = GetAlarmListFromResponse(response, path, axis, obsFocasSupport);
                
                var currentValue = new
                {
                    alarms = alarmList
                };
                
                await OnDataArrivedAsync(input, currentValue);
                
                if(currentValue.alarms.IsDifferentHash((List<dynamic>)lastChangedValue.alarms))
                {
                    await OnDataChangedAsync(input, currentValue);
                }
            }
            else
            {
                await OnHandleErrorAsync(input);
            }

            return new { veneer = this };
        }

        private List<dynamic> GetAlarmListFromResponse(AlarmResponse response, short path, string[] axis, string[] obsFocasSupport)
        {
            List<dynamic> list = new List<dynamic>();
            
            if (response.Count > 0)
            {
                var fields = response.Object.GetType().GetFields();
                for (int x = 0; x <= response.Count - 1; x++)
                {
                    var alarmObject = fields[x].GetValue(response.Object);
                    list.Add(
                        new
                        {
                            path,
                            axis_code = alarmObject.axis,
                            axis = alarmObject.axis > 0 ? axis[alarmObject.axis-1] : "",
                            number = alarmObject.alm_no, 
                            message = ((string)alarmObject.alm_msg).AsAscii(),
                            type_code = alarmObject.type, 
                            type = GetAlarmTypeFromAlarmCode(obsFocasSupport, alarmObject.type)
                        });
                }
            }

            return list;
        }
        
        struct AlarmResponse
        {
            public dynamic Count;
            public dynamic Object;
        }
        
        private AlarmResponse GetAlarmCountAndObjectFromInput(dynamic input)
        {
            if (input == null)
                return new AlarmResponse { Count = 0, Object = null };
            
            var response = input.response
                .GetType().GetProperty(input.method)
                .GetValue(input.response);
            return new AlarmResponse { Count = response.num, Object = response.almmsg };
        }
        
        private string GetAlarmTypeFromAlarmCode(string[] obsFocasSupport, short typeCode)
        {
            string alarmType = "";

            string focasSupport = string.Join("", obsFocasSupport);
            
            foreach(var map in _alarmTypeMaps)
            {
                if (Regex.IsMatch(focasSupport, map.Expression))
                {
                    alarmType = map.Map[typeCode];
                    break;
                }
            }
            
            return alarmType;
        }

        private struct SupportMap
        {
            public string Expression;
            public Dictionary<short, string> Map;
        }
    }
}