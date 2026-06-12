// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Opc.Classic.Dcom.Internal;

namespace Opc.Classic.Dcom.Test;

public sealed class PropertyBagTests
{
    [Test]
    public async Task PropertyBag_returns_null_for_missing_key()
    {
        var bag = new PropertyBag();

        await Assert.That(bag.GetProperty("missing")).IsNull();
    }

    [Test]
    public async Task PropertyBag_set_then_get_round_trips()
    {
        var bag = new PropertyBag();

        bag.SetProperty("rpc.ntlm.sign", "true");

        await Assert.That(bag.GetProperty("rpc.ntlm.sign")).IsEqualTo("true");
    }

    [Test]
    public async Task PropertyBag_default_value_overload()
    {
        var bag = new PropertyBag();

        await Assert.That(bag.GetProperty("rpc.socketTimeout", "0")).IsEqualTo("0");
    }

    [Test]
    public async Task PropertyBag_copy_constructor_carries_defaults()
    {
        var defaults = new PropertyBag();
        defaults.SetProperty("rpc.ntlm.ntlmv2", "true");

        var bag = new PropertyBag(defaults);

        await Assert.That(bag.GetProperty("rpc.ntlm.ntlmv2")).IsEqualTo("true");
    }
}
