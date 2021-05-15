namespace fanuc
{
    public partial class Platform
    {
        public dynamic RdExecProg(short length = 1024)
        {
            //length = 96;

            char[] data = new char[length];
            short blknum = 0;
            ushort length_out = (ushort) data.Length;

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdexecprog(_handle, ref length_out, out blknum, (object) data);
            });

            //string source = string.Join("", data).Trim();
            //string[] source_lines = source.Split('\n');

            /*
            int lc = 0;
            var t = DateTime.Now;
            foreach (var s in source_lines)
            {
                Console.WriteLine(t + " : " + lc + " : " + s);
                lc++;
            }
            */

            //Console.WriteLine("----------------------------");

            return new
            {
                method = "cnc_rdexecprog",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/program/cnc_rdexecprog",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdexecprog = new {length}},
                response = new {cnc_rdexecprog = new {length = length_out, blknum, data}}
            };
        }
    }
}