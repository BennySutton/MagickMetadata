using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Ben.Classes
{
    public static class ExtensionHelpers
    {
        #region filepaths
        /// <summary>
        /// get the full physical path to a file from the root virtual path
        /// </summary>
        /// <param name="rootpath">e.g. "~/images/ben.jpg"</param>
        /// <example>_path = ExtensionHelpers.PhysicalPathFromRootPath(rootpath);</example>
        /// <returns>"C:\Users\Benny Sutton\source\repos\Ben\Ben\Images\ben.jpg"</returns>
        public static string PhysicalPathFromRootPath(this string rootpath)
        {
            HttpContext _ctx = HttpContext.Current;
            // TO DO validate is a rootpath?
            return _ctx.Server.MapPath(rootpath);
        }

        /// <summary>
        /// helper for System.IO.Path.GetFileName
        /// </summary>
        /// <param name="filepath">e.g. "/images/ben.jpg"</param>
        /// <returns>"ben.jpg"</returns>
        public static string GetFileName(this string filepath)
        {
            return System.IO.Path.GetFileName(filepath);
        }

        /// <summary>
        /// create a list of unique words suitable for a keywords field
        /// </summary>
        /// <param name="text">the string to convert into keywords</param>
        public static string Keywords(this string text)
        {
            // split into lower case words with no trailing/leading punctuation and remove duplicate words
            var uniqueWords = Regex.Matches(text.ToLower(), "\\w+('(s|d|t|ve|m))?")
                .Cast<Match>().Select(x => x.Value).Distinct().ToList();

            // remove Contractions (like I'm and don't) and two letter words
            var result = from s in uniqueWords
                         where s.Length > 2 && !s.Contains("'")
                         select s;
            // fill 'illegals' list with words to drop
            string dropwords = "which,than,that,have,seem,the,with,and,all,only,not,out,into,buy,probably,for,over,from,too,not,like,who,what,where,when,how,why,here";
            List<string> filter = dropwords.Split(',').ToList();
            var filtered = result.Except(filter);

            return String.Join(" ", result).Truncate(4000);
        }

        /// <summary>
        /// limit string length
        /// </summary>
        /// <param name="value">string to assess</param>
        /// <param name="maxLength">the number of characters to limit the string to</param>
        /// <returns></returns>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        /// <summary>
        /// number of words in a string
        /// </summary>
        /// <param name="str">e.g. "the quick brown fox"</param>
        /// <returns>4</returns>
        public static int WordCount(this String str)
        {
            return str.Split(new char[] { ' ', '.', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
        #endregion

        #region Time and date
        /// <summary>
        /// how old in years
        /// </summary>
        /// <param name="dateTime">a datetime value</param>
        /// <returns>1,2,3 etc.</returns>
        public static int CalculateAge(this DateTime dateTime)
        {
            var age = DateTime.Now.Year - dateTime.Year;
            if (DateTime.Now < dateTime.AddYears(age))
                age--;
            return age;
        }
        /// <summary>
        /// DateTime.TryParse
        /// </summary>
        /// <param name="s"></param>
        /// <returns>false or new DateTime</returns>
        public static DateTime? ToDateTime(this string s)
        {
            DateTime dtr;
            var tryDtr = DateTime.TryParse(s, out dtr);
            return (tryDtr) ? dtr : new DateTime?();
        }
        /// <summary>
        /// Get a human readable representation of how long ago the datetime was 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>"one second ago" "a minute ago" "an hour ago"</returns>
        public static string ToReadableTime(this DateTime value)
        {
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - value.Ticks);
            double delta = ts.TotalSeconds;
            if (delta < 60)
            {
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
            }
            if (delta < 120)
            {
                return "a minute ago";
            }
            if (delta < 2700) // 45 * 60
            {
                return ts.Minutes + " minutes ago";
            }
            if (delta < 5400) // 90 * 60
            {
                return "an hour ago";
            }
            if (delta < 86400) // 24 * 60 * 60
            {
                return ts.Hours + " hours ago";
            }
            if (delta < 172800) // 48 * 60 * 60
            {
                return "yesterday";
            }
            if (delta < 2592000) // 30 * 24 * 60 * 60
            {
                return ts.Days + " days ago";
            }
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            var years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }

        #endregion

        #region null handling
        /// <summary>
        ///    Always return an int or 0 (if null)
        /// </summary>
        /// <param name="target">the object that should be an int</param>
        /// <example> BlogPostID = reader[IdColumn].IntOrDefault();</example>
        /// <returns></returns>
        public static int NZ_IntOrDefault(this object target)
        {
            int valueToReturn = default(int);
            string parseValue = target != null ? target.ToString() : valueToReturn.ToString();
            int.TryParse(parseValue, out valueToReturn);
            return valueToReturn;
        }

        public static string NZ(this string str)
        {
            return str ?? default(string);
        }
        /// <summary>
        /// check if double is null
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueToCheck"></param>
        /// <returns></returns>
        public static bool IsNullOrValue(this double? value, double valueToCheck)
        {
            return (value ?? valueToCheck) == valueToCheck;
        }
        #endregion

        #region Validation
        /// <summary>
        /// check if a string is properly formed and so COULD BE a valid email address see https://regexr.com/4te27
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsValidEmailAddress(this string s)
        {
            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(s);
        }

        /// <summary>
        ///  Is this an image file? This is a weak check for jpeg/gif/png file extension - but the file is on your hard disk already it should be OK! see www.regexr.com/4t9nr
        /// </summary>
        /// <param name="FileName">FIlename, Rootpath or full PhysicalPath</param>
        public static bool IsImage(this string FileName)
        {
            return Regex.Match(FileName, @".*(\.[Jj][Pp][Gg]|\.[Gg][Ii][Ff]|\.[Jj][Pp][Ee][Gg]|\.[Pp][Nn][Gg])\b").Success;
        }

        /// <summary>
        /// DateTime.TryParse into a DateTime
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsDate(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                DateTime dt;
                return (DateTime.TryParse(input, out dt));
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// is this object null?
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNull(this object source)
        {
            return source == null;
        }
        public static string IsNull(this object source, string str)
        {
            if (source == null) { return str; } else { return source.ToString(); }
        }

        /// <summary>
        /// check if string could be a valid web address https://regexr.com/4te2d
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsValidUrl(this string text)
        {
            Regex rx = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
            return rx.IsMatch(text);
        }
        /// <summary>
        /// Could the string be a number 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNumeric(this string value)
        {
            Regex regex = new Regex(@"[0-9]");
            return regex.IsMatch(value);
        }

        #endregion

        #region HTML encoding

        /// <summary>
        /// extension for HttpUtility.HtmlEncode
        /// </summary>
        /// <example>string s = ExtensionHelpers.HtmlEncode(....)</example>
        public static string HtmlEncode(this string data)
        {
            return HttpUtility.HtmlEncode(data);
        }
        /// <summary>
        /// extension for HttpUtility.HtmlEncode
        /// </summary>
        /// <example>string s = ExtensionHelpers.HtmlEncode(....)</example>
        public static string HtmlDecode(this string data)
        {
            return HttpUtility.HtmlDecode(data);
        }

        /// <summary>
        /// extension for HttpUtility.ParseQueryString
        /// </summary>
        /// <example>string s = ExtensionHelpers.ParseQueryString(....)</example>
        public static NameValueCollection ParseQueryString(this string query)
        {
            return HttpUtility.ParseQueryString(query);
        }

        /// <summary>
        /// extension for HttpUtility.UrlEncode
        /// </summary>
        /// <example>string s = ExtensionHelpers.UrlEncode(....)</example>
        public static string UrlEncode(this string url)
        {
            return HttpUtility.UrlEncode(url);
        }

        /// <summary>
        /// extension for HttpUtility.UrlDecode
        /// </summary>
        /// <example>string s = ExtensionHelpers.UrlDecode(....)</example>
        public static string UrlDecode(this string url)
        {
            return HttpUtility.UrlDecode(url);
        }

        /// <summary>
        /// extension for HttpUtility.UrlPathEncode
        /// </summary>
        /// <example>string s = ExtensionHelpers.UrlPathEncode(....)</example>
        public static string UrlPathEncode(this string url)
        {
            return HttpUtility.UrlPathEncode(url);
        }
        #endregion

        #region String handling

        /// <summary>
        /// One overload simply takes a collection of strings and returns a single string. Another overload can take a collection of any type, and a delegate that projects from a singleton of the collection to a string. There are two more overloads that allow you to specify a separator string.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>onetwothree one:two:three:  123  1:2:3:  </returns>
        public static string StringConcatenate(this IEnumerable<string> source)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in source)
                sb.Append(s);
            return sb.ToString();
        }

        /// <summary>
        /// One overload simply takes a collection of strings and returns a single string. Another overload can take a collection of any type, and a delegate that projects from a singleton of the collection to a string. There are two more overloads that allow you to specify a separator string.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>onetwothree one:two:three:  123  1:2:3:  </returns>
        public static string StringConcatenate<T>(this IEnumerable<T> source,
            Func<T, string> func)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T item in source)
                sb.Append(func(item));
            return sb.ToString();
        }

        /// <summary>
        /// One overload simply takes a collection of strings and returns a single string. Another overload can take a collection of any type, and a delegate that projects from a singleton of the collection to a string. There are two more overloads that allow you to specify a separator string.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>onetwothree one:two:three:  123  1:2:3:  </returns>
        public static string StringConcatenate(this IEnumerable<string> source, string separator)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in source)
                sb.Append(s).Append(separator);
            return sb.ToString();
        }

        /// <summary>
        /// One overload simply takes a collection of strings and returns a single string. Another overload can take a collection of any type, and a delegate that projects from a singleton of the collection to a string. There are two more overloads that allow you to specify a separator string.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>onetwothree one:two:three:  123  1:2:3:  </returns>
        public static string StringConcatenate<T>(this IEnumerable<T> source,
            Func<T, string> func, string separator)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T item in source)
                sb.Append(func(item)).Append(separator);
            return sb.ToString();
        }
        #endregion
    }
}