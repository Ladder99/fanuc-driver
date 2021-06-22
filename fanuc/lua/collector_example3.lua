luanet.load_assembly 'System'

script =  {}


function script:init_root(this, collector)
    print("initialize root");
    
    collector:apply("RdParaInfo","para_info");
    
end


function script:init_paths(this, collector)
    print("initialize paths");
    
end


function script:init_axis_and_spindle(this, collector)
    print("initialize axis/spindle");
    
end


function script:collect_root(this, collector)
    print("collect root");

    --para_num = collector.Platform:RdParaNum();
    --print(collector:to_json(para_num));
    
    para_info_1 = collector.Platform:RdParaInfo(0,10);
    --print(collector:to_json(para_info_1));
    collector:set_native_and_peel("para_info", para_info_1);
    
    next_no = para_info_1.response.cnc_rdparainfo.paraif.next_no;
    para_info_2 = collector.Platform:RdParaInfo(next_no,10);
    --print(collector:to_json(para_info_2));
    collector:set_native_and_peel("para_info", para_info_2);
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