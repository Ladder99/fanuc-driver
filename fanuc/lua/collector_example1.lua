luanet.load_assembly 'System'
--luanet.load_assembly 'fanuc'
--Focas = luanet.import_type("l99.driver.fanuc.Focas")

script =  {}


function script:init_root(this, collector)
    print("initialize root");
    
    collector:apply("CNCId", "cnc_id");
    collector:apply("RdParamLData", "power_on_time");
end


function script:init_paths(this, collector)
    print("initialize paths");
    
    collector:apply("SysInfo", "sys_info");
    collector:apply("StatInfo", "stat_info");
    collector:apply("Figures", "figures");
    collector:apply("GCodeBlocks", "gcode_blocks");
end


function script:init_axis_and_spindle(this, collector)
    print("initialize axis/spindle");
    
    collector:apply("RdDynamic2_1", "axis_data");
    collector:apply("RdActs2", "spindle_data");
end


function script:collect_root(this, collector)
    print("collect root");

    collector:set_native_and_peel("cnc_id", collector.Platform:CNCId());
    collector:set_native_and_peel("power_on_time", collector.Platform:RdParamDoubleWordNoAxisAsync(6750).Result);
end


function script:collect_path(this, collector, current_path)
    print("collect path " .. current_path);
    
    -- Focas call
    system_info = collector.Platform:SysInfo();

    -- print system info to console
    print(system_info);
    
    -- publish raw system info to arbitrary topic
    collector:publish("some_topic_1", system_info);
    
    -- save raw system info to cache, retrieve from cache and publish to arbitrary topic
    collector:set("system-info", system_info);
    collector:publish("some_topic_2", collector:get("system-info"));
    
    -- peel observation using l99.driver.fanuc.veneers.SysInfo veneer
    collector:set_native_and_peel("sys_info", collector:get("system-info"));
    
    -- todo: invalid argument, is it an out param issue?
    --[[
    sysinfo = Focas.ODBSYS();
    rc = Focas.cnc_sysinfo(collector.Platform.Handle, sysinfo);
    print(rc);
    print(sysinfo);
    ]]--
    
    collector:set_native_and_peel("stat_info", collector.Platform:StatInfo());
    collector:set_native_and_peel("figures", collector.Platform:GetFigure(0, 32));
    collector:peel("gcode_blocks",
        collector:set_native("blkcount", collector.Platform:RdBlkCount()),
        collector:set_native("actpt", collector.Platform:RdActPt()),
        collector:set_native("execprog", collector.Platform:RdExecProg(128)));
end


function script:collect_axis(this, collector, current_path, current_axis, axis_name)
    print("collect axis " .. current_path .. " " .. axis_name);
    
    collector:peel("axis_data",
        collector:set_native("axis_dynamic", collector.Platform:RdDynamic2(current_axis, 44, 2)), 
        collector:get("figures"), 
        current_axis - 1);
end


function script:collect_spindle(this, collector, current_path, current_spindle, spindle_name)
    print("collect spindle " .. current_path .. " " .. spindle_name);
    
    collector:set_native_and_peel("spindle_data", collector.Platform:Acts2(current_spindle));
end