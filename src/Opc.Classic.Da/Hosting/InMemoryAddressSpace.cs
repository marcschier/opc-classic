//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// In-memory hierarchical address-space implementation backed by a static
/// dictionary of branch → (sub-branches + items). Useful for sample servers
/// and tests; production servers should implement
/// <see cref="IOpcAddressSpace"/> directly against their data store.
/// </summary>
public sealed class InMemoryAddressSpace : IOpcAddressSpace
{
    private const char BranchSeparator = '.';
    private readonly Dictionary<string, BranchNode> _branches;

    /// <summary>
    /// Initializes a new in-memory hierarchical address-space.
    /// </summary>
    /// <param name="rootBranches">
    /// Root-level branch names; each branch is created empty and populated
    /// via <see cref="AddItem"/> / <see cref="AddBranch"/>.
    /// </param>
    public InMemoryAddressSpace(params string[] rootBranches)
    {
        ArgumentNullException.ThrowIfNull(rootBranches);
        _branches = new Dictionary<string, BranchNode>(StringComparer.Ordinal)
        {
            [string.Empty] = new BranchNode(),
        };
        foreach (string branch in rootBranches)
        {
            AddBranch(branch);
        }
    }

    /// <summary>
    /// Always reports as hierarchical (OPCNS_HIERARCHIAL = 2).
    /// </summary>
    public bool IsHierarchical => true;

    /// <summary>
    /// Adds a branch to the address space; intermediate branches are auto-created.
    /// </summary>
    public void AddBranch(string branchPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(branchPath);
        string[] parts = branchPath.Split(BranchSeparator);
        string current = string.Empty;
        for (int i = 0; i < parts.Length; i++)
        {
            string parent = current;
            current = string.IsNullOrEmpty(parent) ? parts[i] : $"{parent}{BranchSeparator}{parts[i]}";
            if (!_branches.ContainsKey(current))
            {
                _branches[current] = new BranchNode();
                _branches[parent].SubBranches.Add(parts[i]);
            }
        }
    }

    /// <summary>
    /// Adds a leaf item ID under <paramref name="branchPath"/> (empty = root).
    /// </summary>
    public void AddItem(string branchPath, string itemName)
    {
        ArgumentNullException.ThrowIfNull(branchPath);
        ArgumentException.ThrowIfNullOrEmpty(itemName);
        if (!_branches.TryGetValue(branchPath, out BranchNode? node))
        {
            AddBranch(branchPath);
            node = _branches[branchPath];
        }
        node.Items.Add(itemName);
    }

    /// <inheritdoc />
    public Task<OpcBrowseResult> BrowseAsync(string? branchPath, OpcBrowseElementKind kind, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string key = branchPath ?? string.Empty;
        if (!_branches.TryGetValue(key, out BranchNode? node))
        {
            return Task.FromResult(OpcBrowseResult.Empty);
        }
        IReadOnlyList<string> branches = kind == OpcBrowseElementKind.Items
            ? Array.Empty<string>()
            : node.SubBranches.ToArray();
        IReadOnlyList<string> items = kind == OpcBrowseElementKind.Branches
            ? Array.Empty<string>()
            : node.Items.ToArray();
        return Task.FromResult(new OpcBrowseResult(branches, items));
    }

    /// <inheritdoc />
    public Task<string> GetItemIdAsync(string? currentBranchPath, string itemDataId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(itemDataId);
        cancellationToken.ThrowIfCancellationRequested();
        string qualified = string.IsNullOrEmpty(currentBranchPath)
            ? itemDataId
            : $"{currentBranchPath}{BranchSeparator}{itemDataId}";
        return Task.FromResult(qualified);
    }

    private sealed class BranchNode
    {
        public List<string> SubBranches { get; } = new();
        public List<string> Items { get; } = new();
    }
}
