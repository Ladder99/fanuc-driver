using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class Block : Veneer
    {
        public Block(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            _lastChangedValue = new { data = string.Empty };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additional_inputs)
        {
            if (input.success)
            {
                string source = string.Join("", input.response.cnc_rdexecprog.data).Trim();
                string[] source_lines = source.Split('\n');
                string source_line = source_lines[0].Trim(char.MinValue, ' ');
                var current_value = new { data = source_line };
                
                await onDataArrivedAsync(input, current_value);
                
                if (!current_value.Equals(_lastChangedValue))
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