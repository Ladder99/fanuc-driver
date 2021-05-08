using System;
using System.Collections.Generic;
using System.Text;

namespace fanuc
{
    public class Platform
    {
        private Machine _machine;
        
        private ushort _handle;

        public Platform(Machine machine)
        {
            _machine = machine;
        }

        public void StartupProcess(short level = 0, string file = "focas2.log")
        {
            Focas1.cnc_startupprocess(level, file);

        }

        public void ExitProcess()
        {
            Focas1.cnc_exitprocess();
        }
        
        public dynamic Connect()
        {
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_allclibhndl3(_machine.IPAddress, _machine.Port, _machine.ConnectionTimeout, out _handle);

            return new
            {
                method = "cnc_allclibhndl3",
                doc = "https://www.inventcom.net/fanuc-focas-library/handle/cnc_allclibhndl3",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_allclibhndl3 = new { ipaddr = _machine.IPAddress, port = _machine.Port, timeout = _machine.ConnectionTimeout } },
                response = new { cnc_allclibhndl3 = new { FlibHndl = _handle } }
            };
        }

        public dynamic Disconnect()
        {
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_freelibhndl(_handle);

            return new
            {
                method = "cnc_freelibhndl",
                doc = "https://www.inventcom.net/fanuc-focas-library/handle/cnc_freelibhndl",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_freelibhndl = new { } },
                response = new { cnc_freelibhndl = new { } }
            };
        }

        public dynamic SysInfo()
        {
            Focas1.ODBSYS sysinfo = new Focas1.ODBSYS();
            short rc = Focas1.cnc_sysinfo(_handle, sysinfo);

            return new
            {
                method = "cnc_sysinfo",
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_sysinfo",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_sysinfo = new { } },
                response = new { cnc_sysinfo = new { sysinfo } }
            };
        }

        public dynamic GetPath(short path_no = 0)
        {
            short maxpath_no = 0, path_no_out = path_no;
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_getpath(_handle, out path_no_out, out maxpath_no);

            return new
            {
                method = "cnc_getpath",
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_getpath",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_getpath = new { path_no } },
                response = new { cnc_getpath = new { path_no = path_no_out, maxpath_no } }
            };
        }

        public dynamic SetPath(short path_no)
        {
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_setpath(_handle, path_no);

            return new
            {
                method = "cnc_setpath",
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_setpath",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_setpath = new { path_no } },
                response = new { cnc_setpath = new {  } }
            };
        }

        public dynamic StatInfo()
        {
            Focas1.ODBST statinfo = new Focas1.ODBST();
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_statinfo(_handle, statinfo);

            return new
            {
                method = "cnc_statinfo",
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_statinfo",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_statinfo = new { } },
                response = new { cnc_statinfo = new { statinfo } }
            };
        }

        public dynamic Modal(short type = 0, short block = 0, int ODBMDL_type = 1)
        {
            dynamic modal = new object();

            switch(ODBMDL_type)
            {
                case 1:
                    modal = new Focas1.ODBMDL_1();
                    break;
                case 2:
                    modal = new Focas1.ODBMDL_2();
                    break;
                case 3:
                    modal = new Focas1.ODBMDL_3();
                    break;
                case 4:
                    modal = new Focas1.ODBMDL_4();
                    break;
                case 5:
                    modal = new Focas1.ODBMDL_5();
                    break;
            }

           Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_modal(_handle, type, block, modal);

            return new
            {
                method = "cnc_modal",
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_modal",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_modal = new { type, block, ODBMDL_type } },
                response = new { cnc_modal = new { modal, modal_type = modal.GetType().Name } }
            };
        }

        public dynamic RdExecProg(short length = 1024)
        {
            //length = 96;
            
            char[] data = new char[length]; short blknum = 0; ushort length_out = (ushort)data.Length;
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_rdexecprog(_handle, ref length_out, out blknum, (object)data);

            string source = string.Join("", data).Trim();
            string[] source_lines = source.Split('\n');

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
                doc = "https://www.inventcom.net/fanuc-focas-library/program/cnc_rdexecprog",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_rdexecprog = new  { length } },
                response = new { cnc_rdexecprog = new { length = length_out, blknum, data } }
            };
        }

        public dynamic RdAlmMsg(short type = 0, short num = 10)
        {
            short num_out = num;
            Focas1.ODBALMMSG almmsg = new Focas1.ODBALMMSG();
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_rdalmmsg(_handle, type, ref num_out, almmsg);
            
            return new
            {
                method = "cnc_rdalmmsg",
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_rdalmmsg",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_rdalmmsg = new { type, num } },
                response = new { cnc_rdalmmsg = new { num = num_out, almmsg } }
            };
        }

        public dynamic RdAlmMsgAll(short count = 10, short maxType = 20)
        {
            var alms = new Dictionary<short, dynamic>();

            for(short type = 0; type <= maxType; type++)
            {
                short countRead = 10;
                alms.Add(type, RdAlmMsg(type, countRead));
            }

            return new
            {
                method = "cnc_rdalmmsg_ALL",
                request = new { cnc_rdalmmsg_ALL = new { minType = 0, maxType, count } },
                response = new { cnc_rdalmmsg_ALL = alms }
            };
        }

        public dynamic RdAlmMsg2(short type = 0, short num = 10)
        {
            short num_out = num;
            Focas1.ODBALMMSG2 almmsg = new Focas1.ODBALMMSG2();
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_rdalmmsg2(_handle, type, ref num_out, almmsg);

            return new
            {
                method = "cnc_rdalmmsg2",
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_rdalmmsg2",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_rdalmmsg2 = new { type, num } },
                response = new { cnc_rdalmmsg2 = new { num = num_out, almmsg } }
            };
        }

        public dynamic RdAlmMsg2All(short count = 10, short maxType = 20)
        {
            var alms = new Dictionary<short, dynamic>();

            for (short type = 0; type <= maxType; type++)
            {
                short countRead = 10;
                alms.Add(type, RdAlmMsg2(type, countRead));
            }

            return new
            {
                method = "cnc_rdalmmsg2_ALL",
                request = new { cnc_rdalmmsg2_ALL = new { minType = 0, maxType, count } },
                response = new { cnc_rdalmmsg2_ALL = alms }
            };
        }

        public dynamic RdOpMsg(short type = 0, short length = 262)
        {
            Focas1.OPMSG opmsg = new Focas1.OPMSG();
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_rdopmsg(_handle, type, length, opmsg);

            return new
            {
                method = "cnc_rdopmsg",
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_rdopmsg",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_rdopmsg = new { type, length } },
                response = new { cnc_rdopmsg = new { opmsg } }
            };
        }

        public dynamic RdAxisName(short data_num = 8)
        {
            short data_num_out = data_num;
            Focas1.ODBAXISNAME axisname = new Focas1.ODBAXISNAME();
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_rdaxisname(_handle, ref data_num_out, axisname);

            return new
            {
                method = "cnc_rdaxisname",
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_rdaxisname",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_rdaxisname = new { data_num } },
                response = new { cnc_rdaxisname = new { data_num = data_num_out, axisname } }
            };
        }

        public dynamic RdSvMeter(short data_num = 8)
        {
            short data_num_out = data_num;
            Focas1.ODBSVLOAD loadmeter = new Focas1.ODBSVLOAD();
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_rdsvmeter(_handle, ref data_num_out, loadmeter);
            
            // each path
            // loadmeter.svloadX.data / Math.Pow(10, loadmeter.svloadX.dec)
            
            return new
            {
                method = "cnc_rdsvmeter",
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_rdsvmeter",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_rdsvmeter = new { data_num } },
                response = new { cnc_rdsvmeter = new { data_num = data_num_out, loadmeter } }
            };
        }

        public dynamic RdSpdlName(short data_num = 4)
        {
            short data_num_out = data_num;
            Focas1.ODBSPDLNAME spdlname = new Focas1.ODBSPDLNAME();
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_rdspdlname(_handle, ref data_num_out, spdlname);

            return new
            {
                method = "cnc_rdspdlname",
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_rdspdlname",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_rdspdlname = new { data_num } },
                response = new { cnc_rdspdlname = new { data_num = data_num_out, spdlname } }
            };
        }

        public dynamic RdSpMeter(short type = 0, short data_num = 4)
        {
            short data_num_out = data_num;
            Focas1.ODBSPLOAD loadmeter = new Focas1.ODBSPLOAD();
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_rdspmeter(_handle, type, ref data_num_out, loadmeter);

            return new
            {
                method = "cnc_rdspmeter",
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_rdspmeter",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_rdspmeter = new { type, data_num } },
                response = new { cnc_rdspmeter = new { data_num = data_num_out, loadmeter } }
            };
        }

        
        
        public dynamic RdOpMode()
        {
            short mode ; // array?
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_rdopmode(_handle, out mode);

            return new
            {
                method = "cnc_rdopmode",
                doc = "https://www.inventcom.net/fanuc-focas-library/motor/cnc_rdopmode",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_rdopmode = new { } },
                response = new { cnc_rdopmode = new { mode } }
            };
        }

        public dynamic Acts()
        {
            Focas1.ODBACT actualfeed = new Focas1.ODBACT();
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_acts(_handle, actualfeed);

            return new
            {
                method = "cnc_acts",
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_acts",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_acts = new {  } },
                response = new { cnc_acts = new { actualfeed } }
            };
        }

        public dynamic Acts2(short sp_no = -1)
        {
            Focas1.ODBACT2 actualspindle = new Focas1.ODBACT2();
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_acts2(_handle, sp_no, actualspindle);

            return new
            {
                method = "cnc_acts2",
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_acts2",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_acts2 = new { sp_no } },
                response = new { cnc_acts2 = new { actualspindle } }
            };
        }

        public dynamic RdPmcRng(short adr_type, short data_type, ushort s_number, ushort e_number, ushort length, int IODBPMC_type)
        {
            dynamic buf = new object();

            switch (IODBPMC_type)
            {
                case 0:
                    buf = new Focas1.IODBPMC0();
                    break;
                case 1:
                    buf = new Focas1.IODBPMC1();
                    break;
                case 2:
                    buf = new Focas1.IODBPMC2();
                    break;
            }
            
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.pmc_rdpmcrng(_handle, adr_type, data_type, s_number, e_number, length, buf);

            return new
            {
                method = "pmc_rdpmcrng",
                doc = "https://www.inventcom.net/fanuc-focas-library/pmc/pmc_rdpmcrng",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { pmc_rdpmcrng = new { adr_type, data_type, s_number, e_number, length, IODBPMC_type } },
                response = new { pmc_rdpmcrng = new { buf, IODBPMC_type = buf.GetType().Name } }
            };
        }

        public dynamic RdMacro(short number = 1, short length = 10)
        {
            Focas1.ODBM macro = new Focas1.ODBM();
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_rdmacro(_handle, number, length, macro);

            return new
            {
                method = "cnd_rdmacro",
                doc = "https://www.inventcom.net/fanuc-focas-library/ncdata/cnc_rdmacro",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnd_rdmacro = new { number, length } },
                response = new { cnd_rdmacro = new { macro } }
            };
        }

        public dynamic RdParam(short number, short axis, short length, int IODBPSD_type)
        {
            dynamic param = new object();

            switch (IODBPSD_type)
            {
                case 1:
                    param = new Focas1.IODBPSD_1();
                    break;
                case 2:
                    param = new Focas1.IODBPSD_2();
                    break;
                case 3:
                    param = new Focas1.IODBPSD_3();
                    break;
                case 4:
                    param = new Focas1.IODBPSD_4();
                    break;
            }

            Focas1.ODBALMMSG almmsg = new Focas1.ODBALMMSG();
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_rdparam(_handle, number, axis, length, param);

            return new
            {
                method = "cnc_rdparam",
                doc = "https://www.inventcom.net/fanuc-focas-library/ncdata/cnc_rdparam",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_rdparam = new { number, axis, length, IODBPSD_type } },
                response = new { cnc_rdparam = new { param, IODBPSD_type = param.GetType().Name } }
            };
        }
        
        public dynamic RdDynamic2(short axis = 1, short length = 44, int ODBDY2_type = 2)
        {
            dynamic rddynamic = new object();

            switch (ODBDY2_type)
            {
                case 1:
                    rddynamic = new Focas1.ODBDY2_1();
                    break;
                case 2:
                    rddynamic = new Focas1.ODBDY2_2();
                    break;
            }

            //length = (short) Marshal.SizeOf(rddynamic);
            Focas1.focas_ret rc = (Focas1.focas_ret)Focas1.cnc_rddynamic2(_handle, axis, length , rddynamic);

            return new
            {
                method = "cnc_rddynamic2",
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_rddynamic2",
                success = rc == Focas1.EW_OK,
                rc,
                request = new { cnc_rddynamic2 = new { axis, length } },
                response = new { cnc_rddynamic2 = new { rddynamic } }
            };
        }
    }
}