using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.collectors
{
    public class FanucCollector2 : FanucCollector
    {
        private enum SegmentEnum
        {
            NONE,
            BEGIN,
            ROOT,
            PATH,
            AXIS,
            SPINDLE,
            END
        }
        
        protected Dictionary<string, dynamic> propertyBag;
        protected dynamic focas_invocations = new List<dynamic>();
        protected Stopwatch sweepWatch = new Stopwatch();
        protected int sweepRemaining = 1000;
        private SegmentEnum _currentSegment = SegmentEnum.NONE;
        
        public FanucCollector2(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            sweepRemaining = sweepMs;
            propertyBag = new Dictionary<string, dynamic>();
        }
        
        protected void catch_focas_performance(dynamic focas_native_return_object)
        {
            focas_invocations.Add(new
            {
                focas_native_return_object.method,
                focas_native_return_object.invocationMs,
                focas_native_return_object.rc
            });
        }

        protected dynamic? get(string propertyBagKey)
        {
            if (propertyBag.ContainsKey(propertyBagKey))
            {
                return propertyBag[propertyBagKey];
            }
            else
            {
                return null;
            }
        }

        protected async Task<dynamic?> set(string propertyBagKey, dynamic? value)
        {
            return await _set(propertyBagKey, value, false, false);
        }
        
        protected async Task<dynamic?> set_native(string propertyBagKey, dynamic? value)
        {
            return await _set(propertyBagKey, value, true, false);
        }
        
        protected async Task<dynamic?> set_native_and_peel(string propertyBagKey, dynamic? value)
        {
            return await _set(propertyBagKey, value, true, true);
        }

        private async Task<dynamic?> _set(string propertyBagKey, dynamic? value, bool native_response = true, bool peel = true)
        {
            if (propertyBag.ContainsKey(propertyBagKey))
            {
                propertyBag[propertyBagKey] = value;
                if (native_response == true)
                    return await handleNativeResponsePropertyBagAssignment(propertyBagKey, value, peel);
            }
            else
            {
                propertyBag.Add(propertyBagKey, value);
                if (native_response == true)
                    return await handleNativeResponsePropertyBagAssignment(propertyBagKey, value, peel);
            }

            return value;
        }
        
        private async Task<dynamic?> handleNativeResponsePropertyBagAssignment(string key, dynamic value, bool peel)
        {
            catch_focas_performance(value);

            if (!peel)
                return value;

            return await _peel(key, value);
        }

        private async Task<dynamic?> _peel(string veneer_key, dynamic input, params dynamic?[] additional_inputs)
        {
            switch (_currentSegment)
            {
                case SegmentEnum.NONE:
                    break;
                
                case SegmentEnum.BEGIN:
                    return await _machine.PeelVeneerAsync(veneer_key, input, additional_inputs);
                
                case SegmentEnum.ROOT:
                    return await _machine.PeelVeneerAsync(veneer_key, input, additional_inputs);
                
                case SegmentEnum.PATH:
                    return await _machine.PeelAcrossVeneerAsync(get("current_path"),veneer_key, input, additional_inputs);
                    
                case SegmentEnum.AXIS:
                    return await _machine.PeelAcrossVeneerAsync(get("axis_split"), veneer_key, input, additional_inputs);
                
                case SegmentEnum.SPINDLE:
                    return await _machine.PeelAcrossVeneerAsync(get("spindle_split"), veneer_key, input, additional_inputs);
                
                case SegmentEnum.END:
                    return await _machine.PeelVeneerAsync(veneer_key, input, additional_inputs);
            }

            return null;
        }

        protected async Task<dynamic?> peel(string veneer_key, params dynamic[] inputs)
        {
            if (inputs.Length == 0)
            {
                return null;
            }
            else if (inputs.Length == 1)
            {
                return await _peel(veneer_key, inputs[0]);
            }
            else
            {
                return await _peel(veneer_key, inputs[0], inputs.Skip(1).Take(inputs.Length - 1).ToArray());
            }
        }

        protected async Task apply(Type veneer_type, string veneer_name, bool is_internal = false)
        {
            switch (_currentSegment)
            {
                case SegmentEnum.NONE:
                    break;
                
                case SegmentEnum.BEGIN:
                    break;
                
                case SegmentEnum.ROOT:
                    _machine.ApplyVeneer(veneer_type, veneer_name, is_internal);
                    break;
                
                case SegmentEnum.PATH:
                    _machine.ApplyVeneerAcrossSlices(veneer_type, veneer_name, is_internal);
                    break;
                    
                case SegmentEnum.AXIS:
                    _machine.ApplyVeneerAcrossSlices(get("current_path"), veneer_type, veneer_name, is_internal);
                    break;
                
                case SegmentEnum.SPINDLE:
                    break;
                
                case SegmentEnum.END:
                    break;
            }
        }
        
        public override async Task SweepAsync(int delayMs = -1)
        {
            sweepRemaining = _sweepMs - (int)sweepWatch.ElapsedMilliseconds;
            if (sweepRemaining < 0)
            {
                sweepRemaining = _sweepMs;
            }
            _logger.Trace($"[{_machine.Id}] Sweep delay: {sweepRemaining}ms");

            await base.SweepAsync(sweepRemaining);
        }
        
        public override async Task<dynamic?> InitializeAsync()
        {
            try
            {
                _currentSegment = SegmentEnum.NONE;
                
                while (!_machine.VeneersApplied)
                {
                    dynamic connect = await _platform.ConnectAsync();
                    
                    if (connect.success)
                    {
                        _currentSegment = SegmentEnum.ROOT;
                        
                        _machine.ApplyVeneer(typeof(fanuc.veneers.FocasPerf), "focas_perf", true);
                        _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                        _machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "paths");
                        
                        await InitRootAsync();
                        
                        _currentSegment = SegmentEnum.PATH;
                        
                        var paths = await _platform.GetPathAsync();

                        IEnumerable<int> path_slices = Enumerable
                            .Range(paths.response.cnc_getpath.path_no, 
                                paths.response.cnc_getpath.maxpath_no);

                        _machine.SliceVeneer(path_slices.ToArray());

                        await InitPathsAsync();
                        
                        _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdAxisname), "axis_names");
                        _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdSpindlename), "spindle_names");
                        
                        for (short current_path = paths.response.cnc_getpath.path_no;
                            current_path <= paths.response.cnc_getpath.maxpath_no;
                            current_path++)
                        {
                            _currentSegment = SegmentEnum.AXIS;
                            
                            dynamic path = await _platform.SetPathAsync(current_path);
                            
                            dynamic axes = await _platform.RdAxisNameAsync();
                            dynamic spindles = await _platform.RdSpdlNameAsync();
                            dynamic axis_spindle_slices = new List<dynamic> { };

                            var fields_axes = axes.response.cnc_rdaxisname.axisname.GetType().GetFields();
                            for (int x = 0; x <= axes.response.cnc_rdaxisname.data_num - 1; x++)
                            {
                                var axis = fields_axes[x].GetValue(axes.response.cnc_rdaxisname.axisname);
                                axis_spindle_slices.Add(AxisName(axis));
                            }
                            
                            var fields_spindles = spindles.response.cnc_rdspdlname.spdlname.GetType().GetFields();
                            for (int x = 0; x <= spindles.response.cnc_rdspdlname.data_num - 1; x++)
                            {
                                var spindle = fields_spindles[x].GetValue(spindles.response.cnc_rdspdlname.spdlname);
                                axis_spindle_slices.Add(SpindleName(spindle));
                            };

                            _machine.SliceVeneer(current_path, axis_spindle_slices.ToArray());

                            set("current_path", current_path);
                            
                            await InitAxisAndSpindleAsync();
                        }
                        
                        dynamic disconnect = await _platform.DisconnectAsync();
                        
                        _machine.VeneersApplied = true;
                        
                        _currentSegment = SegmentEnum.NONE;
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

        public virtual async Task InitRootAsync()
        {
            
        }
        
        public virtual async Task InitPathsAsync()
        {
            
        }
        
        public virtual async Task InitAxisAndSpindleAsync()
        {
            
        }
        
        public override async Task<dynamic?> CollectAsync()
        {
            try
            {
                _currentSegment = SegmentEnum.BEGIN;
                
                if(await CollectBeginAsync())
                {
                    _currentSegment = SegmentEnum.ROOT;
                    
                    await CollectRootAsync();

                    await set_native_and_peel("paths", await _platform.GetPathAsync());
                    
                    for (short current_path = get("paths").response.cnc_getpath.path_no;
                        current_path <= get("paths").response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
                        _currentSegment = SegmentEnum.PATH;
                        set("current_path", current_path);
                        
                        set("path", await _platform.SetPathAsync(current_path));
                        dynamic path_marker = PathMarker(get("path"));
                        
                        _machine.MarkVeneer(current_path, path_marker);
                        
                        await CollectForEachPathAsync(current_path, path_marker);
                        
                        await set_native_and_peel("axis_names", await _platform.RdAxisNameAsync());
                        
                        await set_native_and_peel("spindle_names", await _platform.RdSpdlNameAsync());
                        
                        var fields_axes = get("axis_names").response.cnc_rdaxisname.axisname.GetType().GetFields();

                        for (short current_axis = 1;
                            current_axis <= get("axis_names").response.cnc_rdaxisname.data_num;
                            current_axis++)
                        {
                            _currentSegment = SegmentEnum.AXIS;
                            
                            dynamic axis = fields_axes[current_axis-1].GetValue(get("axis_names").response.cnc_rdaxisname.axisname);
                            dynamic axis_name = AxisName(axis);
                            dynamic axis_marker = AxisMarker(axis);
                            set("axis_split", new[] {current_path, axis_name});
                            
                            _machine.MarkVeneer(get("axis_split"), new[] { path_marker, axis_marker });

                            await CollectForEachAxisAsync(current_axis, get("axis_split"), axis_marker);
                        }

                        var fields_spindles = get("spindle_names").response.cnc_rdspdlname.spdlname.GetType().GetFields();
                        
                        for (short current_spindle = 1;
                            current_spindle <= get("spindle_names").response.cnc_rdspdlname.data_num;
                            current_spindle++)
                        {
                            _currentSegment = SegmentEnum.SPINDLE;
                            
                            var spindle = fields_spindles[current_spindle - 1].GetValue(get("spindle_names").response.cnc_rdspdlname.spdlname);
                            dynamic spindle_name = SpindleName(spindle);
                            dynamic spindle_marker = SpindleMarker(spindle);
                            set("spindle_split", new[] {current_path, spindle_name});
                                
                            _machine.MarkVeneer(get("spindle_split"), new[] { path_marker, spindle_marker });

                            await CollectForEachSpindleAsync(current_spindle, get("spindle_split"), spindle_marker);
                        };
                    }
                }
                
                _currentSegment = SegmentEnum.END;

                await CollectEndAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[{_machine.Id}] Collector sweep failed at segment {_currentSegment}.");
            }

            return null;
        }
        
        public virtual async Task<bool> CollectBeginAsync()
        {
            sweepWatch.Restart();
            
            await set_native_and_peel("connect", await _platform.ConnectAsync());

            return get("connect").success;
        }
        
        public virtual async Task CollectRootAsync()
        {
            
        }

        public virtual async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            
        }

        public virtual async Task CollectForEachAxisAsync(short current_axis, dynamic axis_split, dynamic axis_marker)
        {
            
        }

        public virtual async Task CollectForEachSpindleAsync(short current_spindle, dynamic spindle_split, dynamic spindle_marker)
        {
            
        }

        public virtual async Task CollectEndAsync()
        {
            await set_native("disconnect", await _platform.DisconnectAsync());

            await _machine.PeelVeneerAsync("focas_perf", new
            {
                sweepMs = sweepWatch.ElapsedMilliseconds,
                focas_invocations
            });
                    
            LastSuccess = get("connect").success;
        }
    }
}