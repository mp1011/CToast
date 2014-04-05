using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CToast
{
    public static class IdGenerator
    {
        private static ulong nextid = 0;
        public static ulong NextId()
        {
            return ++nextid;
        }
    }

    public static class ExtensionMethods
    {
        public static Node NotNull(this Node node)
        {
            return node ?? new NullNode();
        }

        public static TValue TryGet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary.ContainsKey(key))
                return dictionary[key];
            else
                return defaultValue;
        }

        public static bool IsNullOrEmpty(this Node node)
        {
            return node == null || node.IsEmptyList;
        }

    
    }

    public static class PathHelper
    {
        private static string mRootPath;

        public static string RootPath
        {
            get
            {
                if (!String.IsNullOrEmpty(mRootPath))
                    return mRootPath;

                var dir = Assembly.GetExecutingAssembly().Location;
                while (!System.IO.Directory.Exists(dir + System.IO.Path.DirectorySeparatorChar + "lib"))
                {
                    if(dir.LastIndexOf(System.IO.Path.DirectorySeparatorChar) < 0)
                    {
                        dir = Assembly.GetExecutingAssembly().Location;
                        break;
                    }
                    dir = dir.Substring(0, dir.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
                  
                }

                mRootPath = dir;
                return dir;

            }
        }

        public static string GetLibFile(string name)
        {
            return System.IO.Path.Combine(RootPath, "lib", name);
        }

        public static string CreateOutputFolder()
        {
            return System.IO.Path.Combine(RootPath,"output",DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss"));
        }
    }

}
