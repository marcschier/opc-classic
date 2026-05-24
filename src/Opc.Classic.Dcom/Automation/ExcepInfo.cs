// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Automation; 

/// <summary>
/// Exposes error code, exception source, exception description and
/// help file path for an unsuccessful <seealso cref="IDispatch"/>
/// operation.
/// </summary>
public sealed class ExcepInfo {

    /// <summary>
    /// Create exception info
    /// </summary>
    internal ExcepInfo() {
    }

    internal void ClearAll() {
        ErrorCode = -1;
        ExcepSource = null;
        ExcepDesc = null;
        HelpFilePath = null;
    }

    /// <summary>
    /// An error code identifying the error.
    /// </summary>
    public int ErrorCode { get; internal set; } = -1;

    /// <summary>
    /// A textual, human-readable name of the source of the exception.
    /// Typically, this is an application name.
    /// </summary>
    public string ExcepSource { get; internal set; }

    /// <summary>
    /// A textual, human-readable description of the error intended
    ///  for the customer. If nodescription is available it returns
    ///  <code>null</code>.
    /// </summary>
    public string ExcepDesc { get; internal set; }

    /// <summary>
    /// The fully qualified drive, path, and file name of a Help file
    /// that has more information about the error. If no Help is
    /// available it returns <code>null</code>.
    /// </summary>
    public string HelpFilePath { get; internal set; }
}
