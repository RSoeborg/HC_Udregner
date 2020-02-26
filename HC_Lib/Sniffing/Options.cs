﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;

namespace HC_Lib.Sniffing
{
    public class OptionValueCollection : IList, IList<string>
    {
        private readonly List<string> values = new List<string>();
        private readonly OptionContext c;

        internal OptionValueCollection(OptionContext c)
        {
            this.c = c;
        }

        #region ICollection
        void ICollection.CopyTo(Array array, int index) { (this.values as ICollection).CopyTo(array, index); }
        bool ICollection.IsSynchronized => (this.values as ICollection).IsSynchronized;
        object ICollection.SyncRoot => (this.values as ICollection).SyncRoot;

        #endregion

        #region ICollection<T>
        public void Add(string item)
        {
            this.values.Add(item);
        }
        public void Clear()
        {
            this.values.Clear();
        }
        public bool Contains(string item) { return this.values.Contains(item); }
        public void CopyTo(string[] array, int arrayIndex)
        {
            this.values.CopyTo(array, arrayIndex);
        }
        public bool Remove(string item) { return this.values.Remove(item); }
        public int Count => this.values.Count;
        public bool IsReadOnly => false;

        #endregion

        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator() { return this.values.GetEnumerator(); }
        #endregion

        #region IEnumerable<T>
        public IEnumerator<string> GetEnumerator() { return this.values.GetEnumerator(); }
        #endregion

        #region IList
        int IList.Add(object value) { return (this.values as IList).Add(value); }
        bool IList.Contains(object value) { return (this.values as IList).Contains(value); }
        int IList.IndexOf(object value) { return (this.values as IList).IndexOf(value); }
        void IList.Insert(int index, object value) { (this.values as IList).Insert(index, value); }
        void IList.Remove(object value) { (this.values as IList).Remove(value); }
        void IList.RemoveAt(int index) { (this.values as IList).RemoveAt(index); }
        bool IList.IsFixedSize => false;
        object IList.this[int index]
        {
            get => this[index];
            set => (this.values as IList)[index] = value;
        }
        #endregion

        #region IList<T>
        public int IndexOf(string item) { return this.values.IndexOf(item); }
        public void Insert(int index, string item)
        {
            this.values.Insert(index, item);
        }
        public void RemoveAt(int index)
        {
            this.values.RemoveAt(index);
        }

        private void AssertValid(int index)
        {
            if (this.c.Option == null)
                throw new InvalidOperationException("OptionContext.Option is null.");
            if (index >= this.c.Option.MaxValueCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (this.c.Option.OptionValueType == OptionValueType.Required && index >= this.values.Count)
                throw new OptionException(string.Format(this.c.OptionSet.MessageLocalizer("Missing required value for option '{0}'."), this.c.OptionName), this.c.OptionName);
        }

        public string this[int index]
        {
            get
            {
                AssertValid(index);
                return index >= this.values.Count ? null : this.values[index];
            }
            set => this.values[index] = value;
        }
        #endregion

        public override string ToString()
        {
            return string.Join(", ", this.values.ToArray());
        }
    }

    public class OptionContext
    {
        public OptionContext(OptionSet set)
        {
            this.OptionSet = set;
            this.OptionValues = new OptionValueCollection(this);
        }

        public Option Option { get; set; }
        public string OptionName { get; set; }
        public int OptionIndex { get; set; }
        public OptionSet OptionSet { get; }
        public OptionValueCollection OptionValues { get; }
    }

    public enum OptionValueType
    {
        None,
        Optional,
        Required
    }

    public abstract class Option
    {
        protected Option(string prototype, string description)
            : this(prototype, description, 1)
        {
        }

        protected Option(string prototype, string description, int maxValueCount)
        {
            if (prototype == null)
                throw new ArgumentNullException(nameof(prototype));
            if (prototype.Length == 0)
                throw new ArgumentException("Cannot be the empty string.", nameof(prototype));
            if (maxValueCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxValueCount));

            this.Prototype = prototype;
            this.Names = prototype.Split('|');
            this.Description = description;
            this.MaxValueCount = maxValueCount;
            this.OptionValueType = ParsePrototype();

            if (this.MaxValueCount == 0 && this.OptionValueType != OptionValueType.None)
                throw new ArgumentException("Cannot provide maxValueCount of 0 for OptionValueType.Required or OptionValueType.Optional.", nameof(maxValueCount));
            if (this.OptionValueType == OptionValueType.None && maxValueCount > 1)
                throw new ArgumentException($"Cannot provide maxValueCount of {maxValueCount} for OptionValueType.None.", nameof(maxValueCount));
            if (Array.IndexOf(this.Names, "<>") >= 0 &&
                    (this.Names.Length == 1 && this.OptionValueType != OptionValueType.None || this.Names.Length > 1 && this.MaxValueCount > 1))
                throw new ArgumentException("The default option handler '<>' cannot require values.", nameof(prototype));
        }

        public string Prototype { get; }
        public string Description { get; }
        public OptionValueType OptionValueType { get; }
        public int MaxValueCount { get; }

        protected static T Parse<T>(string value, OptionContext c)
        {
            var conv = TypeDescriptor.GetConverter(typeof(T));
            T t = default;

            try
            {
                if (value != null)
                    t = (T)conv.ConvertFromString(value);
            }
            catch (Exception e)
            {
                throw new OptionException(
                        string.Format(
                            c.OptionSet.MessageLocalizer("Could not convert string `{0}' to type {1} for option `{2}'."),
                            value, typeof(T).Name, c.OptionName),
                        c.OptionName, e);
            }

            return t;
        }

        internal string[] Names { get; }

        internal string[] ValueSeparators { get; private set; }

        private static readonly char[] NameTerminator = { '=', ':' };

        private OptionValueType ParsePrototype()
        {
            var type = '\0';
            var seps = new List<string>();

            for (int i = 0; i < this.Names.Length; ++i)
            {
                var name = this.Names[i];

                if (name.Length == 0)
                    throw new ArgumentException("Empty option names are not supported.", nameof(name));

                int end = name.IndexOfAny(NameTerminator);
                if (end == -1)
                    continue;
                this.Names[i] = name.Substring(0, end);
                if (type == '\0' || type == name[end])
                    type = name[end];
                else
                    throw new ArgumentException(
                        $"Conflicting option types: '{type}' vs. '{name[end]}'.", nameof(type));
                AddSeparators(name, end, seps);
            }

            if (type == '\0')
                return OptionValueType.None;

            if (this.MaxValueCount <= 1 && seps.Count != 0)
                throw new ArgumentException($"Cannot provide key/value separators for Options taking {this.MaxValueCount} value(s).", nameof(this.MaxValueCount));

            if (this.MaxValueCount > 1)
            {
                if (seps.Count == 0)
                    this.ValueSeparators = new[] { ":", "=" };
                else if (seps.Count == 1 && seps[0].Length == 0)
                    this.ValueSeparators = null;
                else
                    this.ValueSeparators = seps.ToArray();
            }

            return type == '=' ? OptionValueType.Required : OptionValueType.Optional;
        }

        private static void AddSeparators(string name, int end, ICollection<string> seps)
        {
            int start = -1;

            for (int i = end + 1; i < name.Length; ++i)
            {
                switch (name[i])
                {
                    case '{':
                        if (start != -1)
                            throw new ArgumentException($"Ill-formed name/value separator found in \"{name}\".", nameof(start));
                        start = i + 1;
                        break;
                    case '}':
                        if (start == -1)
                            throw new ArgumentException($"Ill-formed name/value separator found in \"{name}\".", nameof(start));
                        seps.Add(name.Substring(start, i - start));
                        start = -1;
                        break;
                    default:
                        if (start == -1)
                            seps.Add(name[i].ToString());
                        break;
                }
            }
            if (start != -1)
                throw new ArgumentException($"Ill-formed name/value separator found in \"{name}\".", nameof(start));
        }

        public void Invoke(OptionContext c)
        {
            OnParseComplete(c);
            c.OptionName = null;
            c.Option = null;
            c.OptionValues.Clear();
        }

        protected abstract void OnParseComplete(OptionContext c);

        public override string ToString()
        {
            return this.Prototype;
        }
    }

    [Serializable]
    public class OptionException : Exception
    {
        public OptionException()
        {
        }

        public OptionException(string message, string optionName)
            : base(message)
        {
            this.OptionName = optionName;
        }

        public OptionException(string message, string optionName, Exception innerException)
            : base(message, innerException)
        {
            this.OptionName = optionName;
        }

        protected OptionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.OptionName = info.GetString("OptionName");
        }

        public string OptionName { get; }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("OptionName", this.OptionName);
        }
    }

    public delegate void OptionAction<in TKey, in TValue>(TKey key, TValue value);

    public class OptionSet : KeyedCollection<string, Option>
    {
        public OptionSet()
            : this(x => x)
        {
        }

        public OptionSet(Converter<string, string> localizer)
        {
            this.MessageLocalizer = localizer;
        }

        public Converter<string, string> MessageLocalizer { get; }

        protected override string GetKeyForItem(Option item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (item.Names != null && item.Names.Length > 0)
                return item.Names[0];

            // This should never happen, as it's invalid for Option to be
            // constructed w/o any names.
            throw new InvalidOperationException("Option has no names!");
        }

        protected Option GetOptionForName(string option)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            try
            {
                return base[option];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        protected override void InsertItem(int index, Option item)
        {
            base.InsertItem(index, item);
            AddImpl(item);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            Option p = this.Items[index];

            // KeyedCollection.RemoveItem() handles the 0th item
            for (int i = 1; i < p.Names.Length; ++i)
            {
                this.Dictionary.Remove(p.Names[i]);
            }
        }

        protected override void SetItem(int index, Option item)
        {
            base.SetItem(index, item);
            RemoveItem(index);
            AddImpl(item);
        }

        private void AddImpl(Option option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            var added = new List<string>(option.Names.Length);

            try
            {
                // KeyedCollection.InsertItem/SetItem handle the 0th name.
                for (int i = 1; i < option.Names.Length; ++i)
                {
                    this.Dictionary.Add(option.Names[i], option);
                    added.Add(option.Names[i]);
                }
            }
            catch (Exception)
            {
                foreach (string name in added) this.Dictionary.Remove(name);
                throw;
            }
        }

        public new OptionSet Add(Option option)
        {
            base.Add(option);
            return this;
        }

        private sealed class ActionOption : Option
        {
            private readonly Action<OptionValueCollection> action;

            public ActionOption(string prototype, string description, int count, Action<OptionValueCollection> action)
                : base(prototype, description, count)
            {
                this.action = action ?? throw new ArgumentNullException(nameof(action));
            }

            protected override void OnParseComplete(OptionContext c)
            {
                this.action(c.OptionValues);
            }
        }

        public OptionSet Add(string prototype, string description, Action<string> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            Option p = new ActionOption(prototype, description, 1, x => action(x[0]));
            base.Add(p);
            return this;
        }

        public OptionSet Add(string prototype, string description, OptionAction<string, string> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Option p = new ActionOption(prototype, description, 2, x => action(x[0], x[1]));

            base.Add(p);
            return this;
        }

        private sealed class ActionOption<T> : Option
        {
            private readonly Action<T> action;

            public ActionOption(string prototype, string description, Action<T> action)
                : base(prototype, description, 1)
            {
                this.action = action ?? throw new ArgumentNullException(nameof(action));
            }

            protected override void OnParseComplete(OptionContext c)
            {
                this.action(Parse<T>(c.OptionValues[0], c));
            }
        }

        private sealed class ActionOption<TKey, TValue> : Option
        {
            private readonly OptionAction<TKey, TValue> action;

            public ActionOption(string prototype, string description, OptionAction<TKey, TValue> action)
                : base(prototype, description, 2)
            {
                this.action = action ?? throw new ArgumentNullException(nameof(action));
            }

            protected override void OnParseComplete(OptionContext c)
            {
                this.action(Parse<TKey>(c.OptionValues[0], c), Parse<TValue>(c.OptionValues[1], c));
            }
        }

        protected virtual OptionContext CreateOptionContext()
        {
            return new OptionContext(this);
        }

        public List<string> Parse(IEnumerable<string> arguments)
        {
            var process = true;
            OptionContext c = CreateOptionContext();
            c.OptionIndex = -1;
            var def = GetOptionForName("<>");
            var unprocessed =
                from argument in arguments
                where ++c.OptionIndex >= 0 && (process || def != null)
                    ? process
                        ? argument == "--"
                            ? process = false
                            : !Parse(argument, c)
                                ? def != null
                                    ? Unprocessed(null, def, c, argument)
                                    : true
                                : false
                        : def != null
                            ? Unprocessed(null, def, c, argument)
                            : true
                    : true
                select argument;

            List<string> r = unprocessed.ToList();
            c.Option?.Invoke(c);
            return r;
        }

        private static bool Unprocessed(ICollection<string> extra, Option def, OptionContext c, string argument)
        {
            if (def == null)
            {
                extra.Add(argument);
                return false;
            }

            c.OptionValues.Add(argument);
            c.Option = def;
            c.Option.Invoke(c);

            return false;
        }

        private static readonly Regex ValueOptionRegex = new Regex(@"^(?<flag>--|-|/)(?<name>[^:=]+)((?<sep>[:=])(?<value>.*))?$", RegexOptions.Compiled);

        protected bool GetOptionParts(string argument, out string flag, out string name, out string sep, out string value)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            flag = name = sep = value = null;
            Match m = ValueOptionRegex.Match(argument);

            if (!m.Success)
            {
                return false;
            }

            flag = m.Groups["flag"].Value;
            name = m.Groups["name"].Value;

            if (m.Groups["sep"].Success && m.Groups["value"].Success)
            {
                sep = m.Groups["sep"].Value;
                value = m.Groups["value"].Value;
            }

            return true;
        }

        protected virtual bool Parse(string argument, OptionContext c)
        {
            if (c.Option != null)
            {
                ParseValue(argument, c);
                return true;
            }

            if (!GetOptionParts(argument, out var f, out var n, out var s, out var v))
                return false;

            if (Contains(n))
            {
                Option p = this[n];
                c.OptionName = f + n;
                c.Option = p;

                switch (p.OptionValueType)
                {
                    case OptionValueType.None:
                        c.OptionValues.Add(n);
                        c.Option.Invoke(c);
                        break;
                    case OptionValueType.Optional:
                    case OptionValueType.Required:
                        ParseValue(v, c);
                        break;
                }

                return true;
            }
            // no match; is it a bool option?
            if (ParseBool(argument, n, c))
                return true;
            // is it a bundled option?
            if (ParseBundledValue(f, String.Concat(n + s + v), c))
                return true;

            return false;
        }

        private void ParseValue(string option, OptionContext c)
        {
            if (option != null)
            {
                foreach (string o in c.Option.ValueSeparators != null
                    ? option.Split(c.Option.ValueSeparators, StringSplitOptions.None)
                    : new[] { option })
                {
                    c.OptionValues.Add(o);
                }
            }

            if (c.OptionValues.Count == c.Option.MaxValueCount || c.Option.OptionValueType == OptionValueType.Optional)
            {
                c.Option.Invoke(c);
            }
            else if (c.OptionValues.Count > c.Option.MaxValueCount)
            {
                throw new OptionException(this.MessageLocalizer($"Error: Found {c.OptionValues.Count} option values when expecting {c.Option.MaxValueCount}."), c.OptionName);
            }
        }

        private bool ParseBool(string option, string n, OptionContext c)
        {
            string rn;

            if (n.Length >= 1 && (n[n.Length - 1] == '+' || n[n.Length - 1] == '-') &&
                Contains((rn = n.Substring(0, n.Length - 1))))
            {
                Option p = this[rn];
                string v = n[n.Length - 1] == '+' ? option : null;
                c.OptionName = option;
                c.Option = p;
                c.OptionValues.Add(v);
                p.Invoke(c);

                return true;
            }

            return false;
        }

        private bool ParseBundledValue(string f, string n, OptionContext c)
        {
            if (f != "-")
                return false;

            for (int i = 0; i < n.Length; ++i)
            {
                string opt = f + n[i];
                string rn = n[i].ToString();

                if (!Contains(rn))
                {
                    if (i == 0)
                        return false;

                    throw new OptionException(string.Format(this.MessageLocalizer("Cannot bundle unregistered option '{0}'."), opt), opt);
                }

                Option p = this[rn];
                switch (p.OptionValueType)
                {
                    case OptionValueType.None:
                        Invoke(c, opt, n, p);
                        break;
                    case OptionValueType.Optional:
                    case OptionValueType.Required:
                        {
                            string v = n.Substring(i + 1);
                            c.Option = p;
                            c.OptionName = opt;
                            ParseValue(v.Length != 0 ? v : null, c);
                            return true;
                        }
                    default:
                        throw new InvalidOperationException("Unknown OptionValueType: " + p.OptionValueType);
                }
            }

            return true;
        }

        private static void Invoke(OptionContext c, string name, string value, Option option)
        {
            c.OptionName = name;
            c.Option = option;
            c.OptionValues.Add(value);
            option.Invoke(c);
        }

        private const int OPTION_WIDTH = 29;

        public void WriteOptionDescriptions(TextWriter o)
        {
            foreach (Option p in this)
            {
                int written = 0;
                if (!WriteOptionPrototype(o, p, ref written))
                    continue;

                if (written < OPTION_WIDTH)
                    o.Write(new string(' ', OPTION_WIDTH - written));
                else
                {
                    o.WriteLine();
                    o.Write(new string(' ', OPTION_WIDTH));
                }

                List<string> lines = GetLines(this.MessageLocalizer(GetDescription(p.Description)));
                o.WriteLine(lines[0]);
                string prefix = new string(' ', OPTION_WIDTH + 2);
                for (int i = 1; i < lines.Count; ++i)
                {
                    o.Write(prefix);
                    o.WriteLine(lines[i]);
                }
            }
        }

        private bool WriteOptionPrototype(TextWriter o, Option p, ref int written)
        {
            string[] names = p.Names;

            int i = GetNextOptionIndex(names, 0);
            if (i == names.Length)
                return false;

            if (names[i].Length == 1)
            {
                Write(o, ref written, "  -");
                Write(o, ref written, names[0]);
            }
            else
            {
                Write(o, ref written, "      --");
                Write(o, ref written, names[0]);
            }

            for (i = GetNextOptionIndex(names, i + 1);
                    i < names.Length; i = GetNextOptionIndex(names, i + 1))
            {
                Write(o, ref written, ", ");
                Write(o, ref written, names[i].Length == 1 ? "-" : "--");
                Write(o, ref written, names[i]);
            }

            if (p.OptionValueType == OptionValueType.Optional || p.OptionValueType == OptionValueType.Required)
            {
                if (p.OptionValueType == OptionValueType.Optional)
                {
                    Write(o, ref written, this.MessageLocalizer("["));
                }
                Write(o, ref written, this.MessageLocalizer("=" + GetArgumentName(0, p.MaxValueCount, p.Description)));
                string sep = p.ValueSeparators != null && p.ValueSeparators.Length > 0
                    ? p.ValueSeparators[0]
                    : " ";
                for (int c = 1; c < p.MaxValueCount; ++c)
                {
                    Write(o, ref written, this.MessageLocalizer(sep + GetArgumentName(c, p.MaxValueCount, p.Description)));
                }
                if (p.OptionValueType == OptionValueType.Optional)
                {
                    Write(o, ref written, this.MessageLocalizer("]"));
                }
            }
            return true;
        }

        private static int GetNextOptionIndex(string[] names, int i)
        {
            while (i < names.Length && names[i] == "<>")
            {
                ++i;
            }

            return i;
        }

        private static void Write(TextWriter o, ref int n, string s)
        {
            n += s.Length;
            o.Write(s);
        }

        private static string GetArgumentName(int index, int maxIndex, string description)
        {
            if (description == null)
                return maxIndex == 1 ? "VALUE" : "VALUE" + (index + 1);

            var nameStart = maxIndex == 1
                ? new[] { "{0:", "{" }
                : new[] { "{" + index + ":" };

            for (int i = 0; i < nameStart.Length; ++i)
            {
                int start, j = 0;
                do
                {
                    start = description.IndexOf(nameStart[i], j);
                } while (start >= 0 && j != 0 ? description[j++ - 1] == '{' : false);
                if (start == -1)
                    continue;
                int end = description.IndexOf("}", start);
                if (end == -1)
                    continue;
                return description.Substring(start + nameStart[i].Length, end - start - nameStart[i].Length);
            }
            return maxIndex == 1 ? "VALUE" : "VALUE" + (index + 1);
        }

        private static string GetDescription(string description)
        {
            if (description == null)
                return String.Empty;

            var sb = new StringBuilder(description.Length);
            int start = -1;

            for (int i = 0; i < description.Length; ++i)
            {
                switch (description[i])
                {
                    case '{':
                        if (i == start)
                        {
                            sb.Append('{');
                            start = -1;
                        }
                        else if (start < 0)
                            start = i + 1;
                        break;
                    case '}':
                        if (start < 0)
                        {
                            if ((i + 1) == description.Length || description[i + 1] != '}')
                                throw new InvalidOperationException("Invalid option description: " + description);
                            ++i;
                            sb.Append("}");
                        }
                        else
                        {
                            sb.Append(description.Substring(start, i - start));
                            start = -1;
                        }
                        break;
                    case ':':
                        if (start < 0)
                            goto default;
                        start = i + 1;
                        break;
                    default:
                        if (start < 0)
                            sb.Append(description[i]);
                        break;
                }
            }
            return sb.ToString();
        }

        private static List<string> GetLines(string description)
        {
            var lines = new List<string>();

            if (string.IsNullOrEmpty(description))
            {
                lines.Add(string.Empty);
                return lines;
            }

            const int length = 80 - OPTION_WIDTH - 2;
            int start = 0, end;

            do
            {
                end = GetLineEnd(start, length, description);
                bool cont = false;
                if (end < description.Length)
                {
                    char c = description[end];
                    if (c == '-' || (char.IsWhiteSpace(c) && c != '\n'))
                        ++end;
                    else if (c != '\n')
                    {
                        cont = true;
                        --end;
                    }
                }
                lines.Add(description.Substring(start, end - start));
                if (cont)
                {
                    lines[lines.Count - 1] += "-";
                }
                start = end;
                if (start < description.Length && description[start] == '\n')
                    ++start;
            } while (end < description.Length);
            return lines;
        }

        private static int GetLineEnd(int start, int length, string description)
        {
            int end = Math.Min(start + length, description.Length);
            int sep = -1;
            for (int i = start; i < end; ++i)
            {
                switch (description[i])
                {
                    case ' ':
                    case '\t':
                    case '\v':
                    case '-':
                    case ',':
                    case '.':
                    case ';':
                        sep = i;
                        break;
                    case '\n':
                        return i;
                }
            }
            if (sep == -1 || end == description.Length)
                return end;
            return sep;
        }
    }
}
