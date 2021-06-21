luanet.load_assembly 'System'

script =  {}


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

    --print(collector:to_json(collector.Platform:RdProgInfo(1,12,1)));
    --print(collector:to_json(collector.Platform:RdProgInfo(2,31,2)));
    
    pi_bin = collector.Platform:RdProgInfoBinaryAsync();
    print(collector:to_json(pi_bin.Result));
    
    pi_asc = collector.Platform:RdProgInfoAsciiAsync();
    print(collector:to_json(pi_asc.Result));
    print(collector:to_string(pi_asc.Result.response.cnc_rdproginfo.proginfo.asc));
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