using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

using Sprache;
namespace Versatile
{

    /// <summary>
    /// A hybrid implementation of SemVer that supports semantic versioning as described at http://semver.org while not strictly enforcing it to 
    /// allow older 4-digit versioning schemes to continue working.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(NuGetv2TypeConverter))]
    public sealed partial class NuGetv2 : IComparable, IComparable<NuGetv2>, IEquatable<NuGetv2>
    {
        private const RegexOptions _flags = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
        private static readonly Regex _NuGetRegex = new Regex(@"^(?<Version>\d+(\s*\.\s*\d+){0,3})(?<Release>-[a-z][0-9a-z-]*)?$", _flags);
        private static readonly Regex _strictNuGetRegex = new Regex(@"^(?<Version>\d+(\.\d+){2})(?<Release>-[a-z][0-9a-z-]*)?$", _flags);
        private readonly string _originalString;
        private string _normalizedVersionString;

        public NuGetv2(string version)
            : this(Parse(version))
        {
            // The constructor normalizes the version string so that it we do not need to normalize it every time we need to operate on it. 
            // The original string represents the original form in which the version is represented to be used when printing.
            _originalString = version;
        }

        public NuGetv2(int major, int minor, int build, int revision)
            : this(new Version(major, minor, build, revision))
        {
        }

        public NuGetv2(int major, int minor, int build, string specialVersion)
            : this(new Version(major, minor, build), specialVersion)
        {
        }

        public NuGetv2(Version version)
            : this(version, String.Empty)
        {
        }

        public NuGetv2(Version version, string specialVersion)
            : this(version, specialVersion, null)
        {
        }

        private NuGetv2(Version version, string specialVersion, string originalString)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            Version = NormalizeVersionValue(version);
            SpecialVersion = specialVersion ?? String.Empty;
            _originalString = String.IsNullOrEmpty(originalString) ? version.ToString() + (!String.IsNullOrEmpty(specialVersion) ? '-' + specialVersion : null) : originalString;
        }

        internal NuGetv2(NuGetv2 semVer)
        {
            _originalString = semVer.ToString();
            Version = semVer.Version;
            SpecialVersion = semVer.SpecialVersion;
        }

        /// <summary>
        /// Gets the normalized version portion.
        /// </summary>
        public Version Version
        {
            get;
            private set;
        }

        public string Major
        {
            get
            {
                return this.Version.Major == 0 ? "" : this.Version.Major.ToString();
            }
        }

        public string Minor
        {
            get
            {
                return this.Version.Minor == 0 ? "" : this.Version.Minor.ToString();
            }
        }

        public string Build
        {
            get
            {
                return this.Version.Build == 0 ? "" : this.Version.Build.ToString();
            }
        }

        /// <summary>
        /// Gets the optional special version.
        /// </summary>
        public string SpecialVersion
        {
            get;
            private set;
        }

        public string[] GetOriginalVersionComponents()
        {
            if (!String.IsNullOrEmpty(_originalString))
            {
                string original;

                // search the start of the SpecialVersion part, if any
                int dashIndex = _originalString.IndexOf('-');
                if (dashIndex != -1)
                {
                    // remove the SpecialVersion part
                    original = _originalString.Substring(0, dashIndex);
                }
                else
                {
                    original = _originalString;
                }

                return SplitAndPadVersionString(original);
            }
            else
            {
                return SplitAndPadVersionString(Version.ToString());
            }
        }

        private static string[] SplitAndPadVersionString(string version)
        {
            string[] a = version.Split('.');
            if (a.Length == 4)
            {
                return a;
            }
            else
            {
                // if 'a' has less than 4 elements, we pad the '0' at the end 
                // to make it 4.
                var b = new string[4] { "0", "0", "0", "0" };
                Array.Copy(a, 0, b, 0, a.Length);
                return b;
            }
        }

        /// <summary>
        /// Parses a version string using loose semantic versioning rules that allows 2-4 version components followed by an optional special version.
        /// </summary>
        public static NuGetv2 Parse(string version)
        {
            if (String.IsNullOrEmpty(version))
            {
                throw new ArgumentException("Argument Cannot Be Null Or Empty", "version");
            }

            NuGetv2 semVer;
            if (!TryParse(version, out semVer))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Invalid Version String", version), "version");
            }
            return semVer;
        }

        /// <summary>
        /// Parses a version string using loose semantic versioning rules that allows 2-4 version components followed by an optional special version.
        /// </summary>
        public static bool TryParse(string version, out NuGetv2 value)
        {
            return TryParseInternal(version, _NuGetRegex, out value);
        }

        /// <summary>
        /// Parses a version string using strict semantic versioning rules that allows exactly 3 components and an optional special version.
        /// </summary>
        public static bool TryParseStrict(string version, out NuGetv2 value)
        {
            return TryParseInternal(version, _strictNuGetRegex, out value);
        }

        private static bool TryParseInternal(string version, Regex regex, out NuGetv2 semVer)
        {
            semVer = null;
            if (String.IsNullOrEmpty(version))
            {
                return false;
            }

            var match = regex.Match(version.Trim());
            Version versionValue;
            if (!match.Success || !Version.TryParse(match.Groups["Version"].Value, out versionValue))
            {
                return false;
            }

            semVer = new NuGetv2(NormalizeVersionValue(versionValue), match.Groups["Release"].Value.TrimStart('-'), version.Replace(" ", ""));
            return true;
        }

        /// <summary>
        /// Attempts to parse the version token as a NuGet.
        /// </summary>
        /// <returns>An instance of NuGet if it parses correctly, null otherwise.</returns>
        public static NuGetv2 ParseOptionalVersion(string version)
        {
            NuGetv2 semVer;
            TryParse(version, out semVer);
            return semVer;
        }

        private static Version NormalizeVersionValue(Version version)
        {
            return new Version(version.Major,
                                version.Minor,
                                Math.Max(version.Build, 0),
                                Math.Max(version.Revision, 0));
        }

        public int CompareTo(object obj)
        {
            if (Object.ReferenceEquals(obj, null))
            {
                return 1;
            }
            NuGetv2 other = obj as NuGetv2;
            if (other == null)
            {
                throw new ArgumentException("Must be a Semantic Verison", "obj");
            }
            return CompareTo(other);
        }

        public int CompareTo(NuGetv2 other)
        {
            if (Object.ReferenceEquals(other, null))
            {
                return 1;
            }

            int result = Version.CompareTo(other.Version);

            if (result != 0)
            {
                return result;
            }

            bool empty = String.IsNullOrEmpty(SpecialVersion);
            bool otherEmpty = String.IsNullOrEmpty(other.SpecialVersion);
            if (empty && otherEmpty)
            {
                return 0;
            }
            else if (empty)
            {
                return 1;
            }
            else if (otherEmpty)
            {
                return -1;
            }
            return StringComparer.OrdinalIgnoreCase.Compare(SpecialVersion, other.SpecialVersion);
        }

        public static bool operator ==(NuGetv2 version1, NuGetv2 version2)
        {
            if (Object.ReferenceEquals(version1, null))
            {
                return Object.ReferenceEquals(version2, null);
            }
            return version1.Equals(version2);
        }

        public static bool operator !=(NuGetv2 version1, NuGetv2 version2)
        {
            return !(version1 == version2);
        }

        public static bool operator <(NuGetv2 version1, NuGetv2 version2)
        {
            if (version1 == null)
            {
                throw new ArgumentNullException("version1");
            }
            return version1.CompareTo(version2) < 0;
        }

        public static bool operator <=(NuGetv2 version1, NuGetv2 version2)
        {
            return (version1 == version2) || (version1 < version2);
        }

        public static bool operator >(NuGetv2 version1, NuGetv2 version2)
        {
            if (version1 == null)
            {
                throw new ArgumentNullException("version1");
            }
            return version2 < version1;
        }

        public static bool operator >=(NuGetv2 version1, NuGetv2 version2)
        {
            return (version1 == version2) || (version1 > version2);
        }

        public override string ToString()
        {
            return _originalString;
        }

        /// <summary>
        /// Returns the normalized string representation of this instance of <see cref="NuGetv2"/>.
        /// If the instance can be strictly parsed as a <see cref="NuGetv2"/>, the normalized version
        /// string if of the format {major}.{minor}.{build}[-{special-version}]. If the instance has a non-zero
        /// value for <see cref="Version.Revision"/>, the format is {major}.{minor}.{build}.{revision}[-{special-version}].
        /// </summary>
        /// <returns>The normalized string representation.</returns>
        public string ToNormalizedString()
        {
            if (_normalizedVersionString == null)
            {
                var builder = new StringBuilder();
                builder
                    .Append(Version.Major)
                    .Append('.')
                    .Append(Version.Minor)
                    .Append('.')
                    .Append(Math.Max(0, Version.Build));

                if (Version.Revision > 0)
                {
                    builder.Append('.')
                            .Append(Version.Revision);
                }

                if (!string.IsNullOrEmpty(SpecialVersion))
                {
                    builder.Append('-')
                            .Append(SpecialVersion);
                }

                _normalizedVersionString = builder.ToString();
            }

            return _normalizedVersionString;
        }

        public bool Equals(NuGetv2 other)
        {
            return !Object.ReferenceEquals(null, other) &&
                    Version.Equals(other.Version) &&
                    SpecialVersion.Equals(other.SpecialVersion, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            NuGetv2 semVer = obj as NuGetv2;
            return !Object.ReferenceEquals(null, semVer) && Equals(semVer);
        }

        public override int GetHashCode()
        {
            int hashCode = Version.GetHashCode();
            if (SpecialVersion != null)
            {
                hashCode = hashCode * 4567 + SpecialVersion.GetHashCode();
            }

            return hashCode;
        }

        /*
        public static BinaryExpression GetBinaryExpression(NuGetv2 left, NuGetv2 right)
        {
            if (right.GetOriginalVersionComponents().Length == 0)
            {
                return GetBinaryExpression(ExpressionType.Equal, left, left);
            }
            else
            {
                BinaryExpression c = null;
                foreach (Comparator r in right)
                {
                    if (c == null)
                    {
                        c = GetBinaryExpression(r.Operator, left, r.Version);
                    }
                    else
                    {
                        c = Expression.AndAlso(c, GetBinaryExpression(r.Operator, left, r.Version));
                    }
                }
                return c;
            }
        }
        */
        public static BinaryExpression GetBinaryExpression(ExpressionType et, NuGetv2 left, NuGetv2 right)
        {
            return Expression.MakeBinary(et, Expression.Constant(left, typeof(NuGetv2)), Expression.Constant(right, typeof(NuGetv2)));
        }

        public static bool InvokeBinaryExpression(BinaryExpression be)
        {
            return Expression.Lambda<Func<bool>>(be).Compile().Invoke();
        }

        public static bool RangeIntersect(ExpressionType left_operator, NuGetv2 left, ExpressionType right_operator, NuGetv2 right)
        {
            if (left_operator != ExpressionType.LessThan && left_operator != ExpressionType.LessThanOrEqual &&
                    left_operator != ExpressionType.GreaterThan && left_operator != ExpressionType.GreaterThanOrEqual
                    && left_operator != ExpressionType.Equal)
                throw new ArgumentException("Unsupported left operator expression type " + left_operator.ToString() + ".");
            if (right_operator != ExpressionType.LessThan && right_operator != ExpressionType.LessThanOrEqual &&
                   right_operator != ExpressionType.GreaterThan && right_operator != ExpressionType.GreaterThanOrEqual
                   && right_operator != ExpressionType.Equal)
                throw new ArgumentException("Unsupported left operator expression type " + left_operator.ToString() + ".");

            if (left_operator == ExpressionType.Equal)
            {
                return InvokeBinaryExpression(GetBinaryExpression(right_operator, left, right));
            }
            else if (right_operator == ExpressionType.Equal)
            {
                return InvokeBinaryExpression(GetBinaryExpression(left_operator, right, left));
            }

            if ((left_operator == ExpressionType.LessThan || left_operator == ExpressionType.LessThanOrEqual)
                && (right_operator == ExpressionType.LessThan || right_operator == ExpressionType.LessThanOrEqual))
            {
                return true;
            }
            else if ((left_operator == ExpressionType.GreaterThan || left_operator == ExpressionType.GreaterThanOrEqual)
                && (right_operator == ExpressionType.GreaterThan || right_operator == ExpressionType.GreaterThanOrEqual))
            {
                return true;
            }

            else if ((left_operator == ExpressionType.LessThanOrEqual) && (right_operator == ExpressionType.GreaterThanOrEqual))
            {
                return right <= left;
            }

            else if ((left_operator == ExpressionType.GreaterThanOrEqual) && (right_operator == ExpressionType.LessThanOrEqual))
            {
                return right >= left;
            }


            else if ((left_operator == ExpressionType.LessThan || left_operator == ExpressionType.LessThanOrEqual)
                && (right_operator == ExpressionType.GreaterThan || right_operator == ExpressionType.GreaterThanOrEqual))
            {
                return right < left;
            }

            else
            {
                return right > left;
            }
        }

        public static BinaryExpression GetBinaryExpression(NuGetv2 left, ComparatorSet right)
        {
            if (right.Count == 0)
            {
                return GetBinaryExpression(ExpressionType.Equal, left, left);
            }
            else
            {
                BinaryExpression c = null;
                foreach (Comparator r in right)
                {
                    if (c == null)
                    {
                        c = GetBinaryExpression(r.Operator, left, r.Version);
                    }
                    else
                    {
                        c = Expression.AndAlso(c, GetBinaryExpression(r.Operator, left, r.Version));
                    }
                }
                return c;
            }
        }

        public static bool RangeIntersect(string left, string right)
        {
            Comparator l = Grammar.Comparator.Parse(left);
            Comparator r = Grammar.Comparator.Parse(right);
            return RangeIntersect(l.Operator, l.Version, r.Operator, r.Version);
        }

        public static bool Satisfies(NuGetv2 v, ComparatorSet s)
        {
            return InvokeBinaryExpression(GetBinaryExpression(v, s));
        }
    }
}
