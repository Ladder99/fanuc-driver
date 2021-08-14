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
        protected List<dynamic> focasInvocations = new List<dynamic>();
        protected Stopwatch sweepWatch = new Stopwatch();
        protected int sweepRemaining = 1000;
        private SegmentEnum _currentInitSegment = SegmentEnum.NONE;
        private SegmentEnum _currentCollectSegment = SegmentEnum.NONE;

        private IntermediateModelGenerator _intermediateModel;
        
        public FanucCollector2(Machine machine, object cfg) : base(machine, cfg)
        {
            sweepRemaining = sweepMs;
            propertyBag = new Dictionary<string, dynamic>();
            _intermediateModel = new IntermediateModelGenerator();
        }
        
        protected void catchFocasPerformance(dynamic focasNativeReturnObject)
        {
            focasInvocations.Add(new
            {
                focasNativeReturnObject.method,
                focasNativeReturnObject.invocationMs,
                focasNativeReturnObject.rc
            });
        }

        public dynamic? Get(string propertyBagKey)
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

        public async Task<dynamic?> Set(string propertyBagKey, dynamic? value)
        {
            return await set(propertyBagKey, value, false, false);
        }
        
        public async Task<dynamic?> SetNative(string propertyBagKey, dynamic? value)
        {
            var a =  await set(propertyBagKey, value, true, false);
            return a;
        }
        
        public async Task<dynamic?> SetNativeAndPeel(string propertyBagKey, dynamic? value)
        {
            return await set(propertyBagKey, value, true, true);
        }

        private async Task<dynamic?> set(string propertyBagKey, dynamic? value, bool nativeResponse = true, bool peel = true)
        {
            if (propertyBag.ContainsKey(propertyBagKey))
            {
                propertyBag[propertyBagKey] = value;
                if (nativeResponse == true)
                    return await handleNativeResponsePropertyBagAssignment(propertyBagKey, value, peel);
            }
            else
            {
                propertyBag.Add(propertyBagKey, value);
                if (nativeResponse == true)
                    return await handleNativeResponsePropertyBagAssignment(propertyBagKey, value, peel);
            }

            return value;
        }
        
        private async Task<dynamic?> handleNativeResponsePropertyBagAssignment(string key, dynamic value, bool peel)
        {
            catchFocasPerformance(value);

            if (!peel)
                return value;

            return await this.peel(key, value);
        }

        private async Task<dynamic?> peel(string veneerKey, dynamic input, params dynamic?[] additionalInputs)
        {
            switch (_currentCollectSegment)
            {
                case SegmentEnum.NONE:
                    break;
                
                case SegmentEnum.BEGIN:
                    var beginObj = await machine.PeelVeneerAsync(veneerKey, input, additionalInputs);
                
                    if(!_intermediateModel.IsGenerated)
                        _intermediateModel.AddRootItem(veneerKey, beginObj);

                    return beginObj;
                
                case SegmentEnum.ROOT:
                    var rootObj = await machine.PeelVeneerAsync(veneerKey, input, additionalInputs);
                    
                    if(!_intermediateModel.IsGenerated)
                        _intermediateModel.AddRootItem(veneerKey, rootObj);

                    return rootObj;
                
                case SegmentEnum.PATH:
                    var pathObj = await machine.PeelAcrossVeneerAsync(Get("current_path"),veneerKey, input, additionalInputs);
                    
                    if(!_intermediateModel.IsGenerated)
                        _intermediateModel.AddPathItem(veneerKey, pathObj);

                    return pathObj;
                    
                case SegmentEnum.AXIS:
                    var axisObj = await machine.PeelAcrossVeneerAsync(Get("axis_split"), veneerKey, input, additionalInputs);
                    
                    if(!_intermediateModel.IsGenerated)
                        _intermediateModel.AddAxisItem(veneerKey, axisObj);

                    return axisObj;
                
                case SegmentEnum.SPINDLE:
                    var spindleObj = await machine.PeelAcrossVeneerAsync(Get("spindle_split"), veneerKey, input, additionalInputs);
                    
                    if(!_intermediateModel.IsGenerated)
                        _intermediateModel.AddSpindleItem(veneerKey, spindleObj);

                    return spindleObj;
                
                case SegmentEnum.END:
                    var endObj = await machine.PeelVeneerAsync(veneerKey, input, additionalInputs);
                
                    if(!_intermediateModel.IsGenerated)
                        _intermediateModel.AddRootItem(veneerKey, endObj);

                    return endObj;
            }

            return null;
        }

        public async Task<dynamic?> Peel(string veneerKey, params dynamic[] inputs)
        {
            if (inputs.Length == 0)
            {
                return null;
            }
            else if (inputs.Length == 1)
            {
                return await peel(veneerKey, inputs[0]);
            }
            else
            {
                return await peel(veneerKey, inputs[0], inputs.Skip(1).Take(inputs.Length - 1).ToArray());
            }
        }

        public async Task Apply(string veneerType, string veneerName, bool isCompound = false, bool isInternal = false)
        {
            Type t = Type.GetType($"l99.driver.fanuc.veneers.{veneerType}");
            await Apply(t, veneerName, isCompound, isInternal);
        }

        public async Task Apply(Type veneerType, string veneerName, bool isCompound = false, bool isInternal = false)
        {
            switch (_currentInitSegment)
            {
                case SegmentEnum.NONE:
                    break;
                
                case SegmentEnum.BEGIN:
                    break;
                
                case SegmentEnum.ROOT:
                    machine.ApplyVeneer(veneerType, veneerName, isCompound, isInternal);
                    break;
                
                case SegmentEnum.PATH:
                    machine.ApplyVeneerAcrossSlices(veneerType, veneerName, isCompound, isInternal);
                    break;
                    
                case SegmentEnum.AXIS:
                    machine.ApplyVeneerAcrossSlices(Get("current_path"), veneerType, veneerName, isCompound, isInternal);
                    break;
                
                case SegmentEnum.SPINDLE:
                    break;
                
                case SegmentEnum.END:
                    break;
            }
        }
        
        public override async Task SweepAsync(int delayMs = -1)
        {
            sweepRemaining = sweepMs - (int)sweepWatch.ElapsedMilliseconds;
            if (sweepRemaining < 0)
            {
                sweepRemaining = sweepMs;
            }
            logger.Trace($"[{machine.Id}] Sweep delay: {sweepRemaining}ms");

            await base.SweepAsync(sweepRemaining);
        }
        
        public override async Task<dynamic?> InitializeAsync()
        {
            try
            {
                _currentInitSegment = SegmentEnum.NONE;
                
                while (!machine.VeneersApplied)
                {
                    dynamic connect = await platform.ConnectAsync();
                    
                    if (connect.success)
                    {
                        _currentInitSegment = SegmentEnum.ROOT;

                        await Apply(typeof(fanuc.veneers.FocasPerf), "focas_perf", true);
                        await Apply(typeof(fanuc.veneers.Connect), "connect");
                        await Apply(typeof(fanuc.veneers.GetPath), "paths");
                        
                        await InitRootAsync();
                        
                        _currentInitSegment = SegmentEnum.PATH;
                        
                        var paths = await platform.GetPathAsync();

                        IEnumerable<int> path_slices = Enumerable
                            .Range(paths.response.cnc_getpath.path_no, 
                                paths.response.cnc_getpath.maxpath_no);

                        machine.SliceVeneer(path_slices.ToArray());

                        await InitPathsAsync();
                        
                        await Apply(typeof(fanuc.veneers.RdAxisname), "axis_names");
                        await Apply(typeof(fanuc.veneers.RdSpindlename), "spindle_names");
                        
                        _currentInitSegment = SegmentEnum.AXIS;
                        
                        for (short current_path = paths.response.cnc_getpath.path_no;
                            current_path <= paths.response.cnc_getpath.maxpath_no;
                            current_path++)
                        {
                            dynamic path = await platform.SetPathAsync(current_path);
                            
                            dynamic axes = await platform.RdAxisNameAsync();
                            dynamic spindles = await platform.RdSpdlNameAsync();
                            dynamic axis_spindle_slices = new List<dynamic> { };

                            var fields_axes = axes.response.cnc_rdaxisname.axisname.GetType().GetFields();
                            for (int x = 0; x <= axes.response.cnc_rdaxisname.data_num - 1; x++)
                            {
                                var axis = fields_axes[x].GetValue(axes.response.cnc_rdaxisname.axisname);
                                axis_spindle_slices.Add(axisName(axis));
                            }
                            
                            var fields_spindles = spindles.response.cnc_rdspdlname.spdlname.GetType().GetFields();
                            for (int x = 0; x <= spindles.response.cnc_rdspdlname.data_num - 1; x++)
                            {
                                var spindle = fields_spindles[x].GetValue(spindles.response.cnc_rdspdlname.spdlname);
                                axis_spindle_slices.Add(spindleName(spindle));
                            };

                            machine.SliceVeneer(current_path, axis_spindle_slices.ToArray());

                            await Set("current_path", current_path);
                            
                            await InitAxisAndSpindleAsync();
                        }
                        
                        await PostInitAsync();
                        
                        dynamic disconnect = await platform.DisconnectAsync();
                        
                        machine.VeneersApplied = true;
                        
                        _currentInitSegment = SegmentEnum.NONE;
                    }
                    else
                    {
                        await Task.Delay(sweepMs);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{machine.Id}] Collector initialization failed.");
            }

            return null;
        }

        public virtual async Task PostInitAsync()
        {
            
        }
        
        /// <summary>
        /// Applied Veneers:
        ///     FocasPerf as "focas_perf",
        ///     Connect as "connect",
        ///     GetPath as "paths"
        /// </summary>
        public virtual async Task InitRootAsync()
        {
            
        }

        public virtual async Task InitUserRootAsync()
        {
            
        }
        
        /// <summary>
        /// Applied Veneers:
        ///     FocasPerf as "focas_perf",
        ///     Connect as "connect",
        ///     GetPath as "paths",
        ///     RdAxisname as "axis_names",
        ///     RdSpindlename as "spindle_names"
        /// </summary>
        public virtual async Task InitPathsAsync()
        {
            
        }

        public virtual async Task InitUserPathsAsync()
        {
            
        }
        
        /// <summary>
        /// Applied Veneers:
        ///     FocasPerf as "focas_perf",
        ///     Connect as "connect",
        ///     GetPath as "paths",
        ///     RdAxisname as "axis_names",
        ///     RdSpindlename as "spindle_names"
        /// </summary>
        public virtual async Task InitAxisAndSpindleAsync()
        {
            
        }

        public virtual async Task InitUserAxisAndSpindleAsync(short current_path)
        {
            
        }
        
        public override async Task<dynamic?> CollectAsync()
        {
            try
            {
                _currentInitSegment = SegmentEnum.NONE;
                
                focasInvocations.Clear();
                
                _currentCollectSegment = SegmentEnum.BEGIN;
                
                if(!_intermediateModel.IsGenerated)
                    _intermediateModel.Start(machine);
                
                if(await CollectBeginAsync())
                {
                    _currentInitSegment = SegmentEnum.ROOT;
                    _currentCollectSegment = SegmentEnum.ROOT;
                    
                    await SetNativeAndPeel("paths", await platform.GetPathAsync());

                    await InitUserRootAsync();
                    await CollectRootAsync();

                    _currentInitSegment = SegmentEnum.PATH;
                    await InitUserPathsAsync();
                    _currentInitSegment = SegmentEnum.AXIS;
                    
                    for (short current_path = Get("paths").response.cnc_getpath.path_no;
                        current_path <= Get("paths").response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
                        _currentCollectSegment = SegmentEnum.PATH;
                        
                        if(!_intermediateModel.IsGenerated)
                            _intermediateModel.AddPath(current_path);
                        
                        await Set("current_path", current_path);

                        await InitUserAxisAndSpindleAsync(current_path);
                        
                        await Set("path", await platform.SetPathAsync(current_path));
                        dynamic path_marker = PathMarker(Get("path"));
                        
                        machine.MarkVeneer(current_path, path_marker);
                        
                        await SetNativeAndPeel("axis_names", await platform.RdAxisNameAsync());
                        
                        await SetNativeAndPeel("spindle_names", await platform.RdSpdlNameAsync());
                        
                        await CollectForEachPathAsync(current_path, path_marker);
                        
                        var fields_axes = Get("axis_names").response.cnc_rdaxisname.axisname.GetType().GetFields();

                        for (short current_axis = 1;
                            current_axis <= Get("axis_names").response.cnc_rdaxisname.data_num;
                            current_axis++)
                        {
                            _currentCollectSegment = SegmentEnum.AXIS;
                            
                            dynamic axis = fields_axes[current_axis-1].GetValue(Get("axis_names").response.cnc_rdaxisname.axisname);
                            dynamic axis_name = axisName(axis);
                            dynamic axis_marker = axisMarker(axis);
                            await Set("axis_split", new[] {current_path, axis_name});
                            
                            if(!_intermediateModel.IsGenerated)
                                _intermediateModel.AddAxis(current_path, axis_name);
                            
                            machine.MarkVeneer(Get("axis_split"), new[] { path_marker, axis_marker });

                            await CollectForEachAxisAsync(current_axis, axis_name, Get("axis_split"), axis_marker);
                        }

                        var fields_spindles = Get("spindle_names").response.cnc_rdspdlname.spdlname.GetType().GetFields();
                        
                        for (short current_spindle = 1;
                            current_spindle <= Get("spindle_names").response.cnc_rdspdlname.data_num;
                            current_spindle++)
                        {
                            _currentCollectSegment = SegmentEnum.SPINDLE;
                            
                            var spindle = fields_spindles[current_spindle - 1].GetValue(Get("spindle_names").response.cnc_rdspdlname.spdlname);
                            dynamic spindle_name = spindleName(spindle);
                            dynamic spindle_marker = spindleMarker(spindle);
                            await Set("spindle_split", new[] {current_path, spindle_name});
                                
                            if(!_intermediateModel.IsGenerated)
                                _intermediateModel.AddSpindle(current_path, spindle_name);
                            
                            machine.MarkVeneer(Get("spindle_split"), new[] { path_marker, spindle_marker });

                            await CollectForEachSpindleAsync(current_spindle, spindle_name, Get("spindle_split"), spindle_marker);
                        };
                    }
                }
                
                _currentInitSegment = SegmentEnum.NONE;
                _currentCollectSegment = SegmentEnum.END;

                await CollectEndAsync();
                
                if (!_intermediateModel.IsGenerated)
                {
                    _intermediateModel.Finish();
                    await machine.Handler.OnGenerateIntermediateModel(_intermediateModel.Model);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{machine.Id}] Collector sweep failed at segment {_currentCollectSegment}.");
            }

            return null;
        }
        
        /// <summary>
        /// Available Data:
        ///     Connect => get("connect") (after base is called)
        /// </summary>
        public virtual async Task<bool> CollectBeginAsync()
        {
            sweepWatch.Restart();
            
            await SetNativeAndPeel("connect", await platform.ConnectAsync());

            return Get("connect").success;
        }
        
        /// <summary>
        /// Available Data:
        ///     Connect => get("connect"),
        ///     GetPath => get("paths")
        /// </summary>
        public virtual async Task CollectRootAsync()
        {
            
        }

        /// <summary>
        /// Available Data:
        ///     Connect => get("connect"),
        ///     GetPath => get("paths"),
        ///     RdAxisName => get("axis_names"),
        ///     RdSpdlName => get("spindle_names")
        /// </summary>
        public virtual async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            
        }

        /// <summary>
        /// Available Data:
        ///     Connect => get("connect"),
        ///     GetPath => get("paths"),
        ///     RdAxisName => get("axis_names"),
        ///     RdSpdlName => get("spindle_names")
        /// </summary>
        public virtual async Task CollectForEachAxisAsync(short current_axis, string axis_name, dynamic axis_split, dynamic axis_marker)
        {
            
        }

        /// <summary>
        /// Available Data:
        ///     Connect => get("connect"),
        ///     GetPath => get("paths"),
        ///     RdAxisName => get("axis_names"),
        ///     RdSpdlName => get("spindle_names")
        /// </summary>
        public virtual async Task CollectForEachSpindleAsync(short current_spindle, string spindle_name, dynamic spindle_split, dynamic spindle_marker)
        {
            
        }

        /// <summary>
        /// Available Data:
        ///     Connect => get("connect"),
        ///     GetPath => get("paths"),
        ///     RdAxisName => get("axis_names"),
        ///     RdSpdlName => get("spindle_names"),
        ///     Disconnect => get("disconnect") (after base is called)
        /// </summary>
        public virtual async Task CollectEndAsync()
        {
            await SetNative("disconnect", await platform.DisconnectAsync());

            await machine.PeelVeneerAsync("focas_perf", new
            {
                sweepMs = sweepWatch.ElapsedMilliseconds, focas_invocations = focasInvocations
            });
                    
            LastSuccess = Get("connect").success;
        }
    }
}