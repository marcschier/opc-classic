// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Rpc.Core;
using System;

#pragma warning disable MA0051 // OLE Automation type marshaling mirrors the protocol layout.

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Type info
/// </summary>
[Serializable]
internal sealed class TypeInfoImpl : ComObjectImplWrapper, ITypeInfo
{

    /// <summary>
    /// Create implementation
    /// </summary>
    /// <param name="comObject">COM object instance whose exported interfaces are being managed.</param>
    internal TypeInfoImpl(IComObject comObject) :
        base(comObject)
    {
    }

    /// <inheritdoc/>
    public FuncDesc GetFuncDesc(int index)
    {
        var obj = new CallBuilder(true)
        {
            Opnum = 2
        };
        obj.AddInParamAsInt(index);

        // now to prepare out params
        var funcDescStruct = new Struct();
        funcDescStruct.AddMember(typeof(int));
        funcDescStruct.AddMember(new ComPointer(new ComArray(typeof(int), null, 1, true)));
        // first read the pointer representation. Do not want to use funcdesc but only describe
        // it. This should show the flexibility of the API.
        // TODO have to make a Pointer type which only reads the representation.
        obj.AddOutParamAsObject(new ComPointer(funcDescStruct));

        // CLEANLOCALSTORAGE --> this is wrong, since CLEANLOCALSTORAGE is a struct, but it has always
        // come null and even if something comes, I don't know which pointer PVOID stands for.
        var cleanlocalstorage = new Struct();
        cleanlocalstorage.AddMember(typeof(int));
        cleanlocalstorage.AddMember(typeof(int));
        cleanlocalstorage.AddMember(typeof(int));
        obj.AddOutParamAsObject(new ComPointer(cleanlocalstorage));

        // SAFEARRAYBOUNDS
        var safeArrayBounds = new Struct();
        safeArrayBounds.AddMember(typeof(int));
        safeArrayBounds.AddMember(typeof(int));

        // arraydesc
        var arrayDesc = new Struct();
        // typedesc
        var typeDesc = new Struct();

        arrayDesc.AddMember(typeDesc);
        arrayDesc.AddMember(typeof(short));
        arrayDesc.AddMember(new ComArray(safeArrayBounds, new int[] { 1 }, 1, true));

        var forTypeDesc = new Union(typeof(short));
        var ptrToTypeDesc = new ComPointer(typeDesc);
        var ptrToArrayDesc = new ComPointer(arrayDesc);

        forTypeDesc.AddMember(TypeDesc.VT_PTR, ptrToTypeDesc);
        forTypeDesc.AddMember(TypeDesc.VT_SAFEARRAY, ptrToTypeDesc);
        forTypeDesc.AddMember(TypeDesc.VT_CARRAY, ptrToArrayDesc);
        forTypeDesc.AddMember(TypeDesc.VT_USERDEFINED, typeof(int));
        typeDesc.AddMember(forTypeDesc);
        typeDesc.AddMember(typeof(short)); // VARTYPE

        // PARAMDESC
        var paramDesc2 = new Struct();
        paramDesc2.AddMember(typeof(int));
        paramDesc2.AddMember(typeof(Variant));
        var paramDesc = new Struct();
        paramDesc.AddMember(new ComPointer(paramDesc2, false));
        paramDesc.AddMember(typeof(short));

        var elemDesc = new Struct();
        elemDesc.AddMember(typeDesc);
        elemDesc.AddMember(paramDesc);

        funcDescStruct.AddMember(new ComPointer(new ComArray(elemDesc, null, 1, true)));
        funcDescStruct.AddMember(typeof(int));
        funcDescStruct.AddMember(typeof(int));
        funcDescStruct.AddMember(typeof(int));
        funcDescStruct.AddMember(typeof(short));
        funcDescStruct.AddMember(typeof(short));
        funcDescStruct.AddMember(typeof(short));
        funcDescStruct.AddMember(typeof(short));
        funcDescStruct.AddMember(elemDesc);
        funcDescStruct.AddMember(typeof(short));

        var result = ComObject.Call(obj);
        var funcDesc = new FuncDesc((ComPointer)result[0]);
        return funcDesc;
    }

    /// <inheritdoc/>
    public TypeAttr TypeAttr
    {
        get
        {
            var obj = new CallBuilder(true)
            {
                Opnum = 0
            };

            var typeAttr = new Struct();
            var mainPtr = new ComPointer(typeAttr);
            obj.AddOutParamAsObject(mainPtr);

            // CLEANLOCALSTORAGE --> this is wrong, since CLEANLOCALSTORAGE is a struct, but it has always
            // come null and even if something comes, I don't know which pointer PVOID stands for.
            obj.AddOutParamAsObject(new ComPointer(typeof(int)));

            typeAttr.AddMember(typeof(UUID));
            typeAttr.AddMember(typeof(int));
            typeAttr.AddMember(typeof(int));

            typeAttr.AddMember(typeof(int));
            typeAttr.AddMember(typeof(int));

            typeAttr.AddMember(new ComPointer(
                new ComString(null, InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));

            typeAttr.AddMember(typeof(int));
            typeAttr.AddMember(typeof(int));
            typeAttr.AddMember(typeof(short));
            typeAttr.AddMember(typeof(short));
            typeAttr.AddMember(typeof(short));
            typeAttr.AddMember(typeof(short));
            typeAttr.AddMember(typeof(short));
            typeAttr.AddMember(typeof(short));
            typeAttr.AddMember(typeof(short));
            typeAttr.AddMember(typeof(short));

            var typeDesc = new Struct();
            var arrayDesc = new Struct();
            var safeArrayBounds = new Struct();

            safeArrayBounds.AddMember(typeof(int));
            safeArrayBounds.AddMember(typeof(int));

            arrayDesc.AddMember(typeDesc);
            arrayDesc.AddMember(typeof(short));
            arrayDesc.AddMember(new ComArray(safeArrayBounds, new int[] { 1 }, 1, true));

            var forTypeDesc = new Union(typeof(short));
            var ptrToTypeDesc = new ComPointer(typeDesc);
            var ptrToArrayDesc = new ComPointer(arrayDesc);

            forTypeDesc.AddMember(TypeDesc.VT_PTR, ptrToTypeDesc);
            forTypeDesc.AddMember(TypeDesc.VT_SAFEARRAY, ptrToTypeDesc);
            forTypeDesc.AddMember(TypeDesc.VT_CARRAY, ptrToArrayDesc);
            forTypeDesc.AddMember(TypeDesc.VT_USERDEFINED, typeof(int));
            typeDesc.AddMember(forTypeDesc);
            typeDesc.AddMember(typeof(short)); // VARTYPE

            typeAttr.AddMember(typeDesc);
            var paramDesc = new Struct();
            paramDesc.AddMember(new ComPointer(typeof(Variant), false));
            paramDesc.AddMember(typeof(short));

            typeAttr.AddMember(paramDesc);
            var result = ComObject.Call(obj);
            var attr = new TypeAttr((ComPointer)result[0]);
            return attr;
        }
    }

    /// <inheritdoc/>
    public object[] ContainingTypeLib
    {
        get
        {
            var callObject = new CallBuilder(true);
            callObject.AddOutParamAsObject(typeof(IComObject));
            callObject.AddOutParamAsObject(typeof(int));
            callObject.Opnum = 15;
            var result = ComObject.Call(callObject);
            var retVal = new object[2];
            retVal[0] = (ITypeLib)ObjectFactory.NarrowObject((IComObject)result[0]);
            retVal[1] = result[1];
            return retVal;
        }
    }

    /// <inheritdoc/>
    public object[] GetDllEntry(int memberId, int invKind)
    {
        if (invKind != (int)InvokeKind.INVOKE_FUNC &&
            invKind != (int)InvokeKind.INVOKE_PROPERTYGET &&
            invKind != (int)InvokeKind.INVOKE_PROPERTYPUTREF &&
            invKind != (int)InvokeKind.INVOKE_PROPERTYPUT)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(ErrorCode.E_INVALIDARG), nameof(invKind));
        }
        var callObject = new CallBuilder(true);
        callObject.AddInParamAsInt(memberId);
        callObject.AddInParamAsInt(invKind);
        callObject.AddInParamAsInt(1); // refPtrFlags, as per the oaidl.idl...
        callObject.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
        callObject.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
        callObject.AddOutParamAsObject(typeof(short));
        callObject.Opnum = 10;
        return ComObject.Call(callObject);
    }

    /// <inheritdoc/>
    public object[] GetDocumentation(int memberId)
    {
        var callObject = new CallBuilder(true);
        callObject.AddInParamAsInt(memberId);
        callObject.AddInParamAsInt(0xb); // refPtrFlags, as per the oaidl.idl...
        callObject.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
        callObject.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
        callObject.AddOutParamAsObject(typeof(int));
        callObject.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
        callObject.Opnum = 9;
        return ComObject.Call(callObject);
    }

    /// <inheritdoc/>
    public VarDesc GetVarDesc(int index)
    {
        var callObject = new CallBuilder(true)
        {
            Opnum = 3
        };
        callObject.AddInParamAsInt(index);

        // now build the vardesc
        var vardesc = new Struct();
        callObject.AddOutParamAsObject(new ComPointer(vardesc));
        // CLEANLOCALSTORAGE --> this is wrong, since CLEANLOCALSTORAGE is a struct, but it has always
        // come null and even if something comes, I don't know which pointer PVOID stands for.
        var cleanlocalstorage = new Struct();
        cleanlocalstorage.AddMember(typeof(int));
        cleanlocalstorage.AddMember(typeof(int));
        cleanlocalstorage.AddMember(typeof(int));
        callObject.AddOutParamAsObject(new ComPointer(cleanlocalstorage));

        vardesc.AddMember(typeof(int)); // memberid
        vardesc.AddMember(new ComPointer(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));

        var union = new Union(typeof(int));
        union.AddMember(VarDesc.VAR_PERINSTANCE, typeof(int));
        union.AddMember(VarDesc.VAR_DISPATCH, typeof(int));
        union.AddMember(VarDesc.VAR_STATIC, typeof(int));
        union.AddMember(VarDesc.VAR_CONST, typeof(Variant));
        vardesc.AddMember(union);

        var elemDesc = new Struct();

        // SAFEARRAYBOUNDS
        var safeArrayBounds = new Struct();
        safeArrayBounds.AddMember(typeof(int));
        safeArrayBounds.AddMember(typeof(int));

        // arraydesc
        var arrayDesc = new Struct();
        // typedesc
        var typeDesc = new Struct();

        arrayDesc.AddMember(typeDesc);
        arrayDesc.AddMember(typeof(short));
        arrayDesc.AddMember(new ComArray(safeArrayBounds, new int[] { 1 }, 1, true));

        var forTypeDesc = new Union(typeof(short));
        var ptrToTypeDesc = new ComPointer(typeDesc);
        var ptrToArrayDesc = new ComPointer(arrayDesc);

        forTypeDesc.AddMember(TypeDesc.VT_PTR, ptrToTypeDesc);
        forTypeDesc.AddMember(TypeDesc.VT_SAFEARRAY, ptrToTypeDesc);
        forTypeDesc.AddMember(TypeDesc.VT_CARRAY, ptrToArrayDesc);
        forTypeDesc.AddMember(TypeDesc.VT_USERDEFINED, typeof(int));
        typeDesc.AddMember(forTypeDesc);
        typeDesc.AddMember(typeof(short)); // VARTYPE

        // PARAMDESC
        var paramDesc2 = new Struct();
        paramDesc2.AddMember(typeof(int));
        paramDesc2.AddMember(typeof(Variant));
        var paramDesc = new Struct();
        paramDesc.AddMember(new ComPointer(paramDesc2, false));
        paramDesc.AddMember(typeof(short));
        //        <see cref="Struct"/> paramDesc = new <see cref="Struct"/>();
        //        paramDesc.addMember(new <see cref="ComPointer"/>(<see cref="Variant"/>.class,false));
        //        // paramDesc.addMember(<see cref="Variant"/>.class);
        //        paramDesc.addMember(Short.class);

        elemDesc.AddMember(typeDesc);
        elemDesc.AddMember(paramDesc);

        vardesc.AddMember(elemDesc);
        vardesc.AddMember(typeof(short));
        vardesc.AddMember(typeof(int));

        var result = ComObject.Call(callObject);

        return new VarDesc((ComPointer)result[0]);

    }

    /// <inheritdoc/>
    public object[] GetNames(int memberId, int maxNames)
    {
        var callObject = new CallBuilder(true)
        {
            Opnum = 4
        };
        callObject.AddInParamAsInt(memberId);
        callObject.AddInParamAsInt(maxNames);

        callObject.AddOutParamAsObject(new ComArray(
            new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR), null, 1, true, true));
        callObject.AddOutParamAsType(typeof(int));

        return ComObject.Call(callObject);
    }

    /// <inheritdoc/>
    public int GetRefTypeOfImplType(int index)
    {
        var callObject = new CallBuilder(true)
        {
            Opnum = 5
        };
        callObject.AddInParamAsInt(index);
        callObject.AddOutParamAsType(typeof(int));
        return (int)ComObject.Call(callObject)[0];
    }

    /// <inheritdoc/>
    public int GetImplTypeFlags(int index)
    {
        var callObject = new CallBuilder(true)
        {
            Opnum = 6
        };
        callObject.AddInParamAsInt(index);
        callObject.AddOutParamAsType(typeof(int));
        return (int)ComObject.Call(callObject)[0];
    }

    /// <inheritdoc/>
    public ITypeInfo GetRefTypeInfo(int hrefType)
    {
        var callObject = new CallBuilder(true)
        {
            Opnum = 11
        };
        callObject.AddInParamAsInt(hrefType);
        callObject.AddOutParamAsType(typeof(IComObject));
        var result = ComObject.Call(callObject);
        return (ITypeInfo)ObjectFactory.NarrowObject((IComObject)result[0]);
    }

    /// <inheritdoc/>
    public IComObject CreateInstance(string riid)
    {
        var callObject = new CallBuilder(true)
        {
            Opnum = 13
        };
        callObject.AddInParamAsUUID(riid);
        callObject.AddOutParamAsType(typeof(IComObject));
        var result = ComObject.Call(callObject);
        return ObjectFactory.NarrowObject((IComObject)result[0]);
    }

    /// <inheritdoc/>
    public ComString GetMops(int memberId)
    {
        var callObject = new CallBuilder(true)
        {
            Opnum = 14
        };
        callObject.AddInParamAsInt(memberId);
        callObject.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
        var result = ComObject.Call(callObject);
        return (ComString)result[0];
    }
}
