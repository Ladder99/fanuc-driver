using System.Collections.Generic;
using System.Text;

namespace l99.driver.fanuc
{
    public class IntermediateModelGenerator
    {
        private List<string> _rootItems = new List<string>();
        private List<string> _pathItems = new List<string>();
        private List<string> _axisItems = new List<string>();
        private List<string> _spindleItems = new List<string>();
        
        private List<short> _paths = new List<short>();
        private Dictionary<short, List<string>> _axes = new Dictionary<short, List<string>>();
        private Dictionary<short, List<string>> _spindles = new Dictionary<short, List<string>>();
        
        
        public bool IsGenerated { get; private set; }

        public string Model
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("model:");
                foreach(string item in _rootItems)
                    sb.AppendLine($"  - {item}");

                sb.AppendLine($"\tpaths:");
                foreach (short path in _paths)
                {
                    sb.AppendLine($"\t\t{path}:");

                    foreach (string path_item in _pathItems)
                    {
                        sb.AppendLine($"\t\t\t{path_item}");
                    }

                    sb.AppendLine($"\t\t\taxes:");
                    foreach (string axis in _axes[path])
                    {
                        sb.AppendLine($"\t\t\t\t{axis}:");

                        foreach (string axis_item in _axisItems)
                        {
                            sb.AppendLine($"\t\t\t\t\t{axis_item}");
                        }
                    }
                    
                    sb.AppendLine($"\t\t\tspindles:");
                    foreach (string spindle in _spindles[path])
                    {
                        sb.AppendLine($"\t\t\t\t{spindle}:");

                        foreach (string spindle_item in _spindleItems)
                        {
                            sb.AppendLine($"\t\t\t\t\t{spindle_item}");
                        }
                    }
                }

                return sb.ToString();
            }
        }
        
        public void Start()
        {
            
        }
        
        public void AddRootItem(string name)
        {
            _rootItems.Add(name);
        }

        public void AddPath(short path)
        {
            _paths.Add(path);
            _axes.Add(path, new List<string>());
            _spindles.Add(path, new List<string>());
        }
        
        public void AddPathItem(string name)
        {
            _pathItems.Add(name);
        }

        public void AddAxis(short path, string name)
        {
            _axes[path].Add(name);
        }
        
        public void AddAxisItem(string name)
        {
            _axisItems.Add(name);
        }

        public void AddSpindle(short path, string name)
        {
            _spindles[path].Add(name);
        }
        
        public void AddSpindleItem(string name)
        {
            _spindleItems.Add(name);
        }

        public void Finish()
        {
            IsGenerated = true;
        }
    }
}