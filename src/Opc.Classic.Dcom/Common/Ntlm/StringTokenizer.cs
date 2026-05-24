// SPDX-License-Identifier: MIT

using System;

namespace SharpCifs.Util.Sharpen;

public sealed class StringTokenizer {
    private readonly string[] _tokens;
    private int _index;

    public StringTokenizer(string value, string delimiters) =>
        _tokens = value.Split(delimiters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

    public string NextToken() {
        if (_index >= _tokens.Length) {
            throw new NoSuchElementException();
        }

        return _tokens[_index++];
    }
}