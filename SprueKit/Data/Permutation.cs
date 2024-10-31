using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Reflection;

namespace SprueKit.Data
{
    public class PermutationValue : BaseClass
    {
        object value_ = null;
        int weight_ = 1;
        uint flags_ = 0;
        string name_ = "";

        public object Value { get { return value_; } set {
                if (value.GetType() == typeof(string))
                    value_ = Convert.ChangeType(value, value_.GetType());
                else
                    value_ = value;
                OnPropertyChanged();
            }
        }
        public int Weight { get { return weight_; } set { weight_ = value; OnPropertyChanged(); } }
        public string Name { get { return name_; } set { name_ = value; OnPropertyChanged(); } }
        public uint Flags { get { return flags_; } set { flags_ = value; OnPropertyChanged(); } }

        public string TextValue { get { return Value != null ? Value.ToString() : "< none >"; } }

        public string TextFlags
        {
            get
            {
                return Settings.BitFieldNames.MakeFlagName(new IOCDependency<Settings.BitFieldNames>().Object.FlagNames, Flags);
            }
        }

        private PermutationValue()
        {

        }

        public PermutationValue(Type dataType)
        {
            if (dataType == typeof(string))
                Value = "";
            else
            {
                try
                {
                    value_ = Activator.CreateInstance(dataType);
                }
                catch (Exception) {
                    ErrorHandler.inst().PublishError(string.Format("Unable to instantiate type of {0} for permutation value.", dataType.Name), 3);
                }
            }
        }

        public PermutationValue(PropertyInfo pi, object src)
        {
            value_ = pi.GetValue(src);
            if (value_ == null)
                value_ = Activator.CreateInstance(pi.GetType());
        }

        public static PermutationValue CreateFromFile(PropertyInfo pi, Type dataType)
        {
            PermutationValue ret = new PermutationValue(dataType);
            return ret;
        }
    }

    public class PermutationSet
    {
        string property_;
        Type dataType_;

        public PermutationSet(Type fieldType, string property)
        {
            property_ = property;
            dataType_ = fieldType;
        }

        public Type DataType { get { return dataType_; } }
        public string Property { get { return property_; } private set { property_ = value; } }
        public ObservableCollection<PermutationValue> Values { get; private set; } = new ObservableCollection<PermutationValue>();

        public void AddNew()
        {
            Values.Add(new Data.PermutationValue(dataType_));
        }
    }

    /// <summary>
    /// For user interface represents a collapsed set of permutations
    /// </summary>
    public class PermutationRecord
    {
        public string Property { get; set; }
        public Type DataType { get; set; }
        public string DataTypeName { get { return DataType.Name; } }
        public PermutationValue Value { get; set; }

        /// <summary>
        /// Helper method
        /// </summary>
        /// <param name="sets">Permutations table to process</param>
        /// <returns>flat collection of records</returns>
        public static List<PermutationRecord> GetRecords(Dictionary<string, PermutationSet> sets)
        {
            List<PermutationRecord> ret = new List<PermutationRecord>();
            foreach (var set in sets)
            {
                foreach (var val in set.Value.Values)
                {
                    ret.Add(new PermutationRecord
                    {
                        Property = set.Value.Property,
                        DataType = set.Value.DataType,
                        Value = val
                    });
                }
            }
            return ret;
        }
    }
}
