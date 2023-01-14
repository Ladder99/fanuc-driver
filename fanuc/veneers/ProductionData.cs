#pragma warning disable CS8602

using System.Globalization;
using l99.driver.@base;

// ReSharper disable once CheckNamespace
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
            if (input.success && additionalInputs.All(o => o.success == true))
            {
                string modifiedCurrent = "";
                string modifiedSelected = "";

                try
                {
                    modifiedCurrent = new DateTimeOffset(new DateTime(
                            additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.mdate.year,
                            additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.mdate.month,
                            additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.mdate.day,
                            additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.mdate.hour,
                            additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.mdate.minute, 0))
                        .ToString("o", CultureInfo.InvariantCulture);
                }
                catch
                {
                    // ignored
                }

                try
                {
                    modifiedSelected = new DateTimeOffset(new DateTime(
                            additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.mdate.year,
                            additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.mdate.month,
                            additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.mdate.day,
                            additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.mdate.hour,
                            additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.mdate.minute, 0))
                        .ToString("o", CultureInfo.InvariantCulture);
                }
                catch
                {
                   
                }
                
                var currentValue = new
                {
                    program = new {
                        current = new {
                            name = $"O{input.response.cnc_rdprgnum.prgnum.data}",
                            number = input.response.cnc_rdprgnum.prgnum.data,
                            size_b = additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.length,
                            comment = additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.comment,
                            modified = modifiedCurrent
                        },
                        selected = new {
                            name = $"O{input.response.cnc_rdprgnum.prgnum.mdata}",
                            number = input.response.cnc_rdprgnum.prgnum.mdata,
                            size_b = additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.length,
                            comment = additionalInputs[1].response.cnc_rdprogdir3.buf.dir1.comment,
                            modified = modifiedSelected
                        }
                    },
                    pieces = new {
                        produced = additionalInputs[2].response.cnc_rdparam.param.data.ldata,
                        produced_life = additionalInputs[3].response.cnc_rdparam.param.data.ldata,
                        remaining = additionalInputs[4].response.cnc_rdparam.param.data.ldata
                    },
                    timers = new {
                        cycle_time_ms = (additionalInputs[5].response.cnc_rdparam.param.data.ldata * 60000) +
                                        additionalInputs[6].response.cnc_rdparam.param.data.ldata
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
                await onErrorAsync(input);
            }

            return new { veneer = this };
        }
    }
}
#pragma warning restore CS8602