using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class StatInfoText : Veneer
    {
        readonly string[] _autText = new string[] 
        { 
            "****(No selection)", 
            "MDI", 
            "TAPE(Series 15), DNC(Series 15i)",
            "MEMory",
            "EDIT",
            "TeacH IN"
        };
        
        readonly string[] _runText = new string[]
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
        
        readonly string[] _editText = new string[]
        {
            "****(Not editing)",
            "EDIT",
            "SeaRCH",
            "VeRiFY",
            "CONDense",
            "READ",
            "PuNCH"
        };
        
        readonly string[] _motionText = new string[]
        {
            "***",
            "MoTioN",
            "DWeLl",
            "Wait (waiting:only TT)"
        };
        
        readonly string[] _mstbText = new string[]
        {
            "***",
            "FIN"
        };
        
        readonly string[] _emergencyText = new string[]
        {
            "(Not emergency)",
            "EMerGency"
        };
        
        readonly string[] _alarmText = new string[]
        {
            "(No alarm)",
            "ALarM"
        };
        
        public StatInfoText(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
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
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            // TODO: evaluate text based on controller model
            
            if (input.success)
            {
                var current_value = new
                {
                    mode = new
                    {
                        automatic = _autText[input.response.cnc_statinfo.statinfo.aut]
                        // manual ?
                    },
                    status = new
                    {
                        run = _runText[input.response.cnc_statinfo.statinfo.run],
                        edit = _editText[input.response.cnc_statinfo.statinfo.edit],
                        motion = _motionText[input.response.cnc_statinfo.statinfo.motion],
                        mstb = _mstbText[input.response.cnc_statinfo.statinfo.mstb],
                        emergency = _emergencyText[input.response.cnc_statinfo.statinfo.emergency],
                        alarm = _alarmText[input.response.cnc_statinfo.statinfo.alarm]
                    }
                };
                
                await onDataArrivedAsync(input, current_value);
                
                if (current_value.IsDifferentString((object)lastChangedValue))
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