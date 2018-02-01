﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;

namespace SS.Form.Core
{
    public class CacheUtils
    {
        private CacheUtils() { }

        private static readonly Cache Cache;

        public static string GetCacheKey(string nameofClass, string nameofMethod, params string[] values)
        {
            var key = $"SS.Form.Core.{nameofClass}.{nameofMethod}";
            if (values == null || values.Length <= 0) return key;
            foreach (var t in values)
            {
                key += "." + t;
            }
            return key;
        }

        public static T GetCache<T>(string cacheKey) where T : class
        {
            return Get<T>(cacheKey);
        }

        public static int GetIntCache(string cacheKey)
        {
            return GetInt(cacheKey, -1);
        }

        public static DateTime GetDateTimeCache(string cacheKey)
        {
            return GetDateTime(cacheKey, DateTime.MinValue);
        }

        /// <summary>
        /// Static initializer should ensure we only have to look up the current cache
        /// instance once.
        /// </summary>
        static CacheUtils()
        {
            var context = HttpContext.Current;
            Cache = context != null ? context.Cache : HttpRuntime.Cache;
        }

        public static void ClearAll()
        {
            var cacheEnum = Cache.GetEnumerator();
            var keys = new List<string>();
            while (cacheEnum.MoveNext())
            {
                if (cacheEnum.Key != null) keys.Add(cacheEnum.Key.ToString());
            }

            foreach (var key in keys)
            {
                Cache.Remove(key);
            }
        }

        public static void RemoveByStartString(string startString)
        {
            if (!string.IsNullOrEmpty(startString))
            {
                RemoveByPattern(startString + "([w+]*)");
            }
        }

        public static void RemoveByPattern(string pattern)
        {
            var cacheEnum = Cache.GetEnumerator();
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            while (cacheEnum.MoveNext())
            {
                if (cacheEnum.Key != null && regex.IsMatch(cacheEnum.Key.ToString()))
                {
                    Cache.Remove(cacheEnum.Key.ToString());
                }
            }
        }

        /// <summary>
        /// Removes the specified key from the cache
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            Cache.Remove(key);
        }

        public static void Insert(string key, object obj)
        {
            InnerInsert(key, obj, null, Cache.NoSlidingExpiration);
        }

        public static void InsertMinutes(string key, object obj, int minutes)
        {
            InnerInsert(key, obj, null, TimeSpan.FromMinutes(minutes));
        }

        public static void InsertHours(string key, object obj, int hours)
        {
            InnerInsert(key, obj, null, TimeSpan.FromHours(hours));
        }

        public static void Insert(string key, object obj, string filePath)
        {
            InnerInsert(key, obj, filePath, Cache.NoSlidingExpiration);
        }

        public static void Insert(string key, object obj, TimeSpan timeSpan, string filePath)
        {
            InnerInsert(key, obj, filePath, timeSpan);
        }

        private static void InnerInsert(string key, object obj, string filePath, TimeSpan timeSpan)
        {
            if (!string.IsNullOrEmpty(key) && obj != null)
            {
                Cache.Insert(key, obj, string.IsNullOrEmpty(filePath) ? null : new CacheDependency(filePath), Cache.NoAbsoluteExpiration, timeSpan, CacheItemPriority.Normal, null);
            }
        }

        public static bool IsCache(string key)
        {
            return Cache.Get(key) != null;
        }

        public static object Get(string key)
        {
            return Cache.Get(key);
        }

        public static int GetInt(string key, int notFound)
        {
            var retval = Get(key);
            if (retval == null)
            {
                return notFound;
            }
            return (int)retval;
        }

        public static DateTime GetDateTime(string key, DateTime notFound)
        {
            var retval = Get(key);
            if (retval == null)
            {
                return notFound;
            }
            return (DateTime)retval;
        }

        public static T Get<T>(string key) where T : class
        {
            return Cache.Get(key) as T;
        }

        public static List<string> AllKeys
        {
            get
            {
                var keys = new List<string>();

                var cacheEnum = Cache.GetEnumerator();
                while (cacheEnum.MoveNext())
                {
                    if (cacheEnum.Key != null) keys.Add(cacheEnum.Key.ToString());
                }

                return keys;
            }
        }
    }
}
