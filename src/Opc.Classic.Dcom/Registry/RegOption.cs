// SPDX-License-Identifier: MIT

using System;

namespace Opc.Classic.Dcom.Registry; 
/// <summary>
/// Registry options
/// </summary>
[Flags]
public enum RegOption {

    /// <summary>
    /// This key is not volatile; this is the default. The information
    /// is stored in a file and is preserved when the system is restarted.
    /// The RegSaveKey function saves keys that are not volatile.
    /// </summary>
    REG_OPTION_NON_VOLATILE = 0,

    /// <summary>
    /// All keys created by the function are volatile. The information is
    /// stored in memory and is not preserved when the corresponding
    /// registry hive is unloaded.
    /// </summary>
    REG_OPTION_VOLATILE = 1,

    /// <summary>
    /// This key is a symbolic link. The target path is assigned to the
    /// L"SymbolicLinkValue" value of the key. The target path must be
    /// an absolute registry path.
    /// </summary>
    REG_OPTION_CREATE_LINK = 2,

    /// <summary>
    /// If this flag is set, the function ignores the samDesired parameter
    /// and attempts to open the key with the access required to backup or
    /// restore the key
    /// </summary>
    REG_OPTION_BACKUP_RESTORE = 4,
}
