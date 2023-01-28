using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class SysInfo : Veneer
{
    public SysInfo(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(veneers,
        name, isCompound, isInternal)
    {
    }

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        if (nativeInputs.All(o => o.success == true))
        {
            var focasSupport = new string[4];

            // ADDITIONAL INFO
            byte[] infoBytes = BitConverter.GetBytes(nativeInputs[0].response.cnc_sysinfo.sysinfo.addinfo);

            var loaderControl = (infoBytes[0] & 1) == 1;
            var iSeries = (infoBytes[0] & 2) == 2;
            ;
            var compoundMachining = (infoBytes[0] & 4) == 4;
            ;
            var transferLine = (infoBytes[0] & 8) == 8;
            ;

            focasSupport[1] = iSeries ? "i" : "";

            var model = "Unknown";
            switch (infoBytes[1])
            {
                case 0:
                    model = "MODEL information is not supported";
                    focasSupport[2] = "";
                    break;
                case 1:
                    model = "MODEL A";
                    focasSupport[2] = "A";
                    break;
                case 2:
                    model = "MODEL B";
                    focasSupport[2] = "B";
                    break;
                case 3:
                    model = "MODEL C";
                    focasSupport[2] = "C";
                    break;
                case 4:
                    model = "MODEL D";
                    focasSupport[2] = "D";
                    break;
                case 6:
                    model = "MODEL F";
                    focasSupport[2] = "F";
                    break;
            }

            // CNC TYPE
            var cncTypeCode = string.Join("", nativeInputs[0].response.cnc_sysinfo.sysinfo.cnc_type);
            var cncType = "Unknown";
            focasSupport[0] = cncTypeCode.Trim();

            switch (cncTypeCode)
            {
                case "15":
                    cncType = "Series 15" + (iSeries ? "i" : "");
                    break;
                case "16":
                    cncType = "Series 16" + (iSeries ? "i" : "");
                    break;
                case "18":
                    cncType = "Series 18" + (iSeries ? "i" : "");
                    break;
                case "21":
                    cncType = "Series 21" + (iSeries ? "i" : "");
                    break;
                case "30":
                    cncType = "Series 30" + (iSeries ? "i" : "");
                    break;
                case "31":
                    cncType = "Series 31" + (iSeries ? "i" : "");
                    break;
                case "32":
                    cncType = "Series 32" + (iSeries ? "i" : "");
                    break;
                case "35":
                    cncType = "Series 35" + (iSeries ? "i" : "");
                    break;
                case " 0":
                    cncType = "Series 0" + (iSeries ? "i" : "");
                    break;
                case "PD":
                    cncType = "Power Mate D";
                    if (iSeries) cncType = "Power Mate i-D";
                    break;
                case "PH":
                    cncType = "Power Mate H";
                    if (iSeries) cncType = "Power Mate i-H";
                    break;
                case "PM":
                    cncType = "Power Motion";
                    if (iSeries) cncType = "Power Motion i";
                    break;
            }

            // MT TYPE
            var mtTypeCode = string.Join("", nativeInputs[0].response.cnc_sysinfo.sysinfo.mt_type);
            var mtType = "Unknown";
            focasSupport[3] = ((char) nativeInputs[0].response.cnc_sysinfo.sysinfo.mt_type[1]).AsAscii();

            switch (mtTypeCode)
            {
                case " M":
                    mtType = "Machining center";
                    break;
                case " T":
                    mtType = "Lathe";
                    break;
                case "MM":
                    mtType = "M series with 2 path control";
                    break;
                case "TT":
                    mtType = "T series with 2/3 path control";
                    break;
                case "MT":
                    mtType = "T series with compound machining function";
                    break;
                case " P":
                    mtType = "Punch press";
                    break;
                case " L":
                    mtType = "Laser";
                    break;
                case " W":
                    mtType = "Wire cut";
                    break;
            }

            // AXIS COUNT
            dynamic axes;
            if (short.TryParse(string.Join("", nativeInputs[0].response.cnc_sysinfo.sysinfo.axes), out short axisCount))
                axes = axisCount;
            else
                axes = string.Join("", nativeInputs[0].response.cnc_sysinfo.sysinfo.axes);


            var currentValue = new
            {
                focas_support = focasSupport,
                loader_control = loaderControl,
                i_series = iSeries,
                compound_machining = compoundMachining,
                transfer_line = transferLine,
                model,
                model_code = infoBytes[0],
                nativeInputs[0].response.cnc_sysinfo.sysinfo.max_axis,
                cnc_type = cncType,
                cnc_type_code = cncTypeCode.Trim(),
                mt_type = mtType,
                mt_type_code = mtTypeCode.Trim(),
                series = string.Join("", nativeInputs[0].response.cnc_sysinfo.sysinfo.series),
                version = string.Join("", nativeInputs[0].response.cnc_sysinfo.sysinfo.version),
                axes
            };

            await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

            if (currentValue.IsDifferentString((object) LastChangedValue))
                await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);
        }
        else
        {
            await OnHandleErrorAsync(nativeInputs, additionalInputs);
        }

        return new {veneer = this};
    }
}