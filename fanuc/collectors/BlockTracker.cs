using System;
using System.Threading.Tasks;
using l99.driver.@base;
using l99.driver.fanuc.gcode;

namespace l99.driver.fanuc.collectors
{
    public class BlockTracker : FanucCollector
    {
        private Blocks _blocks = new Blocks();
        private short _readAheadBytes = 128;

        public BlockTracker(Machine machine, object cfg) : base(machine, cfg)
        {

        }

        public override async Task<dynamic?> InitializeAsync()
        {
            try
            {
                while (!machine.VeneersApplied)
                {
                    dynamic connect = await machine["platform"].ConnectAsync();

                    if (connect.success)
                    {
                        machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                        machine.ApplyVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");

                        dynamic disconnect = await machine["platform"].DisconnectAsync();
                        machine.VeneersApplied = true;
                    }
                    else
                    {
                        await Task.Delay(sweepMs);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{machine.Id}] Collector initialization failed.");
            }

            return null;
        }

        public override async Task<dynamic?> CollectAsync()
        {
            try
            {
                dynamic connect = await machine["platform"].ConnectAsync();
                //await machine.PeelVeneerAsync("connect", connect);

                if (connect.success)
                {
                    //dynamic info = machine["platform"].SysInfo();
                    //await machine.PeelVeneerAsync("sys_info", info);

                    dynamic blkcount = await machine["platform"].RdBlkCountAsync();
                    //Console.WriteLine($"RdBlkCount({blkcount.rc})::prog_bc = {blkcount.response.cnc_rdblkcount.prog_bc}");

                    dynamic actpt = await machine["platform"].RdActPtAsync();
                    //Console.WriteLine($"RdActPt({actpt.rc})::prog_no = {actpt.response.cnc_rdactpt.prog_no}, blk_no = {actpt.response.cnc_rdactpt.blk_no}");

                    dynamic execprog = await machine["platform"].RdExecProgAsync(_readAheadBytes);
                    /*
                    var execlines = string.Join("", execprog.response.cnc_rdexecprog.data).Trim().Split('\n');
                    Console.WriteLine($"RdExecProg({execprog.rc})::length = {execprog.response.cnc_rdexecprog.length}, blknum = {execprog.response.cnc_rdexecprog.blknum}");
                    Console.WriteLine($"RdExecProg({execprog.rc})::data = ");
                    foreach (var line in execlines)
                    {
                        Console.WriteLine($"\t{line}");
                    }
                    */

                    _blocks.Add(blkcount.response.cnc_rdblkcount.prog_bc, actpt.response.cnc_rdactpt.blk_no,
                        execprog.response.cnc_rdexecprog.data);
                    Console.WriteLine(_blocks.ToString(showMissedBlocks: true));

                    Console.WriteLine();

                    /*
                    dynamic prgnum = machine["platform"].RdPrgNum();
                    Console.WriteLine($"RdPrgNum({prgnum.rc})::data = {prgnum.response.cnc_rdprgnum.prgnum.data}, mdata = {prgnum.response.cnc_rdprgnum.prgnum.mdata}");
                    */

                    /*
                    dynamic prgname = machine["platform"].ExePrgName();
                    Console.WriteLine($"ExePrgName({prgname.rc})::o_num = {prgname.response.cnc_exeprgname.exeprg.o_num}, name = {string.Join("",prgname.response.cnc_exeprgname.exeprg.name).Trim('\0')}");
                    */

                    /*
                    dynamic prgname2 = machine["platform"].ExePrgName2();
                    Console.WriteLine($"ExePrgName2({prgname2.rc})::path_name = {string.Join("",prgname2.response.cnc_exeprgname2.path_name).Trim('\0')}");
                    */

                    /*
                    dynamic seqnum = machine["platform"].RdSeqNum();
                    Console.WriteLine($"RdSeqNum({seqnum.rc})::seqnum = {seqnum.response.cnc_rdseqnum.seqnum.data}");
                    */

                    /*
                    dynamic execpt = machine["platform"].RdExecPt();
                    Console.WriteLine($"RdExecPt({execpt.rc})::pact.prog_no = {execpt.response.cnc_rdexecpt.pact.prog_no}, pact.blk_no = {execpt.response.cnc_rdexecpt.pact.blk_no}");
                    Console.WriteLine($"RdExecPt({execpt.rc})::pnext.prog_no = {execpt.response.cnc_rdexecpt.pnext.prog_no}, pnext.blk_no = {execpt.response.cnc_rdexecpt.pnext.blk_no}");
                    */

                    /*
                    dynamic progline = machine["platform"].RdProgLine(-1, 0, 128);
                    var lines = string.Join("", progline.response.cnc_rdprogline.prog_data).Trim().Split('\n');
                    Console.WriteLine($"RdProgLine({progline.rc})::line_len = {progline.response.cnc_rdprogline.line_len}, data_len = {progline.response.cnc_rdprogline.data_len}");
                    Console.WriteLine($"RdProgLine({progline.rc})::prog_data = ");
                    foreach (var line in lines)
                    {
                        Console.WriteLine($"\t{line}");
                    }
                    */

                    dynamic disconnect = await machine["platform"].DisconnectAsync();

                    LastSuccess = connect.success;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{machine.Id}] Collector sweep failed.");
            }

            return null;
        }
    }
}