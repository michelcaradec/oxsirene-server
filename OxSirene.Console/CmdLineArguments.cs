using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace OxSirene.Console
{
    internal class CmdLineArguments : Dictionary<string, IList<string>>
    {
        private void AddKeyValue(ref string key, ref List<string> values)
        {
            if (key == null && values.Count == 0)
            {
                return;
            }
            if (key == null)
            {
                // Value with no key
                throw new ArgumentException();
            }

            IList<string> existing;
            if (TryGetValue(key, out existing))
            {
                foreach (var value in values)
                {
                    existing.Add(value);
                }
            }
            else
            {
                Add(key, new List<string>(values));
            }

            key = null;
            values.Clear();
        }

        public CmdLineArguments(string[] args, string prefix = "--")
        {
            string key = null;
            var values = new List<string>();

            foreach (string arg in args)
            {
                if (arg.StartsWith(prefix))
                {
                    // Add previous key-value pair.
                    AddKeyValue(ref key, ref values);
                    key = arg.Substring(prefix.Length);
                }
                else
                {
                    values.Add(arg);
                }
            }

            // Add last key-value pair.
            AddKeyValue(ref key, ref values);
        }

        [DebuggerStepThrough]
        public string GetValueOrDefault(string key)
        {
            var values = GetValuesOrDefault(key);
            return values?.FirstOrDefault();
        }

        [DebuggerStepThrough]
        public IList<string> GetValuesOrDefault(string key)
        {
            IList<string> values;
            TryGetValue(key, out values);
            return values;
        }        
    }
}
