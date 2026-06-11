// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Test;

public interface IEventNotificationListener
{

    void OnEvent(Struct[] @event);

}
