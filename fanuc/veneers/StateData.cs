using System.Dynamic;
using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class StateData : Veneer
{
    public StateData(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(
        veneers, name, isCompound, isInternal)
    {
    }

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        if (nativeInputs.All(o => o.success == true))
        {
            var execution = "UNAVAILABLE";

            switch ((int) nativeInputs[0].response.cnc_statinfo.statinfo.emergency)
            {
                case 0:
                    switch ((int) nativeInputs[0].response.cnc_statinfo.statinfo.run)
                    {
                        case 0:
                            execution = "READY";
                            break;
                        case 1:
                            switch ((int) nativeInputs[7]!.response.cnc_modal.modal.aux.aux_data)
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
                            if (255 - nativeInputs[4]!.response.pmc_rdpmcrng.buf.cdata[0] == 0)
                                execution = "INTERRUPTED";
                            else
                                switch ((int) nativeInputs[0].response.cnc_statinfo.statinfo.motion)
                                {
                                    case 0:
                                        switch ((int) nativeInputs[7]!.response.cnc_modal.modal.aux.aux_data)
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

                            break;
                    }

                    break;
                case 1:
                    execution = "STOPPED";
                    break;
            }

            var mode = "UNAVAILABLE";

            switch ((int) nativeInputs[0].response.cnc_statinfo.statinfo.aut)
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

            dynamic currentValue = new ExpandoObject();
            currentValue.mode = mode;
            currentValue.execution = execution;
            currentValue.aut = nativeInputs[0].response.cnc_statinfo.statinfo.aut;
            currentValue.run = nativeInputs[0].response.cnc_statinfo.statinfo.run;
            currentValue.motion = nativeInputs[0].response.cnc_statinfo.statinfo.motion;
            currentValue.mstb = nativeInputs[0].response.cnc_statinfo.statinfo.mstb;
            currentValue.emergency = nativeInputs[0].response.cnc_statinfo.statinfo.emergency;
            currentValue.alarm = nativeInputs[0].response.cnc_statinfo.statinfo.alarm;
            currentValue.timers = new ExpandoObject();
            currentValue.timers.poweron_min = nativeInputs[1]!.response.cnc_rdparam.param.data.ldata;
            currentValue.timers.operating_min = nativeInputs[2]!.response.cnc_rdparam.param.data.ldata;
            currentValue.timers.cutting_min = nativeInputs[3]!.response.cnc_rdparam.param.data.ldata;
            currentValue.@override = new ExpandoObject();
            currentValue.@override.feed = 255 - nativeInputs[4]!.response.pmc_rdpmcrng.buf.cdata[0];
            currentValue.@override.rapid = nativeInputs[5]!.response.pmc_rdpmcrng.buf.cdata[0];
            currentValue.@override.spindle = nativeInputs[6]!.response.pmc_rdpmcrng.buf.cdata[0];
            currentValue.modal = new ExpandoObject();
            currentValue.modal.m1 = nativeInputs[7]!.response.cnc_modal.modal.aux.aux_data;
            currentValue.modal.m2 = nativeInputs[8]!.response.cnc_modal.modal.aux.aux_data;
            currentValue.modal.m3 = nativeInputs[9]!.response.cnc_modal.modal.aux.aux_data;
            currentValue.modal.t = nativeInputs[10]!.response.cnc_modal.modal.aux.aux_data;
            
            /*
            var currentValue = new
            {
                mode,
                execution,
                nativeInputs[0].response.cnc_statinfo.statinfo.aut,
                nativeInputs[0].response.cnc_statinfo.statinfo.run,
                nativeInputs[0].response.cnc_statinfo.statinfo.motion,
                nativeInputs[0].response.cnc_statinfo.statinfo.mstb,
                nativeInputs[0].response.cnc_statinfo.statinfo.emergency,
                nativeInputs[0].response.cnc_statinfo.statinfo.alarm,
                timers = new
                {
                    poweron_min = nativeInputs[1]!.response.cnc_rdparam.param.data.ldata,
                    operating_min = nativeInputs[2]!.response.cnc_rdparam.param.data.ldata,
                    cutting_min = nativeInputs[3]!.response.cnc_rdparam.param.data.ldata
                },
                @override = new
                {
                    feed = 255 - nativeInputs[4]!.response.pmc_rdpmcrng.buf.cdata[0],
                    rapid = nativeInputs[5]!.response.pmc_rdpmcrng.buf.cdata[0],
                    spindle = nativeInputs[6]!.response.pmc_rdpmcrng.buf.cdata[0]
                },
                modal = new
                {
                    m1 = nativeInputs[7]!.response.cnc_modal.modal.aux.aux_data,
                    m2 = nativeInputs[8]!.response.cnc_modal.modal.aux.aux_data,
                    m3 = nativeInputs[9]!.response.cnc_modal.modal.aux.aux_data,
                    t = nativeInputs[10]!.response.cnc_modal.modal.aux.aux_data
                }
            };
            */
            
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
}