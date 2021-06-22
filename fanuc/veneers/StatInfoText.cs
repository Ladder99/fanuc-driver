using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class StatInfoText : Veneer
    {
        readonly string[] _aut_text = new string[] 
        { 
            "****(No selection)", 
            "MDI", 
            "TAPE(Series 15), DNC(Series 15i)",
            "MEMory",
            "EDIT",
            "TeacH IN"
        };
        
        readonly string[] _run_text = new string[]
        {
            "STOP",
            "HOLD",
            "STaRT",
            "MSTR(jog mdi)",
            "ReSTaRt(not blinking)",
            "PRSR(program restart)",
            "NSRC(sequence number search)",
            "ReSTaRt(blinking)",
            "ReSET",
            "(Not used)",
            "(Not used)",
            "(Not used)",
            "(Not used)",
            "HPCC(during RISC operation)"
        };
        
        readonly string[] _edit_text = new string[]
        {
            "****(Not editing)",
            "EDIT",
            "SeaRCH",
            "VeRiFY",
            "CONDense",
            "READ",
            "PuNCH"
        };
        
        readonly string[] _motion_text = new string[]
        {
            "***",
            "MoTioN",
            "DWeLl",
            "Wait (waiting:only TT)"
        };
        
        readonly string[] _mstb_text = new string[]
        {
            "***",
            "FIN"
        };
        
        readonly string[] _emergency_text = new string[]
        {
            "(Not emergency)",
            "EMerGency"
        };
        
        readonly string[] _alarm_text = new string[]
        {
            "(No alarm)",
            "ALarM"
        };
        
        public StatInfoText(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            _lastChangedValue = new
            {
                mode = new
                {
                    automatic = string.Empty,
                    manual = string.Empty
                },
                status = new
                {
                    run = string.Empty,
                    edit = string.Empty,
                    motion = string.Empty,
                    mstb = string.Empty,
                    emergency = string.Empty,
                    write = string.Empty,
                    label_skip = string.Empty,
                    alarm = string.Empty,
                    warning = string.Empty,
                    battery = string.Empty
                }
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additional_inputs)
        {
            if (input.success)
            {
                var current_value = new
                {
                    mode = new
                    {
                        automatic = _aut_text[input.response.cnc_statinfo.statinfo.aut]
                    },
                    status = new
                    {
                        run = _run_text[input.response.cnc_statinfo.statinfo.run],
                        edit = _edit_text[input.response.cnc_statinfo.statinfo.edit],
                        motion = _motion_text[input.response.cnc_statinfo.statinfo.motion],
                        mstb = _mstb_text[input.response.cnc_statinfo.statinfo.mstb],
                        emergency = _emergency_text[input.response.cnc_statinfo.statinfo.emergency],
                        alarm = _alarm_text[input.response.cnc_statinfo.statinfo.alarm]
                    }
                };
                
                await onDataArrivedAsync(input, current_value);
                
                if (current_value.IsDifferentString((object)_lastChangedValue))
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