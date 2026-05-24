// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;

namespace Opc.Classic.Dcom.Registry; 
/// <summary>
/// Perform C-R-U-D on the Windows Registry.
/// This interface uses "Windows Remote Registry" and "Server" services
/// and these must be running on target workstation.
/// </summary>
public interface IRegistry {

    /// <summary>
    /// Opens the HKEY_CLASSES_ROOT key
    /// </summary>
    /// <returns> handle representing the opened key </returns>
    /// <exception cref="InteropException"> </exception>
    PolicyHandle OpenHKCR();

    /// <summary>
    /// Opens the HKEY_CURRENT_USER key
    /// </summary>
    /// <returns> handle representing the opened key </returns>
    /// <exception cref="InteropException"> </exception>
    PolicyHandle OpenHKCU();

    /// <summary>
    /// Opens the HKEY_USERS key
    /// </summary>
    /// <returns> handle representing the opened key </returns>
    /// <exception cref="InteropException"> </exception>
    PolicyHandle OpenHKU();

    /// <summary>
    /// Opens the HKEY_LOCAL_MACHINE key
    /// </summary>
    /// <returns> handle representing the opened key </returns>
    /// <exception cref="InteropException"> </exception>
    PolicyHandle OpenHKLM();

    /// <summary>
    /// Opens the subkey of key specified by handle.
    /// </summary>
    /// <param name="handle"> </param>
    /// <param name="key"> </param>
    /// <param name="accessMask"> type of access required.
    /// </param>
    /// <exception cref="InteropException"> </exception>
    PolicyHandle OpenKey(PolicyHandle handle, string key,
        RegKeyAccess accessMask);

    /// <summary>
    /// Closes the key.
    /// </summary>
    /// <param name="handle"> </param>
    /// <exception cref="InteropException"> </exception>
    void CloseKey(PolicyHandle handle);

    /// <summary>
    /// Query the key for it's name. Please put buffer size more
    /// than the estimated expected value. In this case 1024 would do.
    /// </summary>
    /// <param name="handle"> </param>
    /// <param name="bufferSize">
    /// </param>
    /// <exception cref="InteropException"> </exception>
    byte[] QueryValue(PolicyHandle handle, int bufferSize);

    /// <summary>
    /// Query the key-value for it's value.Please put buffer size more than
    /// the estimated expected value.
    /// </summary>
    /// <param name="handle"> </param>
    /// <param name="bufferSize"> </param>
    /// <param name="valueName"> </param>
    /// <returns> first param contains the class type as an Integer, second
    /// param contains the value as a 1 dimensional byte array,if any.
    /// In case of REG_MULTI_SZ you will get a 2 dimensional byte array as
    /// the second param. </returns>
    /// <exception cref="InteropException"> </exception>
    object[] QueryValue(PolicyHandle handle, string valueName, int bufferSize);

    /// <summary>
    ///Creates a new key by name subKey under the handle. If
    /// REG_OPTION_NON_VOLATILE option is used then the key is preserved
    /// in the registry when the machine shutsdown, otherwise it is stored
    /// only in memory.
    /// </summary>
    /// <param name="handle"> </param>
    /// <param name="subKey"> </param>
    /// <param name="options"> </param>
    /// <param name="accessMask">
    /// </param>
    /// <exception cref="InteropException"> </exception>
    PolicyHandle CreateKey(PolicyHandle handle, string subKey,
        RegOption options, RegKeyAccess accessMask);

    /// <summary>
    /// Sets name-value for a REG_MULTI_SZ type. data is a 2 dimensional
    /// array, each primary dimension representing one string.
    /// Please make sure that the encoding is correct.
    /// </summary>
    /// <param name="handle"> </param>
    /// <param name="valueName"> </param>
    /// <param name="data"> </param>
    /// <exception cref="InteropException"> </exception>
    void SetValue(PolicyHandle handle, string valueName, byte[][] data);

    /// <summary>
    ///Sets an empty name-value for a REG_NONE type.
    /// </summary>
    /// <param name="handle"> </param>
    /// <param name="valueName"> </param>
    /// <exception cref="InteropException"> </exception>
    void SetValue(PolicyHandle handle, string valueName);

    /// <summary>
    /// Sets name-value for a REG_SZ\REG_EXPAND_SZ\REG_BINARY type. The data
    /// will be considered as String if the binary flag is not set to true.
    /// In case of non binary data, please make sure that the encoding is correct
    /// while doing String.getBytes(...). Set expand_sz to true if the String
    /// contains environment variables. When both binary and expand_sz are set,
    /// binary will take precedence.
    /// </summary>
    /// <param name="handle"> </param>
    /// <param name="valueName"> </param>
    /// <param name="data"> </param>
    /// <param name="binary"> </param>
    /// <param name="expand_sz"> </param>
    /// <exception cref="InteropException"> </exception>
    void SetValue(PolicyHandle handle, string valueName, byte[] data,
        bool binary, bool expand_sz);

    /// <summary>
    /// Sets name-value for a REG_DWORD type.
    /// </summary>
    /// <param name="handle"> </param>
    /// <param name="valueName"> </param>
    /// <param name="data"> </param>
    /// <exception cref="InteropException"> </exception>
    void SetValue(PolicyHandle handle, string valueName, int data);

    /// <summary>
    /// Deletes a key or value specified by valueName.
    /// </summary>
    /// <param name="handle"> </param>
    /// <param name="valueName"> </param>
    /// <param name="isKey"> </param>
    /// <exception cref="InteropException"> </exception>
    void DeleteKeyOrValue(PolicyHandle handle, string valueName,
        bool isKey);

    /// <summary>
    /// Saves registry entries from handle location to local fileName.
    /// This path is local to the target machine.
    /// </summary>
    /// <param name="handle"> </param>
    /// <param name="fileName"> </param>
    /// <exception cref="InteropException"> </exception>
    void SaveFile(PolicyHandle handle, string fileName);

    /// <summary>
    /// Returns name and class (in that order) for the key identified
    /// by index under parent handle.
    /// </summary>
    /// <param name="handle"> </param>
    /// <param name="index">
    /// </param>
    /// <exception cref="InteropException"> </exception>
    string[] EnumKey(PolicyHandle handle, int index);

    /// <summary>
    ///Returns name and type (in that order) for the value identified
    ///by index under parent handle.
    /// </summary>
    /// <param name="handle"> </param>
    /// <param name="index"> </param>
    /// <returns> First is a String (valueName) and second param is
    /// an Integer (type) </returns>
    /// <exception cref="InteropException"> </exception>
    object[] EnumValue(PolicyHandle handle, int index);

    /// <summary>
    /// Closes this connection, but a word of caution, it does not close
    /// any OPEN Key. Just releases the NP resources it is holding.
    /// </summary>
    /// <exception cref="InteropException"> </exception>
    void CloseConnection();
}
