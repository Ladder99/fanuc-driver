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

        public BlockTracker(Machine machine, int sweepMs = 1000, params dynamic[] additional_params) : base(machine, sweepMs, additional_params)
        {

        }

        public override async Task<dynamic?> InitializeAsync()
        {
            try
            {
                while (!_machine.VeneersApplied)
                {
                    dynamic connect = await _machine["platform"].ConnectAsync();

                    if (connect.success)
                    {
                        _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                        _machine.ApplyVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");

                        dynamic disconnect = await _machine["platform"].DisconnectAsync();
                        _machine.VeneersApplied = true;
                    }
                    else
                    {
                        await Task.Delay(_sweepMs);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[{_machine.Id}] Collector initialization failed.");
            }

            return null;
        }

        public override async Task<dynamic?> CollectAsync()
        {
            try
            {
                dynamic connect = await _machine["platform"].ConnectAsync();
                //await _machine.PeelVeneerAsync("connect", connect);

                if (connect.success)
                {
                    //dynamic info = _machine["platform"].SysInfo();
                    //await _machine.PeelVeneerAsync("sys_info", info);

                    dynamic blkcount = await _machine["platform"].RdBlkCountAsync();
                    //Console.WriteLine($"RdBlkCount({blkcount.rc})::prog_bc = {blkcount.response.cnc_rdblkcount.prog_bc}");

                    dynamic actpt = await _machine["platform"].RdActPtAsync();
                    //Console.WriteLine($"RdActPt({actpt.rc})::prog_no = {actpt.response.cnc_rdactpt.prog_no}, blk_no = {actpt.response.cnc_rdactpt.blk_no}");

                    dynamic execprog = await _machine["platform"].RdExecProgAsync(_readAheadBytes);
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
                    dynamic prgnum = _machine["platform"].RdPrgNum();
                    Console.WriteLine($"RdPrgNum({prgnum.rc})::data = {prgnum.response.cnc_rdprgnum.prgnum.data}, mdata = {prgnum.response.cnc_rdprgnum.prgnum.mdata}");
                    */

                    /*
                    dynamic prgname = _machine["platform"].ExePrgName();
                    Console.WriteLine($"ExePrgName({prgname.rc})::o_num = {prgname.response.cnc_exeprgname.exeprg.o_num}, name = {string.Join("",prgname.response.cnc_exeprgname.exeprg.name).Trim('\0')}");
                    */

                    /*
                    dynamic prgname2 = _machine["platform"].ExePrgName2();
                    Console.WriteLine($"ExePrgName2({prgname2.rc})::path_name = {string.Join("",prgname2.response.cnc_exeprgname2.path_name).Trim('\0')}");
                    */

                    /*
                    dynamic seqnum = _machine["platform"].RdSeqNum();
                    Console.WriteLine($"RdSeqNum({seqnum.rc})::seqnum = {seqnum.response.cnc_rdseqnum.seqnum.data}");
                    */

                    /*
                    dynamic execpt = _machine["platform"].RdExecPt();
                    Console.WriteLine($"RdExecPt({execpt.rc})::pact.prog_no = {execpt.response.cnc_rdexecpt.pact.prog_no}, pact.blk_no = {execpt.response.cnc_rdexecpt.pact.blk_no}");
                    Console.WriteLine($"RdExecPt({execpt.rc})::pnext.prog_no = {execpt.response.cnc_rdexecpt.pnext.prog_no}, pnext.blk_no = {execpt.response.cnc_rdexecpt.pnext.blk_no}");
                    */

                    /*
                    dynamic progline = _machine["platform"].RdProgLine(-1, 0, 128);
                    var lines = string.Join("", progline.response.cnc_rdprogline.prog_data).Trim().Split('\n');
                    Console.WriteLine($"RdProgLine({progline.rc})::line_len = {progline.response.cnc_rdprogline.line_len}, data_len = {progline.response.cnc_rdprogline.data_len}");
                    Console.WriteLine($"RdProgLine({progline.rc})::prog_data = ");
                    foreach (var line in lines)
                    {
                        Console.WriteLine($"\t{line}");
                    }
                    */

                    dynamic disconnect = await _machine["platform"].DisconnectAsync();

                    LastSuccess = connect.success;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[{_machine.Id}] Collector sweep failed.");
            }

            return null;
        }
    }
}