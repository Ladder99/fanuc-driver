using System.Diagnostics;
using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.strategies
{
    public class FanucExtendedStrategy : FanucStrategy
    {
        private enum StrategyStateEnum
        {
            Unknown,
            Ok,
            Failed
        }
        
        private enum SegmentEnum
        {
            None,
            Begin,
            Root,
            Path,
            Axis,
            Spindle,
            End
        }

        private readonly Dictionary<string, dynamic> _propertyBag;
        private readonly List<dynamic> _focasInvocations = new();
        private int _failedInvocationCountDuringSweep;
        private readonly Stopwatch _sweepWatch = new();
        private int _sweepRemaining;
        private SegmentEnum _currentInitSegment = SegmentEnum.None;
        private SegmentEnum _currentCollectSegment = SegmentEnum.None;

        private readonly IntermediateModelGenerator _intermediateModel;

        private StrategyStateEnum _strategyState = StrategyStateEnum.Unknown;

        protected FanucExtendedStrategy(Machine machine, object cfg) : base(machine, cfg)
        {
            _sweepRemaining = SweepMs;
            _propertyBag = new Dictionary<string, dynamic>();
            _intermediateModel = new IntermediateModelGenerator();
        }

        private void CatchFocasPerformance(dynamic focasNativeReturnObject)
        {
            _focasInvocations.Add(new
            {
                focasNativeReturnObject.method,
                focasNativeReturnObject.invocationMs,
                focasNativeReturnObject.rc
            });

            if (focasNativeReturnObject.rc != 0)
            {
                _failedInvocationCountDuringSweep++;
            }
        }

        private dynamic? GetCurrentPropertyBagKey()
        {
            switch (_currentCollectSegment)
            {
                case SegmentEnum.None:
                case SegmentEnum.Begin:
                case SegmentEnum.Root:
                case SegmentEnum.End:
                    return "none";
                
                case SegmentEnum.Path:
                    return Get("current_path");
                    
                case SegmentEnum.Axis:
                    return string.Join("/", Get("axis_split"));
                
                case SegmentEnum.Spindle:
                    return string.Join("/", Get("spindle_split"));
            }

            return "none";
        }
        
        public dynamic? Get(string propertyBagKey)
        {
            if (_propertyBag.ContainsKey(propertyBagKey))
            {
                return _propertyBag[propertyBagKey];
            }
            else
            {
                return null;
            }
        }

        public dynamic? GetKeyed(string propertyBagKey)
        {
            return Get($"{propertyBagKey}+{GetCurrentPropertyBagKey()}");
        }

        private bool Has(string propertyBagKey)
        {
            return _propertyBag.ContainsKey(propertyBagKey);
        }
        
        public bool HasKeyed(string propertyBagKey)
        {
            return Has($"{propertyBagKey}+{GetCurrentPropertyBagKey()}");
        }

        private async Task<dynamic?> Set(string propertyBagKey, dynamic? value)
        {
            return await SetInternal(propertyBagKey, value, false, false);
        }
        
        public async Task<dynamic?> SetKeyed(string propertyBagKey, dynamic? value)
        {
            return await SetInternal($"{propertyBagKey}+{GetCurrentPropertyBagKey()}", value, false, false);
        }
        
        public async Task<dynamic?> SetNative(string propertyBagKey, dynamic? value)
        {
            return await SetInternal(propertyBagKey, value, true, false);
        }
        
        public async Task<dynamic?> SetNativeKeyed(string propertyBagKey, dynamic? value)
        {
            return await SetInternal($"{propertyBagKey}+{GetCurrentPropertyBagKey()}", value, true, false);
        }

        private async Task<dynamic?> SetNativeAndPeel(string propertyBagKey, dynamic? value)
        {
            return await SetInternal(propertyBagKey, value, true, true);
        }

        private async Task<dynamic?> SetInternal(string propertyBagKey, dynamic? value, bool nativeResponse = true, bool peel = true)
        {
            if (_propertyBag.ContainsKey(propertyBagKey))
            {
                _propertyBag[propertyBagKey] = value!;
                if (nativeResponse)
                    return await HandleNativeResponsePropertyBagAssignment(propertyBagKey, value, peel);
            }
            else
            {
                _propertyBag.Add(propertyBagKey, value);
                if (nativeResponse)
                    return await HandleNativeResponsePropertyBagAssignment(propertyBagKey, value, peel);
            }

            return value;
        }
        
        private async Task<dynamic?> HandleNativeResponsePropertyBagAssignment(string key, dynamic value, bool peel)
        {
            CatchFocasPerformance(value);

            if (!peel)
                return value;

            return await this.PeelInternal(key, value);
        }

        private async Task<dynamic?> PeelInternal(string veneerKey, dynamic input, params dynamic?[] additionalInputs)
        {
            switch (_currentCollectSegment)
            {
                case SegmentEnum.None:
                    break;
                
                case SegmentEnum.Begin:
                    return await Machine.PeelVeneerAsync(veneerKey, input, additionalInputs);
                
                case SegmentEnum.Root:
                    return await Machine.PeelVeneerAsync(veneerKey, input, additionalInputs);

                case SegmentEnum.Path:
                    return await Machine.PeelAcrossVeneerAsync(Get("current_path"),veneerKey, input, additionalInputs);

                case SegmentEnum.Axis:
                    return await Machine.PeelAcrossVeneerAsync(Get("axis_split"), veneerKey, input, additionalInputs);

                case SegmentEnum.Spindle:
                    return await Machine.PeelAcrossVeneerAsync(Get("spindle_split"), veneerKey, input, additionalInputs);

                case SegmentEnum.End:
                    return await Machine.PeelVeneerAsync(veneerKey, input, additionalInputs);
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
                return await PeelInternal(veneerKey, inputs[0]);
            }
            else
            {
                return await PeelInternal(veneerKey, inputs[0], inputs.Skip(1).Take(inputs.Length - 1).ToArray());
            }
        }

        public async Task Apply(string veneerType, string veneerName, bool isCompound = false, bool isInternal = false)
        {
            Type t = Type.GetType($"l99.driver.fanuc.veneers.{veneerType}")!;
            await Apply(t, veneerName, isCompound, isInternal);
        }

        public async Task Apply(Type veneerType, string veneerName, bool isCompound = false, bool isInternal = false)
        {
            switch (_currentInitSegment)
            {
                case SegmentEnum.None:
                    break;
                
                case SegmentEnum.Begin:
                    break;
                
                case SegmentEnum.Root:
                    Machine.ApplyVeneer(veneerType, veneerName, isCompound, isInternal);
                    _intermediateModel.AddRootItem(veneerName, veneerType);
                    break;
                
                case SegmentEnum.Path:
                    Machine.ApplyVeneerAcrossSlices(veneerType, veneerName, isCompound, isInternal);
                    _intermediateModel.AddPathItem(veneerName, veneerType);
                    break;
                    
                case SegmentEnum.Axis:
                    Machine.ApplyVeneerAcrossSlices(Get("current_path"), veneerType, veneerName, isCompound, isInternal);
                    _intermediateModel.AddAxisItem(veneerName, veneerType);
                    break;
                
                case SegmentEnum.Spindle:
                    Machine.ApplyVeneerAcrossSlices(Get("current_path"), veneerType, veneerName, isCompound, isInternal);
                    _intermediateModel.AddSpindleItem(veneerName, veneerType);
                    break;
                
                case SegmentEnum.End:
                    break;
            }
        }
       
        public override async Task SweepAsync(int delayMs = -1)
        {
            _sweepRemaining = SweepMs - (int)_sweepWatch.ElapsedMilliseconds;
            if (_sweepRemaining < 0)
            {
                _sweepRemaining = SweepMs;
            }
            Logger.Trace($"[{Machine.Id}] Sweep delay: {_sweepRemaining}ms");

            await base.SweepAsync(_sweepRemaining);
        }
        
        public override async Task<dynamic?> InitializeAsync()
        {
            int initMinutes = 0;
            var initStopwatch = new Stopwatch();
            initStopwatch.Start();
            
            Logger.Info($"[{Machine.Id}] Strategy initializing.");
            
            try
            {
                _currentInitSegment = SegmentEnum.None;
                
                while (!Machine.VeneersApplied)
                {
                    // connect focas
                    dynamic connect = await Platform.ConnectAsync();
                    
                    // init strategy if able to connect
                    if (connect.success)
                    {
                        // build intermediate model
                        _intermediateModel.Start(Machine);
                        
                        #region init root veneers
                        _currentInitSegment = SegmentEnum.Root;

                        await Apply(typeof(veneers.FocasPerf), "focas_perf", isInternal:true, isCompound: true);
                        await Apply(typeof(veneers.Connect), "connect", isInternal: true);
                        await Apply(typeof(veneers.GetPath), "paths", isInternal: true);
                        
                        // init root veneers in user strategy
                        await InitRootAsync();
                        #endregion
                        
                        #region init path veneers
                        _currentInitSegment = SegmentEnum.Path;
                        
                        // retrieve controller paths
                        var paths = await Platform.GetPathAsync();

                        var pathNumbers = Enumerable
                            .Range(
                                (int) paths.response.cnc_getpath.path_no,
                                (int) paths.response.cnc_getpath.maxpath_no)
                            .ToList()
                            .ConvertAll(x => (short)x);
                        
                        // following veneers will be applied over each path
                        Machine.SliceVeneer(pathNumbers.Cast<dynamic>());

                        await Apply(typeof(veneers.RdAxisname), "axis_names", isInternal: true);
                        await Apply(typeof(veneers.RdSpindlename), "spindle_names", isInternal: true);
                        
                        // init path veneers in user strategy
                        await InitPathsAsync();
                        #endregion
                        
                        #region init axis+spindle veneers
                        //_currentInitSegment = SegmentEnum.AXIS;
                        
                        // iterate paths
                        foreach(var currentPath in pathNumbers)
                        {
                            // build intermediate model
                            _intermediateModel.AddPath(currentPath);
                            // set current path
                            dynamic path = await Platform.SetPathAsync(currentPath);
                            // read axes and spindles for current path
                            dynamic axes = await Platform.RdAxisNameAsync();
                            dynamic spindles = await Platform.RdSpdlNameAsync();
                            dynamic axisAndSpindleSlices = new List<dynamic> { };

                            // axes - get fields from focas response
                            var fieldsAxes = axes.response
                                .cnc_rdaxisname.axisname.GetType().GetFields();
                            for (int x = 0; x <= axes.response.cnc_rdaxisname.data_num - 1; x++)
                            {
                                // get axis name
                                var axis = fieldsAxes[x]
                                    .GetValue(axes.response.cnc_rdaxisname.axisname);
                                
                                // build intermediate model
                                _intermediateModel.AddAxis(currentPath, AxisName(axis));
                                
                                axisAndSpindleSlices.Add(AxisName(axis));
                            }
                            
                            // spindles - get fields from focas response
                            var fieldsSpindles = spindles.response
                                .cnc_rdspdlname.spdlname.GetType().GetFields();
                            for (int x = 0; x <= spindles.response.cnc_rdspdlname.data_num - 1; x++)
                            {
                                // get spindle name
                                var spindle = fieldsSpindles[x]
                                    .GetValue(spindles.response.cnc_rdspdlname.spdlname);
                                
                                // build intermediate model
                                _intermediateModel.AddSpindle(currentPath, SpindleName(spindle));
                                
                                axisAndSpindleSlices.Add(SpindleName(spindle));
                            };

                            // following veneers will be applied over axes+spindles
                            Machine.SliceVeneer(
                                currentPath, 
                                axisAndSpindleSlices.ToArray()); 
                            
                            // store current path
                            await Set("current_path", currentPath);
                            
                            // init axis veneers in user strategy
                            _currentInitSegment = SegmentEnum.Axis;
                            await InitAxisAsync();
                            
                            // init spindle veneers in user strategy
                            _currentInitSegment = SegmentEnum.Spindle;
                            await InitSpindleAsync();
                        }
                        #endregion
                        
                        await PostInitAsync();
                        
                        // disconnect focas
                        dynamic disconnect = await Platform.DisconnectAsync();
                        
                        Machine.VeneersApplied = true;
                        
                        _currentInitSegment = SegmentEnum.None;
                        
                        // build intermediate model
                        _intermediateModel.Finish();
                        await Machine.Handler.OnGenerateIntermediateModelAsync(_intermediateModel.Model);
                        await Machine.Transport.OnGenerateIntermediateModelAsync(_intermediateModel.Model);
                        
                        Logger.Info($"[{Machine.Id}] Strategy initialized.");
                    }
                    else
                    {
                        if (initMinutes == 0 || initStopwatch.ElapsedMilliseconds > 60000)
                        {
                            Logger.Warn($"[{Machine.Id}] Strategy initialization pending ({initMinutes} min).");
                            initMinutes++;
                            initStopwatch.Restart();
                        }

                        await Task.Delay(SweepMs);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"[{Machine.Id}] Strategy initialization failed.");
            }

            initStopwatch.Stop();

            return null;
        }

        protected virtual async Task PostInitAsync()
        {
            await Task.FromResult(0);
        }
        
        /// <summary>
        /// Applied Veneers:
        ///     FocasPerf as "focas_perf",
        ///     Connect as "connect",
        ///     GetPath as "paths"
        /// </summary>
        protected virtual async Task InitRootAsync()
        {
            await Task.FromResult(0);
        }

        protected virtual async Task InitUserRootAsync()
        {
            await Task.FromResult(0);
        }
        
        /// <summary>
        /// Applied Veneers:
        ///     FocasPerf as "focas_perf",
        ///     Connect as "connect",
        ///     GetPath as "paths",
        ///     RdAxisname as "axis_names",
        ///     RdSpindlename as "spindle_names"
        /// </summary>
        protected virtual async Task InitPathsAsync()
        {
            await Task.FromResult(0);
        }

        protected virtual async Task InitUserPathsAsync()
        {
            await Task.FromResult(0);
        }
        
        /// <summary>
        /// Applied Veneers:
        ///     FocasPerf as "focas_perf",
        ///     Connect as "connect",
        ///     GetPath as "paths",
        ///     RdAxisname as "axis_names",
        ///     RdSpindlename as "spindle_names"
        /// </summary>
        protected virtual async Task InitAxisAsync()
        {
            await Task.FromResult(0);
        }

        protected virtual async Task InitSpindleAsync()
        {
            await Task.FromResult(0);
        }

        protected virtual async Task InitUserAxisAndSpindleAsync(short currentPath)
        {
            await Task.FromResult(0);
        }

        protected override async Task<dynamic?> CollectAsync()
        {
            try
            {
                _currentInitSegment = SegmentEnum.None;
                
                _focasInvocations.Clear();
                _failedInvocationCountDuringSweep = 0;
                
                _currentCollectSegment = SegmentEnum.Begin;
                
                if(await CollectBeginAsync())
                {
                    if (_strategyState == StrategyStateEnum.Unknown)
                    {
                        Logger.Info($"[{Machine.Id}] Strategy started.");
                        _strategyState = StrategyStateEnum.Ok;
                    }
                    else if (_strategyState == StrategyStateEnum.Failed)
                    {
                        Logger.Info($"[{Machine.Id}] Strategy recovered.");
                        _strategyState = StrategyStateEnum.Ok;
                    }
                    
                    _currentInitSegment = SegmentEnum.Root;
                    _currentCollectSegment = SegmentEnum.Root;
                    
                    await SetNativeAndPeel("paths", 
                        await Platform.GetPathAsync());

                    await InitUserRootAsync();
                    await CollectRootAsync();

                    _currentInitSegment = SegmentEnum.Path;
                    await InitUserPathsAsync();
                    _currentInitSegment = SegmentEnum.Axis;
                    
                    for (short currentPath = Get("paths")!.response.cnc_getpath.path_no;
                        currentPath <= Get("paths")!.response.cnc_getpath.maxpath_no;
                        currentPath++)
                    {
                        _currentCollectSegment = SegmentEnum.Path;
                        
                        await Set("current_path", currentPath);

                        await InitUserAxisAndSpindleAsync(currentPath);
                        
                        await Set("path", await Platform.SetPathAsync(currentPath));
                        dynamic pathMarker = PathMarker(Get("path")!.request.cnc_setpath.path_no);
                        dynamic pathMarkerFull = new[] {pathMarker};
                        Machine.MarkVeneer(currentPath, pathMarkerFull);

                        await SetNativeKeyed($"axis_names",
                            await Platform.RdAxisNameAsync());
                        await Peel("axis_names",
                            GetKeyed($"axis_names"));

                        var fieldsAxes = GetKeyed($"axis_names")!
                            .response.cnc_rdaxisname.axisname.GetType().GetFields();

                        short axisCount = GetKeyed($"axis_names")!
                            .response.cnc_rdaxisname.data_num;

                        string[] axisNames = new string[axisCount]; 

                        for (short i = 0; i < axisCount; i++)
                        {
                            axisNames[i] = AxisName(fieldsAxes[i]
                                .GetValue(GetKeyed($"axis_names")!
                                    .response.cnc_rdaxisname.axisname));
                        }

                        await SetNativeKeyed($"spindle_names",
                            await Platform.RdSpdlNameAsync());
                        await SetNativeAndPeel("spindle_names", 
                            GetKeyed($"spindle_names"));
                        
                        var fieldsSpindles = GetKeyed($"spindle_names")!
                            .response.cnc_rdspdlname.spdlname.GetType().GetFields();
                        
                        short spindleCount = GetKeyed($"spindle_names")!
                            .response.cnc_rdspdlname.data_num;

                        string[] spindleNames = new string[spindleCount]; 

                        for (short i = 0; i < spindleCount; i++)
                        {
                            spindleNames[i] = SpindleName(fieldsSpindles[i]
                                .GetValue(GetKeyed($"spindle_names")!
                                    .response.cnc_rdspdlname.spdlname));
                        }
                        
                        await CollectForEachPathAsync(currentPath, axisNames, spindleNames, pathMarkerFull);
                        
                        for (short currentAxis = 1; currentAxis <= axisNames.Length; currentAxis ++)
                        {
                            _currentCollectSegment = SegmentEnum.Axis;
                            dynamic axisName = axisNames[currentAxis-1];
                            //Debug.Print($"PATH:{current_path} AXIS:{axis_name}");
                            dynamic axisMarker = AxisMarker(currentAxis, axisName);
                            dynamic axisMarkerFull = new[] {pathMarker, axisMarker};
                            await Set("axis_split", new[] {currentPath.ToString(), axisName});
                            
                            Machine.MarkVeneer(Get("axis_split"), axisMarkerFull);

                            await CollectForEachAxisAsync(currentPath, currentAxis, axisName, Get("axis_split"), axisMarkerFull);
                        }

                        for (short currentSpindle = 1; currentSpindle <= spindleNames.Length; currentSpindle ++)
                        {
                            _currentCollectSegment = SegmentEnum.Spindle;
                            dynamic spindleName = spindleNames[currentSpindle-1];
                            //Debug.Print($"PATH:{current_path} SPINDLE:{spindle_name}");
                            dynamic spindleMarker = SpindleMarker(currentSpindle, spindleName);
                            dynamic spindleMarkerFull = new[] {pathMarker, spindleMarker};
                            await Set("spindle_split", new[] {currentPath.ToString(), spindleName});
                            
                            Machine.MarkVeneer(Get("spindle_split"), spindleMarkerFull);

                            await CollectForEachSpindleAsync(currentPath, currentSpindle, spindleName, Get("spindle_split"), spindleMarkerFull);
                        };
                    }
                }
                else
                {
                    if (_strategyState == StrategyStateEnum.Unknown || _strategyState == StrategyStateEnum.Ok)
                    {
                        Logger.Warn($"[{Machine.Id}] Strategy failed to connect.");
                        _strategyState = StrategyStateEnum.Failed;
                    }
                }
                
                _currentInitSegment = SegmentEnum.None;
                _currentCollectSegment = SegmentEnum.End;

                await CollectEndAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"[{Machine.Id}] Strategy sweep failed at segment {_currentCollectSegment}.");
            }

            return null;
        }
        
        /// <summary>
        /// Available Data:
        ///     Connect => get("connect") (after base is called)
        /// </summary>
        protected virtual async Task<bool> CollectBeginAsync()
        {
            _sweepWatch.Restart();
            
            await SetNativeAndPeel("connect", await Platform.ConnectAsync());

            return Get("connect")!.success;
        }
        
        /// <summary>
        /// Available Data:
        ///     Connect => get("connect"),
        ///     GetPath => get("paths")
        /// </summary>
        protected virtual async Task CollectRootAsync()
        {
            await Task.FromResult(0);
        }

        /// <summary>
        /// Available Data:
        ///     Connect => get("connect"),
        ///     GetPath => get("paths"),
        ///     RdAxisName => get("axis_names"),
        ///     RdSpdlName => get("spindle_names")
        /// </summary>
        protected virtual async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle, dynamic pathMarker)
        {
            await Task.FromResult(0);
        }

        /// <summary>
        /// Available Data:
        ///     Connect => get("connect"),
        ///     GetPath => get("paths"),
        ///     RdAxisName => get("axis_names"),
        ///     RdSpdlName => get("spindle_names")
        /// </summary>
        protected virtual async Task CollectForEachAxisAsync(short currentPath, short currentAxis, string axisName, dynamic axisSplit, dynamic axisMarker)
        {
            await Task.FromResult(0);
        }

        /// <summary>
        /// Available Data:
        ///     Connect => get("connect"),
        ///     GetPath => get("paths"),
        ///     RdAxisName => get("axis_names"),
        ///     RdSpdlName => get("spindle_names")
        /// </summary>
        protected virtual async Task CollectForEachSpindleAsync(short currentPath, short currentSpindle, string spindleName, dynamic spindleSplit, dynamic spindleMarker)
        {
            await Task.FromResult(0);
        }

        /// <summary>
        /// Available Data:
        ///     Connect => get("connect"),
        ///     GetPath => get("paths"),
        ///     RdAxisName => get("axis_names"),
        ///     RdSpdlName => get("spindle_names"),
        ///     Disconnect => get("disconnect") (after base is called)
        /// </summary>
        protected virtual async Task CollectEndAsync()
        {
            await SetNative("disconnect", await Platform.DisconnectAsync());

            await Machine.PeelVeneerAsync("focas_perf", new
            {
                sweepMs = _sweepWatch.ElapsedMilliseconds, focas_invocations = _focasInvocations
            });
                    
            //TODO: make veneer
            LastSuccess = Get("connect").success;
            IsHealthy = _failedInvocationCountDuringSweep == 0;
        }
    }
}