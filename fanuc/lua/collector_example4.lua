luanet.load_assembly 'System'

script =  {}

--[[
user script:


* ROOT

1)

publish to fanuc/{machine_id}/lua/apply/root

collector:apply("RdParaInfo","para_info");

2)

publish to fanuc/{machine_id}/lua/collect/root

collector:set_native_and_peel("para_info", collector.Platform:RdParaInfo(0,10));


* PATH

1)

publish to fanuc/{machine_id}/lua/apply/path

collector:apply("SysInfo", "sys_info");

2)

publish to fanuc/{machine_id}/lua/collect/path

collector:set_native_and_peel("sys_info", collector.Platform:SysInfo());


* AXIS AND SPINDLE

1)

publish to fanuc/{machine_id}/lua/apply/axis_spindle

collector:apply("RdDynamic2_1", "axis_data");
collector:apply("RdActs2", "spindle_data");

2)

publish to fanuc/{machine_id}/lua/collect/axis

collector:peel("axis_data",
    collector:set_native("axis_dynamic", collector.Platform:RdDynamic2(current_axis, 44, 2)), 
    collector:get("figures"), 
    current_axis - 1);
    
3)

publish to fanuc/{machine_id}/lua/collect/spindle

collector:set_native_and_peel("spindle_data", collector.Platform:Acts2(current_spindle));

]]--

function script:init_root(this, collector)
    print("initialize root");
    
end


function script:init_paths(this, collector)
    print("initialize paths");
    
end


function script:init_axis_and_spindle(this, collector)
    print("initialize axis/spindle");
    
end


function script:collect_root(this, collector)
    print("collect root");

end


function script:collect_path(this, collector, current_path)
    print("collect path " .. current_path);
    
end


function script:collect_axis(this, collector, current_path, current_axis, axis_name)
    print("collect axis " .. current_path .. " " .. axis_name);
    
end


function script:collect_spindle(this, collector, current_path, current_spindle, spindle_name)
    print("collect spindle " .. current_path .. " " .. spindle_name);
    
end