luanet.load_assembly 'System'
--luanet.load_assembly 'fanuc'
--Focas = luanet.import_type("l99.driver.fanuc.Focas")

script =  {}

function script:init_root(table, collector)
    print("init root");
    
    collector:apply("SysInfo", "sys_info");
end

function script:init_paths(table, collector)
    print("init paths");
end

function script:init_axis_and_spindle(table, collector)
    print("init axis");
end

function script:collect_root(table, collector)
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
end


function script:collect_path(table, collector, current_path)
    print("collect path " .. current_path);
end


function script:collect_axis(table, collector, current_path, current_axis, axis_name)
    print("collect axis " .. current_path .. " " .. axis_name);
end


function script:collect_spindle(table, collector, current_path, current_spindle, spindle_name)
    print("collect spindle " .. current_path .. " " .. spindle_name);
end