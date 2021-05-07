namespace fanuc.veneers
{
    public class Block : Veneer
    {
        public Block(string name = "") : base(name)
        {
            _lastValue = string.Empty;
        }
        
        protected override dynamic Any(dynamic input)
        {
            if (input.success)
            {
                string source = string.Join("", input.response.cnc_rdexecprog.data).Trim();
                string[] source_lines = source.Split('\n');
                string source_line = source_lines[0].Trim(char.MinValue, ' ');
                var current_value = new {data = source_line};
                
                if (!current_value.Equals(_lastValue))
                {
                    this.dataChanged(input, current_value);
                }
            }
            else
            {
                onError(input);
            }

            return new { veneer = this };
        }
    }
}