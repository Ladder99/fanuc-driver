using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class RdParaInfo: Veneer
    {
        public RdParaInfo(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                info_no = -1,
                prev_no = -1,
                next_no = -1,
                info = new List<dynamic>()
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                var temp_value = new List<dynamic>();
                
                var fields = input.response.cnc_rdparainfo.paraif.info.GetType().GetFields();
                for (int x = 0; x <= input.response.cnc_rdparainfo.paraif.info_no - 1; x++)
                {
                    var para = fields[x].GetValue(input.response.cnc_rdparainfo.paraif.info);
                    var type = Convert.ToString((short) para.prm_type, 2)
                        .PadLeft(16, '0')
                        .Select(c => c.Equals('1')).Reverse().ToArray();

                    var data_type_1 = "bit";
                    if (type[0] && type[1]) data_type_1 = "2-word";
                    else if (type[1]) data_type_1 = "word";
                    else if (type[0]) data_type_1 = "byte";
                    
                    var data_type_2 = "bit";
                    if (type[11]) data_type_2 = "real";
                    else if (type[9]&&type[10]) data_type_2 = "2-word";
                    else if (type[10]) data_type_2 = "word";
                    else if (type[9]) data_type_2 = "byte";
                    
                    temp_value.Add(new
                    {
                        number = para.prm_no,
                        data_type_1,
                        data_type_2,
                        axis = type[2],
                        sign = !type[3],
                        settings_input = type[4],
                        write_protection = !type[5],
                        restart_after_write = type[6],
                        read_protect = !type[7],
                        spindle = type[8],
                        real = type[12]
                    });
                }

                var current_value = new
                {
                    input.response.cnc_rdparainfo.paraif.info_no,
                    input.response.cnc_rdparainfo.paraif.prev_no,
                    input.response.cnc_rdparainfo.paraif.next_no,
                    info = temp_value
                };
                
                await OnDataArrivedAsync(input, current_value);
                
                //todo: has to be a better way to compare dynamic
                if(!current_value.info_no.Equals(lastChangedValue.info_no) ||
                   !current_value.prev_no.Equals(lastChangedValue.prev_no) ||
                   !current_value.next_no.Equals(lastChangedValue.next_no) ||
                    current_value.info.IsDifferentHash((List<dynamic>)lastChangedValue.info))
                    await OnDataChangedAsync(input, current_value);
                
            }
            else
            {
                await onErrorAsync(input);
            }
            
            return new { veneer = this };
        }
    }
}