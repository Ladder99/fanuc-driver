using System;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class SysInfo : Veneer
    {
        public SysInfo(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                loader_control = false,
                i_series = false,
                compound_machining = false,
                transfer_line = false,
                model = string.Empty,
                max_axis = -1,
                cnc_type = string.Empty,
                mt_type = string.Empty,
                series = string.Empty,
                version = string.Empty,
                axes = -1
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                // ADDITIONAL INFO
                byte[] info_bytes = BitConverter.GetBytes(input.response.cnc_sysinfo.sysinfo.addinfo);
                
                var loader_control = (info_bytes[0] & 1) == 1;
                var i_series = (info_bytes[0] & 2) == 2;;
                var compound_machining = (info_bytes[0] & 4) == 4;;
                var transfer_line = (info_bytes[0] & 8) == 8;;
                
                var model = "Unknown";
                switch(info_bytes[1])
                {
                    case 0:
                        model = "MODEL information is not supported";
                        break;
                    case 1:
                        model = "MODEL A";
                        break;
                    case 2:
                        model = "MODEL B";
                        break;
                    case 3:
                        model = "MODEL C";
                        break;
                    case 4:
                        model = "MODEL D";
                        break;
                    case 6:
                        model = "MODEL F";
                        break;
                }
                
                // CNC TYPE
                var cnc_type_code = string.Join("", input.response.cnc_sysinfo.sysinfo.cnc_type);
                var cnc_type = "Unknown";

                switch (cnc_type_code)
                {
                        case "15"	:
                            cnc_type = "Series 15/15i";
                            break;
                        case "16"	:
                            cnc_type = "Series 16/16i";
                            break;
                        case "18"	: 
                            cnc_type = "Series 18/18i";
                            break;
                        case "21"	: 
                            cnc_type = "Series 21/21i";
                            break;
                        case "30"	: 
                            cnc_type = "Series 30i";
                            break;
                        case "31"	: 
                            cnc_type = "Series 31i";
                            break;
                        case "32"	: 
                            cnc_type = "Series 32i";
                            break;
                        case "35"	: 
                            cnc_type = "Series 35i";
                            break;
                        case " 0"	: 
                            cnc_type = "Series 0i";
                            break;
                        case "PD"	: 
                            cnc_type = "Power Mate i-D";
                            break;
                        case "PH"	: 
                            cnc_type = "Power Mate i-H";
                            break;
                        case "PM"	: 
                            cnc_type = "Power Motion i";
                            break;
                }

                // MT TYPE
                var mt_type_code = string.Join("", input.response.cnc_sysinfo.sysinfo.mt_type);
                var mt_type = "Unknown";

                switch (mt_type_code)
                {
                    case " M"	:	
                        mt_type = "Machining center";
                        break;
                    case " T"	:	
                        mt_type = "Lathe";
                        break;
                    case "MM"	:	
                        mt_type = "M series with 2 path control";
                        break;
                    case "TT"	:	
                        mt_type = "T series with 2/3 path control";
                        break;
                    case "MT"	:	
                        mt_type = "T series with compound machining function";
                        break;
                    case " P"	:	
                        mt_type = "Punch press";
                        break;
                    case " L"	:	
                        mt_type = "Laser";
                        break;
                    case " W"	:	
                        mt_type = "Wire cut";
                        break;
                }

                // AXIS COUNT
                dynamic axes;
                short axis_count = 0;
                if (Int16.TryParse(string.Join("", input.response.cnc_sysinfo.sysinfo.axes), out axis_count))
                    axes = axis_count;
                else
                    axes = string.Join("", input.response.cnc_sysinfo.sysinfo.axes);
                
                
                var current_value = new
                {
                    loader_control,
                    i_series,
                    compound_machining,
                    transfer_line,
                    model,
                    input.response.cnc_sysinfo.sysinfo.max_axis,
                    cnc_type,
                    mt_type,
                    series = string.Join("", input.response.cnc_sysinfo.sysinfo.series),
                    version = string.Join("", input.response.cnc_sysinfo.sysinfo.version),
                    axes
                };
                
                await onDataArrivedAsync(input, current_value);
                
                if (!current_value.Equals(this.lastChangedValue))
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
    }
}