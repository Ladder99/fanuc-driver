using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class ProductionData : Veneer
    {
        public ProductionData(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                program = new
                {
                    running = new {
                        name = string.Empty,
                        number = -1,
                        size_b = -1,
                        comment = string.Empty,
                        modified = -1
                    },
                    main = new {
                        name = string.Empty,
                        number = -1,
                        size_b = -1,
                        comment = string.Empty,
                        modified = -1
                    }
                },
                pieces = new
                {
                    produced = -1,
                    produced_life = -1,
                    remaining = -1
                },
                timers = new
                {
                    cycle_time_ms = -1
                }
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            // remove input.success check for 16i
            if (additionalInputs.All(o => o.success == true))
            {
                bool has_prog_name_running = input.response.cnc_exeprgname.exeprg.o_num > 0;
                long modified_running = 0;
                long modified_main = 0;

                try
                {
                    modified_running = new DateTimeOffset(new DateTime(
                            additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.mdate.year,
                            additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.mdate.month,
                            additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.mdate.day,
                            additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.mdate.hour,
                            additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.mdate.minute, 0))
                        .ToUnixTimeMilliseconds();
                }
                catch
                {
                   
                }
                
                try
                {
                    modified_main = new DateTimeOffset(new DateTime(
                            additionalInputs[2].response.cnc_rdprogdir3.buf.dir1.mdate.year,
                            additionalInputs[2].response.cnc_rdprogdir3.buf.dir1.mdate.month,
                            additionalInputs[2].response.cnc_rdprogdir3.buf.dir1.mdate.day,
                            additionalInputs[2].response.cnc_rdprogdir3.buf.dir1.mdate.hour,
                            additionalInputs[2].response.cnc_rdprogdir3.buf.dir1.mdate.minute, 0))
                        .ToUnixTimeMilliseconds();
                }
                catch
                {
                   
                }
                
                var current_value = new
                {
                    program = new {
                        running = new {
                            name = has_prog_name_running ? new string(input.response.cnc_exeprgname.exeprg.name).AsAscii() : "",
                            number = additionalInputs[0].response.cnc_rdprgnum.prgnum.data,
                            size_b = additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.length,
                            comment = additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.comment,
                            modified = modified_running
                        },
                        main = new {
                            name = "",
                            number = additionalInputs[0].response.cnc_rdprgnum.prgnum.mdata,
                            size_b = additionalInputs[2].response.cnc_rdprogdir3.buf.dir1.length,
                            comment = additionalInputs[2].response.cnc_rdprogdir3.buf.dir1.comment,
                            modified = modified_main
                        }
                    },
                    pieces = new {
                        produced = additionalInputs[3].response.cnc_rdparam.param.data.ldata,
                        produced_life = additionalInputs[4].response.cnc_rdparam.param.data.ldata,
                        remaining = additionalInputs[5].response.cnc_rdparam.param.data.ldata
                    },
                    timers = new {
                        cycle_time_ms = (additionalInputs[6].response.cnc_rdparam.param.data.ldata * 60000) +
                                        additionalInputs[7].response.cnc_rdparam.param.data.ldata
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