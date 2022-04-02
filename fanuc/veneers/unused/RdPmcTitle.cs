using System;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class RdPmcTitle : Veneer
    {
        public RdPmcTitle(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                mtb = string.Empty,
                machine = string.Empty,
                type = string.Empty,
                prgno = string.Empty,
                prgvers = string.Empty,
                prgdraw = string.Empty,
                date = string.Empty,
                design = string.Empty,
                written = string.Empty,
                remarks = string.Empty
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                var current_value = new
                {
                    input.response.pmc_rdpmctitle.title.mtb,
                    input.response.pmc_rdpmctitle.title.machine,
                    input.response.pmc_rdpmctitle.title.type,
                    input.response.pmc_rdpmctitle.title.prgno,
                    input.response.pmc_rdpmctitle.title.prgvers,
                    input.response.pmc_rdpmctitle.title.prgdraw,
                    input.response.pmc_rdpmctitle.title.date,
                    input.response.pmc_rdpmctitle.title.design,
                    input.response.pmc_rdpmctitle.title.written,
                    input.response.pmc_rdpmctitle.title.remarks
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