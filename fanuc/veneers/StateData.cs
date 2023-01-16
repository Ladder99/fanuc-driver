using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class StateData : Veneer
    {
        public StateData(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success && additionalInputs.All(o => o.success == true))
            {
                var execution = "UNAVAILABLE";

                switch ((int)input.response.cnc_statinfo.statinfo.emergency)
                {
                    case 0:
                        switch ((int)input.response.cnc_statinfo.statinfo.run)
                        {
                            case 0:
                                execution = "READY";
                                break;
                            case 1:
                                switch ((int)additionalInputs[6]!.response.cnc_modal.modal.aux.aux_data)
                                {
                                    case 0:
                                        execution = "PROGRAM_STOPPED";
                                        break;
                                    case 1:
                                        execution = "OPTIONAL_STOP";
                                        break;
                                    case 2:
                                        execution = "PROGRAM_COMPLETED";
                                        break;
                                    default:
                                        execution = "STOPPED";
                                        break;
                                }

                                break;
                            case 2:
                                execution = "FEED_HOLD";
                                break;
                            case 3:
                                if (255 - additionalInputs[3]!.response.pmc_rdpmcrng.buf.cdata[0] == 0)
                                {
                                    execution = "INTERRUPTED";
                                }
                                else
                                {
                                    switch ((int)input.response.cnc_statinfo.statinfo.motion)
                                    {
                                        case 0:
                                            switch ((int)additionalInputs[6]!.response.cnc_modal.modal.aux.aux_data)
                                            {
                                                case 0:
                                                    execution = "PROGRAM_STOPPED";
                                                    break;
                                                case 1:
                                                    execution = "OPTIONAL_STOP";
                                                    break;
                                                case 2:
                                                    execution = "PROGRAM_COMPLETED";
                                                    break;
                                                //case M<prog_stop>:
                                                //    execution = "PROGRAM_STOPPED";
                                                //    break;
                                                default:
                                                    execution = "ACTIVE";
                                                    break;
                                            }
                                            break;
                                        default:
                                            execution = "ACTIVE";
                                            break;
                                    }
                                }
                                break;
                        }
                        break;
                    case 1:
                        execution = "STOPPED";
                        break;
                }
                
                var mode = "UNAVAILABLE";

                switch ((int)input.response.cnc_statinfo.statinfo.aut)
                {
                    case 0:
                        mode = "MANUAL_DATA_INPUT";
                        break;
                    case 1:
                    case 10:
                        mode = "AUTOMATIC";
                        break;
                    case 3:
                        mode = "EDIT";
                        break;
                    default:
                        mode = "MANUAL";
                        break;
                }
                
                
                var currentValue = new
                {
                    mode,
                    execution,
                    input.response.cnc_statinfo.statinfo.aut,
                    input.response.cnc_statinfo.statinfo.run,
                    input.response.cnc_statinfo.statinfo.motion,
                    input.response.cnc_statinfo.statinfo.mstb,
                    input.response.cnc_statinfo.statinfo.emergency,
                    input.response.cnc_statinfo.statinfo.alarm,
                    timers = new
                    {
                        poweron_min = additionalInputs[0]!.response.cnc_rdparam.param.data.ldata,
                        operating_min = additionalInputs[1]!.response.cnc_rdparam.param.data.ldata,
                        cutting_min = additionalInputs[2]!.response.cnc_rdparam.param.data.ldata
                    },
                    @override = new {
                        feed = 255-additionalInputs[3]!.response.pmc_rdpmcrng.buf.cdata[0],
                        rapid = additionalInputs[4]!.response.pmc_rdpmcrng.buf.cdata[0],
                        spindle = additionalInputs[5]!.response.pmc_rdpmcrng.buf.cdata[0]
                    },
                    modal = new
                    {
                        m1 = additionalInputs[6]!.response.cnc_modal.modal.aux.aux_data,
                        m2 = additionalInputs[7]!.response.cnc_modal.modal.aux.aux_data,
                        m3 = additionalInputs[8]!.response.cnc_modal.modal.aux.aux_data,
                        t = additionalInputs[9]!.response.cnc_modal.modal.aux.aux_data
                    }
                };
                
                await OnDataArrivedAsync(input, currentValue);
                
                if (currentValue.IsDifferentString((object)lastChangedValue))
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
    }
}
