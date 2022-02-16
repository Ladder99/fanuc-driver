using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class AlarmsSeries : Veneer
    {
        public AlarmsSeries(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new 
            {
                alarms = new List<dynamic>() { -1 }
            };
        }

        private string rex1 = "15i[A-Z]?";
        private enum map1 { ALL=-1,BG,PS,OH,SB,SN,SW,OT,PC,EX,NU9,SR,NU11,SV,IO,PW,NU15,EX2,EX3,EX4,MC,SP };

        private string rex2 = "16i[A-Z]?|18i[A-Z]?|21i[A-Z]?|0i[A|B|C]";
        private enum map2 { ALL=-1,PS100,PS000,PS101,PS,OT,OH,SV,NU7,APC,SP,PSPP,LS,NU12,RT,NU14,EX };

        //private string rex3 = "";
        //private enum map3 { ALL=-1,SW,PW,IO,PS,OT,OH,SV,SR,MC,SP,DS,IE,BH,SN,RV16,RV17,RV18,PC };

        private string rex4 = "30i[A-Z]?|31i[A-Z]?|32i[A-Z]?|0i[D|F]|PMi[A]?";
        private enum map4 { ALL=-1,SW,PW,IO,PS,OT,OH,SV,SR,MC,SP,DS,IE,BG,SN,RV14,EX,RV16,RV17,RV18,PC,NU20,NU21,NU22,NU23,NU24,NU25,NU26,NU27,NU28,NU29,NU30,NU31};
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if(input.success == true)
            {
                var path = additionalInputs[0];
                var axis = additionalInputs[1];
                var obs_focas_support = additionalInputs[2];
                
                
                var temp_value = new List<dynamic>() ;
                var response = input.response.GetType().GetProperty(input.method).GetValue(input.response);
                var count = response.num;
                var alms = response.almmsg;
                
                if (count > 0)
                {
                    var fields = alms.GetType().GetFields();
                    for (int x = 0; x <= count - 1; x++)
                    {
                        var alm = fields[x].GetValue(alms);
                        temp_value.Add(
                            new
                            {
                                path,
                                axis_code = alm.axis,
                                axis = axis[alm.axis],
                                number = alm.alm_no, 
                                message = ((string)alm.alm_msg).AsAscii(),
                                type_code = alm.type, 
                                type = getAlmType(obs_focas_support, alm.type)
                            });
                    }
                }
            
                var current_value = new
                {
                    alarms = temp_value
                };
                
                await onDataArrivedAsync(input, current_value);
                
                if(current_value.alarms.IsDifferentHash((List<dynamic>)lastChangedValue.alarms))
                {
                    await onDataChangedAsync(input, current_value);
                }
            }
            else
            {
                await onErrorAsync(input);
            }

            return new { veneer = this };
        }

        string getAlmType(string[] focas_support, short type_code)
        {
            string almType = "";
            
            if(Regex.IsMatch(string.Join("",focas_support),rex4))
            {
                almType = Enum.GetName(typeof(map4), type_code);
            }
            else if(Regex.IsMatch(string.Join("",focas_support),rex2))
            {
                almType = Enum.GetName(typeof(map2), type_code);
            }
            if(Regex.IsMatch(string.Join("",focas_support),rex1))
            {
                almType = Enum.GetName(typeof(map1), type_code);
            }

            return almType;
        }
    }
}