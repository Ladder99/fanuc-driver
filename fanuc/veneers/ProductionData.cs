using System.Globalization;
using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class ProductionData : Veneer
    {
        public ProductionData(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(veneers, name, isCompound, isInternal)
        {
            
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
        {
            if (nativeInputs.All(o => o.success == true))
            {
                string executing_program = additionalInputs[0]
                    ? new string(nativeInputs[8]!.response.cnc_exeprgname.exeprg.name).AsAscii()
                    : "UNAVAILABLE";
                string executing_program_2 = additionalInputs[0]
                    ? new string(nativeInputs[9]!.response.cnc_exeprgname2.path_name).AsAscii()
                    : "UNAVAILABLE";
                int executing_sequence = additionalInputs[0]
                    ? nativeInputs[10]!.response.cnc_rdseqnum.seqnum.data
                    : -1;
                
                string modifiedCurrent = "";
                string modifiedSelected = "";

                try
                {
                    modifiedCurrent = new DateTimeOffset(new DateTime(
                            nativeInputs[1]!.response.cnc_rdprogdir3.buf.dir1.mdate.year,
                            nativeInputs[1]!.response.cnc_rdprogdir3.buf.dir1.mdate.month,
                            nativeInputs[1]!.response.cnc_rdprogdir3.buf.dir1.mdate.day,
                            nativeInputs[1]!.response.cnc_rdprogdir3.buf.dir1.mdate.hour,
                            nativeInputs[1]!.response.cnc_rdprogdir3.buf.dir1.mdate.minute, 0))
                        .ToString("o", CultureInfo.InvariantCulture);
                }
                catch
                {
                    // ignored
                }

                try
                {
                    modifiedSelected = new DateTimeOffset(new DateTime(
                            nativeInputs[2]!.response.cnc_rdprogdir3.buf.dir1.mdate.year,
                            nativeInputs[2]!.response.cnc_rdprogdir3.buf.dir1.mdate.month,
                            nativeInputs[2]!.response.cnc_rdprogdir3.buf.dir1.mdate.day,
                            nativeInputs[2]!.response.cnc_rdprogdir3.buf.dir1.mdate.hour,
                            nativeInputs[2]!.response.cnc_rdprogdir3.buf.dir1.mdate.minute, 0))
                        .ToString("o", CultureInfo.InvariantCulture);
                }
                catch
                {
                    // ignored
                }

                var currentValue = new
                {
                    program = new {
                        executing = new {
                            name = executing_program,
                            path = executing_program_2,
                            sequence = executing_sequence
                        },
                        current = new {
                            name = $"O{nativeInputs[0].response.cnc_rdprgnum.prgnum.data}",
                            number = nativeInputs[0].response.cnc_rdprgnum.prgnum.data,
                            size_b = nativeInputs[1]!.response.cnc_rdprogdir3.buf.dir1.length,
                            comment = nativeInputs[1]!.response.cnc_rdprogdir3.buf.dir1.comment,
                            modified = modifiedCurrent
                        },
                        selected = new {
                            name = $"O{nativeInputs[0].response.cnc_rdprgnum.prgnum.mdata}",
                            number = nativeInputs[0].response.cnc_rdprgnum.prgnum.mdata,
                            size_b = nativeInputs[2]!.response.cnc_rdprogdir3.buf.dir1.length,
                            comment = nativeInputs[2]!.response.cnc_rdprogdir3.buf.dir1.comment,
                            modified = modifiedSelected
                        }
                    },
                    pieces = new {
                        produced = nativeInputs[3]!.response.cnc_rdparam.param.data.ldata,
                        produced_life = nativeInputs[4]!.response.cnc_rdparam.param.data.ldata,
                        remaining = nativeInputs[5]!.response.cnc_rdparam.param.data.ldata
                    },
                    timers = new {
                        cycle_time_ms = (nativeInputs[6]!.response.cnc_rdparam.param.data.ldata * 60000) +
                                        nativeInputs[7]!.response.cnc_rdparam.param.data.ldata
                    }
                };

                await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

                if (currentValue.IsDifferentString((object)LastChangedValue))
                {
                    await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);
                }
            }
            else
            {
                await OnHandleErrorAsync(nativeInputs, additionalInputs);
            }

            return new { veneer = this };
        }
    }
}