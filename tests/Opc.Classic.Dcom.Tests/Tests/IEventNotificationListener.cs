// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Core;


    public interface IEventNotificationListener {

        void OnEvent(Struct[] @event);

    }

}
