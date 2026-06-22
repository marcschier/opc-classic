// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Cpx.Hosting;

/// <summary>
/// Configuration used by CPX DA-hosting helpers.
/// </summary>
public sealed class OpcCpxOptions
{
    private readonly List<DictionaryRegistration> _dictionaries = new();
    private readonly List<ComplexItemRegistration> _complexItems = new();

    /// <summary>
    /// Registered CPX type dictionaries.
    /// </summary>
    public IReadOnlyList<DictionaryRegistration> Dictionaries => _dictionaries;

    /// <summary>
    /// Registered DA items that expose complex-data metadata.
    /// </summary>
    public IReadOnlyList<ComplexItemRegistration> ComplexItems => _complexItems;

    /// <summary>
    /// Adds a type dictionary to the CPX browse namespace.
    /// </summary>
    public OpcCpxOptions AddDictionary(
        string typeSystemId,
        string dictionaryId,
        TypeDictionary dictionary,
        string? dictionaryValue = null,
        IReadOnlyDictionary<string, string>? typeDescriptionValues = null,
        string? dictionarySegment = null)
    {
        _dictionaries.Add(new DictionaryRegistration(
            typeSystemId,
            dictionaryId,
            dictionary,
            dictionaryValue,
            typeDescriptionValues,
            dictionarySegment));
        return this;
    }

    /// <summary>
    /// Adds a DA item whose value is described by a CPX type dictionary.
    /// </summary>
    public OpcCpxOptions AddComplexItem(
        string itemId,
        string typeSystemId,
        string dictionaryId,
        string typeId,
        string? consistencyWindow = null,
        string? writeBehavior = null,
        string? unconvertedItemId = null,
        string? unfilteredItemId = null,
        string? dataFilterValue = null)
    {
        _complexItems.Add(new ComplexItemRegistration(
            itemId,
            typeSystemId,
            dictionaryId,
            typeId,
            consistencyWindow,
            writeBehavior,
            unconvertedItemId,
            unfilteredItemId,
            dataFilterValue));
        return this;
    }

    internal bool TryGetDictionary(string typeSystemId, string dictionaryId, out DictionaryRegistration registration)
    {
        foreach (var candidate in _dictionaries)
        {
            if (StringComparer.Ordinal.Equals(candidate.TypeSystemId, typeSystemId)
                && StringComparer.Ordinal.Equals(candidate.DictionaryId, dictionaryId))
            {
                registration = candidate;
                return true;
            }
        }

        registration = null!;
        return false;
    }

    internal bool TryGetDictionaryBySegment(string typeSystemId, string dictionarySegment, out DictionaryRegistration registration)
    {
        foreach (var candidate in _dictionaries)
        {
            if (StringComparer.Ordinal.Equals(candidate.TypeSystemId, typeSystemId)
                && StringComparer.Ordinal.Equals(candidate.DictionarySegment, dictionarySegment))
            {
                registration = candidate;
                return true;
            }
        }

        registration = null!;
        return false;
    }

    internal bool TryGetComplexItem(string itemId, out ComplexItemRegistration registration)
    {
        foreach (var candidate in _complexItems)
        {
            if (StringComparer.Ordinal.Equals(candidate.ItemId, itemId))
            {
                registration = candidate;
                return true;
            }
        }

        registration = null!;
        return false;
    }

    /// <summary>
    /// A dictionary exposed under <c>/CPX/{TypeSystem}/{Dictionary}</c>.
    /// </summary>
    public sealed class DictionaryRegistration
    {
        private readonly Dictionary<string, string> _typeDescriptionValues;

        internal DictionaryRegistration(
            string typeSystemId,
            string dictionaryId,
            TypeDictionary dictionary,
            string? dictionaryValue,
            IReadOnlyDictionary<string, string>? typeDescriptionValues,
            string? dictionarySegment)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(typeSystemId);
            ArgumentException.ThrowIfNullOrWhiteSpace(dictionaryId);
            ArgumentNullException.ThrowIfNull(dictionary);

            TypeSystemId = typeSystemId;
            DictionaryId = dictionaryId;
            Dictionary = dictionary;
            DictionaryValue = string.IsNullOrWhiteSpace(dictionaryValue) ? null : dictionaryValue;
            DictionarySegment = string.IsNullOrWhiteSpace(dictionarySegment)
                ? CpxNamespaceBuilder.GetDictionarySegment(dictionaryId)
                : dictionarySegment.Trim('/', '\\');
            _typeDescriptionValues = CopyTypeDescriptions(typeDescriptionValues);
            TypeDescriptionValues = _typeDescriptionValues;
        }

        /// <summary>
        /// Type-system identifier, such as <c>XMLSchema</c> or <c>OPCBinary</c>.
        /// </summary>
        public string TypeSystemId { get; }

        /// <summary>
        /// Dictionary identifier exposed through property 601.
        /// </summary>
        public string DictionaryId { get; }

        /// <summary>
        /// Browse segment used below the type-system branch.
        /// </summary>
        public string DictionarySegment { get; }

        /// <summary>
        /// Parsed type dictionary.
        /// </summary>
        public TypeDictionary Dictionary { get; }

        /// <summary>
        /// Optional serialized dictionary value for property 603.
        /// </summary>
        public string? DictionaryValue { get; }

        /// <summary>
        /// Optional serialized type descriptions keyed by TypeID for property 604.
        /// </summary>
        public IReadOnlyDictionary<string, string> TypeDescriptionValues { get; }

        internal bool TryGetTypeDescriptionValue(string typeId, out string value) =>
            _typeDescriptionValues.TryGetValue(typeId, out value!);

        private static Dictionary<string, string> CopyTypeDescriptions(IReadOnlyDictionary<string, string>? values)
        {
            var copy = new Dictionary<string, string>(StringComparer.Ordinal);
            if (values is null)
            {
                return copy;
            }

            foreach (var pair in values)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(pair.Key, nameof(values));
                ArgumentException.ThrowIfNullOrWhiteSpace(pair.Value, nameof(values));
                copy.Add(pair.Key, pair.Value);
            }

            return copy;
        }
    }

    /// <summary>
    /// A DA item that publishes CPX properties 600-609.
    /// </summary>
    public sealed class ComplexItemRegistration
    {
        internal ComplexItemRegistration(
            string itemId,
            string typeSystemId,
            string dictionaryId,
            string typeId,
            string? consistencyWindow,
            string? writeBehavior,
            string? unconvertedItemId,
            string? unfilteredItemId,
            string? dataFilterValue)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
            ArgumentException.ThrowIfNullOrWhiteSpace(typeSystemId);
            ArgumentException.ThrowIfNullOrWhiteSpace(dictionaryId);
            ArgumentException.ThrowIfNullOrWhiteSpace(typeId);

            ItemId = itemId;
            TypeSystemId = typeSystemId;
            DictionaryId = dictionaryId;
            TypeId = typeId;
            ConsistencyWindow = Normalize(consistencyWindow);
            WriteBehavior = Normalize(writeBehavior);
            UnconvertedItemId = Normalize(unconvertedItemId);
            UnfilteredItemId = Normalize(unfilteredItemId);
            DataFilterValue = Normalize(dataFilterValue);
        }

        /// <summary>
        /// DA item identifier.
        /// </summary>
        public string ItemId { get; }

        /// <summary>
        /// Type-system identifier exposed through property 600.
        /// </summary>
        public string TypeSystemId { get; }

        /// <summary>
        /// Dictionary identifier exposed through property 601.
        /// </summary>
        public string DictionaryId { get; }

        /// <summary>
        /// Type identifier exposed through property 602.
        /// </summary>
        public string TypeId { get; }

        /// <summary>
        /// Optional consistency-window value for property 605.
        /// </summary>
        public string? ConsistencyWindow { get; }

        /// <summary>
        /// Optional write-behavior value for property 606.
        /// </summary>
        public string? WriteBehavior { get; }

        /// <summary>
        /// Optional unconverted source item for property 607.
        /// </summary>
        public string? UnconvertedItemId { get; }

        /// <summary>
        /// Optional unfiltered source item for property 608.
        /// </summary>
        public string? UnfilteredItemId { get; }

        /// <summary>
        /// Optional active data-filter expression for property 609.
        /// </summary>
        public string? DataFilterValue { get; }

        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
