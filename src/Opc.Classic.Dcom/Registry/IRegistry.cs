// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom.Common;

namespace Opc.Classic.Dcom.Registry;

/// <summary>
/// Perform C-R-U-D on the Windows Registry.
/// This interface uses "Windows Remote Registry" and "Server" services
/// and these must be running on target workstation.
/// </summary>
public interface IRegistry
{
    /// <summary>
    /// Opens the HKEY_CLASSES_ROOT key
    /// </summary>
    /// <returns> handle representing the opened key </returns>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    PolicyHandle OpenHKCR();

    /// <summary>
    /// Opens the HKEY_CURRENT_USER key
    /// </summary>
    /// <returns> handle representing the opened key </returns>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    PolicyHandle OpenHKCU();

    /// <summary>
    /// Opens the HKEY_USERS key
    /// </summary>
    /// <returns> handle representing the opened key </returns>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    PolicyHandle OpenHKU();

    /// <summary>
    /// Opens the HKEY_LOCAL_MACHINE key
    /// </summary>
    /// <returns> handle representing the opened key </returns>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    PolicyHandle OpenHKLM();

    /// <summary>
    /// Opens the subkey of key specified by handle.
    /// </summary>
    /// <param name="handle">Registry policy handle for the parent key to open from.</param>
    /// <param name="key">Lookup key used to select the value from the collection.</param>
    /// <param name="accessMask"> type of access required.
    /// </param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    PolicyHandle OpenKey(PolicyHandle handle, string key,
        RegKeyAccess accessMask);

    /// <summary>
    /// Closes the key.
    /// </summary>
    /// <param name="handle">Registry policy handle to close.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    void CloseKey(PolicyHandle handle);

    /// <summary>
    /// Query the key for it's name. Please put buffer size more
    /// than the estimated expected value. In this case 1024 would do.
    /// </summary>
    /// <param name="handle">Registry policy handle of the key that contains the queried value.</param>
    /// <param name="bufferSize">
    /// </param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    byte[] QueryValue(PolicyHandle handle, int bufferSize);

    /// <summary>
    /// Query the key-value for it's value.Please put buffer size more than
    /// the estimated expected value.
    /// </summary>
    /// <param name="handle">Registry policy handle of the key that contains the queried value.</param>
    /// <param name="bufferSize">Size of the caller-provided buffer used to receive registry data.</param>
    /// <param name="valueName">Name used to identify the target server, member, or descriptor.</param>
    /// <returns> first param contains the class type as an Integer, second
    /// param contains the value as a 1 dimensional byte array,if any.
    /// In case of REG_MULTI_SZ you will get a 2 dimensional byte array as
    /// the second param. </returns>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    object[] QueryValue(PolicyHandle handle, string valueName, int bufferSize);

    /// <summary>
    ///Creates a new key by name subKey under the handle. If
    /// REG_OPTION_NON_VOLATILE option is used then the key is preserved
    /// in the registry when the machine shutsdown, otherwise it is stored
    /// only in memory.
    /// </summary>
    /// <param name="handle">Registry policy handle for the parent key that will receive the new subkey.</param>
    /// <param name="subKey">Lookup key used to identify the cached or serialized value.</param>
    /// <param name="options">Registry creation options to apply to the new key.</param>
    /// <param name="accessMask">
    /// </param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    PolicyHandle CreateKey(PolicyHandle handle, string subKey,
        RegOption options, RegKeyAccess accessMask);

    /// <summary>
    /// Sets name-value for a REG_MULTI_SZ type. data is a 2 dimensional
    /// array, each primary dimension representing one string.
    /// Please make sure that the encoding is correct.
    /// </summary>
    /// <param name="handle">Registry policy handle of the key whose value should be updated.</param>
    /// <param name="valueName">Name used to identify the target server, member, or descriptor.</param>
    /// <param name="data">Wire-format payload bytes to process.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    void SetValue(PolicyHandle handle, string valueName, byte[][] data);

    /// <summary>
    ///Sets an empty name-value for a REG_NONE type.
    /// </summary>
    /// <param name="handle">Registry policy handle of the key whose value should be updated.</param>
    /// <param name="valueName">Name used to identify the target server, member, or descriptor.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    void SetValue(PolicyHandle handle, string valueName);

    /// <summary>
    /// Sets name-value for a REG_SZ\REG_EXPAND_SZ\REG_BINARY type. The data
    /// will be considered as String if the binary flag is not set to true.
    /// In case of non binary data, please make sure that the encoding is correct
    /// while doing String.getBytes(...). Set expand_sz to true if the String
    /// contains environment variables. When both binary and expand_sz are set,
    /// binary will take precedence.
    /// </summary>
    /// <param name="handle">Registry policy handle of the key whose value should be updated.</param>
    /// <param name="valueName">Name used to identify the target server, member, or descriptor.</param>
    /// <param name="data">Wire-format payload bytes to process.</param>
    /// <param name="binary">Value indicating whether the registry value should be stored as binary data.</param>
    /// <param name="expand_sz">Value indicating whether the registry string should use REG_EXPAND_SZ semantics.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    void SetValue(PolicyHandle handle, string valueName, byte[] data,
        bool binary, bool expand_sz);

    /// <summary>
    /// Sets name-value for a REG_DWORD type.
    /// </summary>
    /// <param name="handle">Registry policy handle of the key whose value should be updated.</param>
    /// <param name="valueName">Name used to identify the target server, member, or descriptor.</param>
    /// <param name="data">Wire-format payload bytes to process.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    void SetValue(PolicyHandle handle, string valueName, int data);

    /// <summary>
    /// Deletes a key or value specified by valueName.
    /// </summary>
    /// <param name="handle">Registry policy handle of the key from which the subkey or value should be deleted.</param>
    /// <param name="valueName">Name used to identify the target server, member, or descriptor.</param>
    /// <param name="isKey">Lookup key used to identify the cached or serialized value.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    void DeleteKeyOrValue(PolicyHandle handle, string valueName,
        bool isKey);

    /// <summary>
    /// Saves registry entries from handle location to local fileName.
    /// This path is local to the target machine.
    /// </summary>
    /// <param name="handle">Registry policy handle of the key to persist to a hive file.</param>
    /// <param name="fileName">Name used to identify the target server, member, or descriptor.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    void SaveFile(PolicyHandle handle, string fileName);

    /// <summary>
    /// Returns name and class (in that order) for the key identified
    /// by index under parent handle.
    /// </summary>
    /// <param name="handle">Registry policy handle of the key whose subkeys should be enumerated.</param>
    /// <param name="index">
    /// </param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    string[] EnumKey(PolicyHandle handle, int index);

    /// <summary>
    ///Returns name and type (in that order) for the value identified
    ///by index under parent handle.
    /// </summary>
    /// <param name="handle">Registry policy handle of the key whose values should be enumerated.</param>
    /// <param name="index">Zero-based index at which the read or write operation begins.</param>
    /// <returns> First is a String (valueName) and second param is
    /// an Integer (type) </returns>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    object[] EnumValue(PolicyHandle handle, int index);

    /// <summary>
    /// Closes this connection, but a word of caution, it does not close
    /// any OPEN Key. Just releases the NP resources it is holding.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    void CloseConnection();
}
