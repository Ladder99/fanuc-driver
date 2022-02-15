using System;
using System.Collections.Generic;
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
                focas_suport = new string[4],
                loader_control = false,
                i_series = false,
                compound_machining = false,
                transfer_line = false,
                model = string.Empty,
                model_code = -1,
                max_axis = -1,
                cnc_type = string.Empty,
                cnc_type_code = -1,
                mt_type = string.Empty,
                mt_type_code = -1,
                series = string.Empty,
                version = string.Empty,
                axes = -1,
                focas_series = string.Empty
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                string[] focas_support = new string[4];
                
                // ADDITIONAL INFO
                byte[] info_bytes = BitConverter.GetBytes(input.response.cnc_sysinfo.sysinfo.addinfo);
                
                var loader_control = (info_bytes[0] & 1) == 1;
                var i_series = (info_bytes[0] & 2) == 2;;
                var compound_machining = (info_bytes[0] & 4) == 4;;
                var transfer_line = (info_bytes[0] & 8) == 8;;

                focas_support[1] = i_series ? "i" : "";
                
                var model = "Unknown";
                switch(info_bytes[1])
                {
                    case 0:
                        model = "MODEL information is not supported";
                        focas_support[2] = "";
                        break;
                    case 1:
                        model = "MODEL A";
                        focas_support[2] = "A";
                        break;
                    case 2:
                        model = "MODEL B";
                        focas_support[2] = "B";
                        break;
                    case 3:
                        model = "MODEL C";
                        focas_support[2] = "C";
                        break;
                    case 4:
                        model = "MODEL D";
                        focas_support[2] = "D";
                        break;
                    case 6:
                        model = "MODEL F";
                        focas_support[2] = "F";
                        break;
                }
                
                // CNC TYPE
                var cnc_type_code = string.Join("", input.response.cnc_sysinfo.sysinfo.cnc_type);
                var cnc_type = "Unknown";
                focas_support[0] = cnc_type_code.Trim();
                
                switch (cnc_type_code)
                {
                        case "15"	:
                            cnc_type = "Series 15" + (i_series?"i":"");
                            break;
                        case "16"	:
                            cnc_type = "Series 16" + (i_series?"i":"");
                            break;
                        case "18"	: 
                            cnc_type = "Series 18" + (i_series?"i":"");
                            break;
                        case "21"	: 
                            cnc_type = "Series 21" + (i_series?"i":"");
                            break;
                        case "30"	: 
                            cnc_type = "Series 30" + (i_series?"i":"");
                            break;
                        case "31"	: 
                            cnc_type = "Series 31" + (i_series?"i":"");
                            break;
                        case "32"	: 
                            cnc_type = "Series 32" + (i_series?"i":"");
                            break;
                        case "35"	: 
                            cnc_type = "Series 35" + (i_series?"i":"");
                            break;
                        case " 0"	: 
                            cnc_type = "Series 0" + (i_series?"i":"");
                            break;
                        case "PD"	: 
                            cnc_type = "Power Mate D";
                            if (i_series) cnc_type = "Power Mate i-D";
                            break;
                        case "PH"	: 
                            cnc_type = "Power Mate H";
                            if (i_series) cnc_type = "Power Mate i-H";
                            break;
                        case "PM"	: 
                            cnc_type = "Power Motion";
                            if (i_series) cnc_type = "Power Motion i";
                            break;
                }

                // MT TYPE
                var mt_type_code = string.Join("", input.response.cnc_sysinfo.sysinfo.mt_type);
                var mt_type = "Unknown";
                focas_support[3] = ((char)input.response.cnc_sysinfo.sysinfo.mt_type[1]).AsAscii();

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
                    focas_support,
                    loader_control,
                    i_series,
                    compound_machining,
                    transfer_line,
                    model,
                    model_code = info_bytes[0],
                    input.response.cnc_sysinfo.sysinfo.max_axis,
                    cnc_type,
                    cnc_type_code = cnc_type_code.Trim(),
                    mt_type,
                    mt_type_code = mt_type_code.Trim(), 
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