luanet.load_assembly 'System'

script =  {}

function script:init_root(this, collector)
    collector:log("initialize root",5);
    
    collector:apply("RdParamLData", "v1");
    collector:apply("RdParamLData", "v2");
end


function script:init_paths(this, collector)
    collector:log("initialize paths",5);
    
end


function script:init_axis_and_spindle(this, collector)
    collector:log("initialize axis/spindle",5);
    
end


function script:collect_root(this, collector)
    collector:log("collect root",5);
    
    print("--- p1 ---");
    p1 = collector.Platform:RdParam(6750, 0, 6+2*1, 1);
    print(collector:to_json(p1));
    
    print("--- p2_1 ---");
    p2_1 = collector:set("p2", collector.Platform:RdParam(6750, 0, 6+2*1, 1));
    print(collector:to_json(p2_1));
    
    print("--- p2_2 ---");
    p2_2 = collector:get("p2");
    print(collector:to_json(p2_2));
    
    print("--- p3_1 ---");
    p3_1 = collector:set_native("p3", collector.Platform:RdParam(6750, 0, 6+2*1, 1));
    print(collector:to_json(p3_1));
    
    print("--- p3_2 ---");
    p3_2 = collector:get("p3");
    print(collector:to_json(p3_2));
    
    print("--- p4 ---");
    p4 = collector:set_native_and_peel("v1", collector.Platform:RdParam(6750, 0, 6+2*1, 1));
    print(p4);
    --print(collector:to_json(p4)); -- TODO: self referencing loop
    
    print("--- p5 ---");
    p5 = collector:peel("v2", collector:set_native("p3", collector.Platform:RdParam(6750, 0, 6+2*1, 1)));
    print(p5);
    --print(collector:to_json(p5)); -- TODO: self referencing loop
end


function script:collect_path(this, collector, current_path)
    collector:log("collect path " .. current_path,5);
    
end


function script:collect_axis(this, collector, current_path, current_axis, axis_name)
    collector:log("collect axis " .. current_path .. " " .. axis_name,5);
    
end


function script:collect_spindle(this, collector, current_path, current_spindle, spindle_name)
    collector:log("collect spindle " .. current_path .. " " .. spindle_name,5);
    
end