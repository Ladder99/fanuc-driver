using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class Block : Veneer
    {
        public Block(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new { data = string.Empty };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                string source = string.Join("", input.response.cnc_rdexecprog.data).Trim();
                string[] source_lines = source.Split('\n');
                string source_line = source_lines[0].Trim(char.MinValue, ' ');
                var current_value = new { data = source_line };
                
                await OnDataArrivedAsync(input, current_value);
                
                if (!current_value.Equals(lastChangedValue))
                {
                    await OnDataChangedAsync(input, current_value);
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