// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Automation;
using System;
using Opc.Classic.Dcom;

namespace Opc.Classic.Dcom.Test;

public class FirstContact_Stub : FirstContact
{

    private readonly ComServer _stub;

    internal Session _session;

    public FirstContact_Stub(string address)
    {
        var arry123 = new ComArray(new sbyte[10][], true);
        var struct123 = new Struct();
        struct123.AddMember(arry123);
        struct123.AddMember(0, 1);
        struct123.AddMember(0, new ComPointer(arry123));

        // ComArray array = new ComArray(new short[]{0});
        Interop.UseAutoRegistration = true;

        // Config.setProperty("SharpCifs.smb.client.domain","ITLINFOSYS");
        _session = Session.CreateSession("FDGNT", "testuser", "QweQwe007");
        // session = Session.createSession("10.74.85.56","itl-hw-38602a\\testuser","Infosys@123");
        // session = Session.createSession("federation","administrator","enterprise");
        // stub = new ComServer(Clsid.ValueOf("8B21775E-717D-11CE-AB5B-D41203C10000"),address,session);
        // stub = new ComServer(ProgId.ValueOf(session,"TestCOM123.TestServer2"),address,session);
        // stub = new ComServer(ProgId.ValueOf(session,"VirtualServer.Application"),address,session);

        // stub = new ComServer(ProgId.ValueOf(session,"ArrayTry.myarray"),address,session);
        _stub = new ComServer(ProgId.ValueOf("ATLDemo.TestSafeArray"), address, _session);
        // stub = new ComServer(ProgId.ValueOf(session,"SafeArrayDemo.SafeArrayTest"),address,session);
        // stub = new ComServer(ProgId.ValueOf(session,"Project1.Class1"),address,session);
        // stub = new ComServer(ProgId.ValueOf(session,"TLI.TLIApplication"),address,session);

        // stub = new ComServer(ProgId.ValueOf(session,"TestSinglePtr.TestSinglePtr2"),address,session);
    }
    //    protected String getSyntax() {
    //        // TODO Auto-generated method stub
    //        // return "e1af8308-5d1f-11c9-91a4-08002b14a0fa:3.0";
    //        return Interfaces.IID_IActivation + ":0.0";
    //    }

    public void ObtainReference()
    {
        try
        {
            //            System.setOut(new PrintStream(new FileOutputStream("c:/temp/testuser.txt")));
            //        } catch (FileNotFoundException e) {
            //            // TODO Auto-generated catch block
            //            e.printStackTrace();
            //        }
            // TODO Auto-generated method stub
            // try {
            // call(Endpoint.IDEMPOTENT,new RemActivation("10000002-0000-0000-0000-000000000001"));
            // IIDSum 10000001-0000-0000-0000-000000000001,ICreate_MyCar 5DD52389-B1A4-4fe7-B131-0F8EF73DD175, IParseDisplayName {0000011A-0000-0000-C000-000000000046}
            // ITestServer 35AF6037-294F-48B2-9B7E-AA8D4885E084
            // IID_ITestServer2 620012E2-69E3-4DC0-B553-AE252524D2F6
            // media player2 20D4F5E0-5475-11D2-9774-0000F80855E6
            // 5E456FAC-D883-416A-B965-25C140C08AEF ITestObject (TestAnotherCOM)
            // 0BBE2D86-D665-4DCC-B9DC-C24F631BDD0E, ITestCOMT4
            // init();
            //



            var unknown = _stub.CreateInstance();
            var dispatch = (IDispatch)ObjectFactory.NarrowObject(unknown.QueryInterface(Interfaces.IID_IDispatch));
            var variants = dispatch.CallMethodA("GetDispatch");

            //            dispatch.callMethodA("TestVariant1", new Object[]{variants[1]} );

            //            Struct struct = new Struct();
            //            struct.addMember(Character.class);
            //            struct.addMember(Double.class);
            //            struct.addMember(new ComString(Flags.FLAG_REPRESENTATION_STRING_BSTR));
            //
            //            Object[] t1 = dispatch.callMethodA("CreateArray", new Object[]{new Variant(10), new Variant(new ComArray(struct,null,1,true),true)} );
            //            Object[] t1 = dispatch.callMethodA("GetFlavorsWithPrices", new Object[]{Variant.EMPTY_BYREF()} );
            //            t1 = dispatch.callMethodA("GetFlavors", new Object[]{Variant.EMPTY_BYREF()} );

            //            String sXmlEncode = "";
            //            for (int i=0; i<10000;i++)
            //                    sXmlEncode = sXmlEncode + "P";
            //
            //            var psXml = new Variant(new ComString(sXmlEncode));
            //            var psError = new Variant(new ComString(""), true);
            //            Object params[] = new Object[] {psXml, psError};

            //            int id = dispatch.getIDsOfNames("testHresult2");
            //           var rt = dispatch.callMethodA("testSafeArrayOfVariants", new Object[]{Variant.EMPTY()_BYREF});




            // Variant t1234 = dispatch.callMethodA("GetStooges");

            // dispatch.callMethod("testArrayOfVariants",new Object[]{new ComArray(new Variant[]{new Variant(new ComArray(new ComString[]{new ComString("ab"),new ComString("cd")}))},true)});
            var handle2 = unknown.QueryInterface("620012E2-69E3-4DC0-B553-AE252524D2F6");
            // var handle3 = (IComObject)unknown.queryInterface(ITypeLib.IID);
            // ComArray arry34 = new ComArray(new Variant[]{new Variant(new ComString("40807810804000300798")),new Variant(new ComString("1"))},true);
            // var c2 = dispatch.callMethodA("Request", new Object[]{new ComString("rtrtr"),new Variant(new Variant(arry34)),Variant.EMPTY()_BYREF,Variant.EMPTY()_BYREF} );
            // Object[] t1 = dispatch.callMethodA("GetFlavorsWithPrices", new Object[]{Variant.EMPTY()_BYREF} );

            var callObject = new CallBuilder
            {
                Opnum = 156
            };
            callObject.AddInParamAsPointer(new ComPointer(new ComArray(new Variant[] { }, true)));
            // callObject.addInParamAsArray(new ComArray(new Variant[]{new Variant(new ComArray(new ComString[]{new ComString("ab"),new ComString("cd")}))},true), Flags.FLAG_NULL);
            var r = handle2.Call(callObject);


            object[] t123 = dispatch.CallMethodA("GetFlavorsWithPrices", new object[] { Variant.CreateEMPTY_BYREF() });
            object[] t12 = dispatch.CallMethodA("GetFlavors", new object[] { Variant.CreateEMPTY_BYREF() });
            // dispatch.callMethodA("testSAFEARRAY01", new Object[]{new Variant(new ComArray(new Integer[]{new Integer(1),new Integer(2),new Integer(4)},true), true)});
            // ComArray arry34 = new ComArray(new Variant[]{new Variant(new ComString("40807810804000300798")),new Variant(new ComString("1"))},true);
            // dispatch.callMethodA("Request", new Object[]{new Integer(8194),arry34,Variant.EMPTY()_BYREF,Variant.EMPTY()_BYREF} );
            object[] ret0 = dispatch.CallMethodA("LongArray", new object[] { new Variant(new ComArray(new int[] { 1, 2, 4 }, true), true) });
            var ret01 = ((Variant)ret0[1]).ObjectAsArray;
            ret0 = dispatch.CallMethodA("ReadAsicRegisterBlock", new object[] { new ComString("Chonap"), new ComString("Cho"), new Variant(new ComArray(new ushort[] { 4000, 4001 }, true), true), new Variant(new ComArray(new uint[] { 9999, 9999 }, true), true), false, true });
            ret01 = ((Variant)ret0[1]).ObjectAsArray;

            ret0 = dispatch.CallMethodA("testSA1", new object[] { new Variant(new ComArray(new bool[] { false, true }, true), true), new Variant(new ComArray(new float[] { 123.4f, 123.4f }, true), true), new Variant(new ComArray(new double?[] { 123.4, new double?(123.4) }, true), true) });
            ret01 = ((Variant)ret0[1]).ObjectAsArray;
            ret01 = ((Variant)ret0[2]).ObjectAsArray;
            ret01 = ((Variant)ret0[3]).ObjectAsArray;
            ret0 = dispatch.CallMethodA("testSA3", new object[] { new Variant(new ComArray(new Variant[] { new Variant(new Variant(dispatch, true)), new Variant(dispatch, true), new Variant(new ComString("Hello")), new Variant(123, true) }, true), true) });
            ret01 = ((Variant)ret0[1]).ObjectAsArray;
            ret0 = dispatch.CallMethodA("testSA2", new object[] { new Variant(new ComArray(new sbyte[] { 1, 1 }, true), true), new Variant(new ComArray(new Variant[] { new Variant(dispatch, true) }, true), true), new Variant(new ComArray(new Variant[] { new Variant(unknown, true) }, true), true) });
            ret01 = ((Variant)ret0[1]).ObjectAsArray;
            ret01 = ((Variant)ret0[2]).ObjectAsArray;
            ret01 = ((Variant)ret0[3]).ObjectAsArray;

            var tr = dispatch.CallMethodA("testHresult2");


            // IComObject handle2 = (IComObject)unknown.queryInterface("FA11DECE-7660-11D2-9C43-006008AD8BC06");

            // IComObject handle2 = (IComObject)unknown.queryInterface("A12E7F85-B011-4AB3-A924-215F67A725D5");

            dispatch.CallMethod("testUnsignedInt", new object[] { (byte)200 });

            var filetime = new Struct();
            filetime.AddMember(typeof(int));
            filetime.AddMember(typeof(int));

            var ONEVENTSTRUCT = new Struct();
            ONEVENTSTRUCT.AddMember(typeof(short));
            ONEVENTSTRUCT.AddMember(typeof(short));
            ONEVENTSTRUCT.AddMember(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR));
            ONEVENTSTRUCT.AddMember(filetime);
            ONEVENTSTRUCT.AddMember(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR));
            ONEVENTSTRUCT.AddMember(typeof(int));
            ONEVENTSTRUCT.AddMember(typeof(int));
            ONEVENTSTRUCT.AddMember(typeof(int));
            ONEVENTSTRUCT.AddMember(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR));
            ONEVENTSTRUCT.AddMember(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR));
            ONEVENTSTRUCT.AddMember(typeof(short));
            ONEVENTSTRUCT.AddMember(typeof(short));
            ONEVENTSTRUCT.AddMember(typeof(int));
            ONEVENTSTRUCT.AddMember(filetime);
            ONEVENTSTRUCT.AddMember(typeof(int));
            ONEVENTSTRUCT.AddMember(typeof(int));
            ONEVENTSTRUCT.AddMember(new ComPointer(typeof(Variant)));
            ONEVENTSTRUCT.AddMember(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR));

            callObject = new CallBuilder();
            //            callObject.setOpnum(3);
            //            callObject.addOutParamAsType(Integer.class, Flags.FLAG_NULL);
            //            callObject.addOutParamAsType(Integer.class, Flags.FLAG_NULL);
            //            callObject.addOutParamAsType(Integer.class, Flags.FLAG_NULL);
            //            callObject.addOutParamAsType(Integer.class, Flags.FLAG_NULL);
            //            callObject.addOutParamAsObject(new ComArray(ONEVENTSTRUCT,null,1,true), Flags.FLAG_NULL);
            //            handle2.call(callObject);



            // Long
            // Short
            // Integer
            callObject.ReInit();
            callObject.Opnum = 147;
            callObject.AddInParamAsUnsigned(200);
            handle2.Call(callObject);

            var aIn = new ComArray(new Variant[] { new Variant(new ComString("40807810804000300798")), new Variant(new ComString("1")) }, true);
            var varArray = new Variant(aIn);

            callObject.Opnum = 3;

            var vOpt = new Variant(8194);

            //            callObject.addInParamAsInt(8194,Flags.FLAG_NULL);
            callObject.AddInParamAsShort(8194);
            //            callObject.addInParamAsVariant(vOpt,Flags.FLAG_NULL);
            callObject.AddInParamAsVariant(varArray);
            callObject.AddInParamAsVariant(Variant.CreateEMPTY_BYREF());
            callObject.AddInParamAsVariant(Variant.CreateEMPTY_BYREF());
            //         callObject.addInParamAsVariant(vOut,Flags.FLAG_NULL);
            //         callObject.addInParamAsVariant(vExc,Flags.FLAG_NULL);

            callObject.AddOutParamAsType(typeof(Variant));
            callObject.AddOutParamAsType(typeof(Variant));
            callObject.AddOutParamAsType(typeof(int));

            var t = handle2.Call(callObject);

            // since this is a byRef (check using the isByReflagSet())
            var arrt = ((Variant)t[0]).ObjectAsVariant.ObjectAsArray;
            Console.WriteLine(arrt);

            callObject.Opnum = 16;

            callObject.AddInParamAsPointer(new ComPointer(new ComString("123", InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
            callObject.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR));
            var t2 = handle2.Call(callObject);

            callObject.ReInit();
            callObject.Opnum = 143;

            callObject.AddInParamAsString("123", InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR);
            callObject.AddInParamAsBoolean(true);
            callObject.AddInParamAsInt(10);
            callObject.AddInParamAsInt(20);
            callObject.AddInParamAsInt(10);
            callObject.AddInParamAsFloat(20);

            //            callObject.addInParamAsPointer ( new ComPointer(new Integer(10)),Flags.FLAG_NULL );
            //            callObject.addInParamAsPointer ( new ComPointer(new Float(20.2)),Flags.FLAG_NULL );
            callObject.AddInParamAsInt(0x800);
            callObject.AddOutParamAsObject(new ComPointer(typeof(int), false));
            callObject.AddOutParamAsObject(new ComPointer(typeof(int), false));
            callObject.AddInParamAsUUID("620012E2-69E3-4DC0-B553-AE252524D2F6");
            callObject.AddOutParamAsType(typeof(IComObject));

            t2 = handle2.Call(callObject);


            //            Variant variantDate = new Variant(new Date(),true);
            //            callObject.addInParamAsVariant(variantDate,Flags.FLAG_NULL);
            //            callObject.addOutParamAsType(Variant.class,Flags.FLAG_NULL);
            //            callObject.setOpnum(2);
            //            Object[] t = handle2.call(callObject);
            //            Date date = ((Variant)t[0]).getObjectAsDate();
            //
            //            callObject = new CallBuilder(handle2.getIpid());
            //            callObject.addInParamAsVariant(Variant.EMPTY()_BYREF,Flags.FLAG_NULL);
            //            callObject.addOutParamAsType(Variant.class,Flags.FLAG_NULL);
            //            callObject.setOpnum(1);
            //             t = handle2.call(callObject);
            //            Variant ref = (Variant)t[0];
            //            ComArray tr = ref.getObjectAsArray();


            //    Object[] t1 = dispatch.callMethodA("GetFlavorsWithPrices", new Object[]{Variant.EMPTY()_BYREF} );


            // ComArray arry34 = new ComArray(new ComString[]{new ComString("40807810804000300798"),new ComString("1")},true);
            // Variant[] c = dispatch.callMethodA("Request", new Object[]{new Integer(8194),new Variant(new Variant(arry34)),Variant.EMPTY()_BYREF,Variant.EMPTY()_BYREF} );
            // Variant[] c = dispatch.callMethodA("Request", new Object[]{new Integer(8194),arry34,Variant.EMPTY()_BYREF,Variant.EMPTY()_BYREF} );
            // ComArray arrtt = (c[2]).getObjectAsVariant().getObjectAsArray();
            // System.out.println(arrtt);

            var handle = unknown.QueryInterface("620012E2-69E3-4DC0-B553-AE252524D2F6");
            var callObject2 = new CallBuilder();



            // Variant variantwe = new Variant();
            // Variant[] rett = dispatch.callMethodA("GetFlavors", new Object[]{Variant.EMPTY()});




            // handle.addRef();
            // handle.release();



            //            dispatch.put("TestProperty1", new Object[]{new Short((short)1), new ComString("Hello")});
            //        dispatch.put("TestProperty2", new Object[]{new Short((short)1), new Short((short)2), new Integer(3)});

            var bhalue = dispatch.Get("TestProperty1", new object[] { (short)1 });
            bhalue = dispatch.Get("TestProperty2", new object[] { (short)1, (short)2 });

            var typeInfo = dispatch.GetTypeInfo(0);
            var funcDesc = typeInfo.GetFuncDesc(0);
            var re = typeInfo.GetNames(funcDesc.memberId, 100);
            var arry = typeInfo.GetDocumentation(funcDesc.memberId);
            var mops = typeInfo.GetMops(funcDesc.memberId);
            // int[] ids = typeInfo.getIdOfNames(new String[]{"QueryInterface"});
            // IUnknown unknown2 = typeInfo.createInstance(Interfaces.IID_IDispatch);
            var hrefType = typeInfo.GetRefTypeOfImplType(0);
            var info = typeInfo.GetRefTypeInfo(hrefType);
            // int implTypeFlags = typeInfo.getImplTypeFlags(1);
            // VarDesc varDesc = typeInfo.getVarDesc(0);
            var typeLib = (ITypeLib)typeInfo.ContainingTypeLib[0];
            var type = typeLib.TypeInfoCount;
            typeLib.GetLibAttr();
            typeLib.FindName(new ComString("QueryInterface", InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR), 0, 1);
            // Object[] ry = typeLib.getDocumentation(funcDesc.memberId);
            type = typeLib.GetTypeInfoType(0);
            // ITypeInfo type2 = typeLib.getTypeInfo(type);
            // typeInfo.getDllEntry(funcDesc.memberId, InvokeKind.INVOKE_FUNC.intValue());
            // typeInfo.getTypeAttr();


            //            IJMeowWrapper handle = queryInterface("0BBE2D86-D665-4DCC-B9DC-C24F631BDD0E",false);
            // if (handle.isIDispatchSupported())
            {
                //                // System.out.println(handle.getDispatch().GetTypeInfoCount());
                //                dispatch = (IDispatch)ObjectFactory.createCOMInstance(ObjectFactory.IID_IDispatch,(IComObject)unknown.queryInterface(IDispatch.IID,false));
                //                int i = dispatch.GetIDsOfNames("testSA");
                //                // int i = dispatch.GetIDsOfNames("testAllVARIANTS");
                //                // ITypeInfo type = dispatch.GetTypeInfo(0);
                //                // flags are going to be defined in IDispatch
                //                Variant variant = new Variant(new Object[]{new ComString("Hi")});
                //                dispatch.invoke(i,1,new Object[]{variant,null,null,null,null},new Object[]{Variant.class});
                //                // Variant params = new Variant(new Object[]{new Integer(10),new ComString("123456")});
                //                // dispatch.invoke(i,1,new Object[]{params,null,new Integer(2),new Integer(0)},new Object[]{Variant.class});
            }

            var obj = new CallBuilder();
            //            obj.setOpnum(13);// 31);// 30);// 29);// 32);
            object[] result = null;
            //
            //
            //            obj.reInit();
            //            obj.setOpnum(2);
            //            obj.addInParamAsVariant(new Variant(Variant.SCODE,0x80020004), Flags.FLAG_NULL);
            //            // obj.setUpParams(new Object[]{new Variant(new Object[]{new Character('S'),new Integer(12),handle,dispatch,new Double(12.23),new Float(101),new Float(101),new Double(12.23)})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
            //            result = handle.call(obj);
            ////
            //            obj.reInit();
            //            obj.setOpnum(2);
            //            obj.addInParamAsVariant(new Variant(Variant.NULL), Flags.FLAG_NULL);
            //            // obj.setUpParams(new Object[]{new Variant(new Object[]{new Character('S'),new Integer(12),handle,dispatch,new Double(12.23),new Float(101),new Float(101),new Double(12.23)})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
            //            result = handle.call(obj);

            //            obj.reInit();
            //            obj.setOpnum(93);
            //            obj.addInParamAsString("testuser",Flags.FLAG_REPRESENTATION_STRING_LPCTSTR);
            //            obj.addInParamAsString("ShilpaAkshat",Flags.FLAG_REPRESENTATION_STRING_LPWSTR);
            //
            //            // obj.setUpParams(new Object[]{new Variant(new Object[]{new Character('S'),new Integer(12),handle,dispatch,new Double(12.23),new Float(101),new Float(101),new Double(12.23)})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
            //            result = handle.call(obj);
            //
            // Variant variant = new Variant(new ComString("4567"));
            //            Variant variant = new Variant(Variant.NULL);
            //            Variant variant2 = new Variant(new Integer(10));
            //            dispatch.callMethod("test3variants",new Object[]{variant,variant,variant,variant2});

            //            obj.reInit();
            //            obj.setOpnum(4);
            //            Variant variant3 = new Variant(new Object[]{new Variant(new Variant(123.234)),Integer.ValueOf(100)});
            //            obj.addInParamAsVariant(variant3,Flags.FLAG_NULL);
            //            // obj.addOutParamAsType(Variant.class,Flags.FLAG_NULL);
            //            result = handle.call(obj);

            //            obj.reInit();
            //            obj.setOpnum(9);
            //            Variant variant =  new Variant(new ComString("4567"));
            //            Variant variant2 =  new Variant(handle);
            //            ComArray array = new ComArray(new Variant[]{variant,variant2});
            //            obj.addInParamAsArray(array,Flags.FLAG_NULL);
            //            // obj.addOutParamAsType(Variant.class,Flags.FLAG_NULL);
            //            result = handle.call(obj);

            //            obj.reInit();
            //            obj.setOpnum(4); // 4
            //            Variant variant =  new Variant(dispatch,true);
            //            Variant variant2 =  new Variant(variant);
            //            obj.addInParamAsVariant(variant,Flags.FLAG_NULL);
            //            obj.addInParamAsInt(10,Flags.FLAG_NULL);
            //            obj.addInParamAsVariant(variant2,Flags.FLAG_NULL);
            //            // obj.addOutParamAsType(Variant.class,Flags.FLAG_NULL);
            //            result = handle.call(obj);
            //
            //            obj.reInit();
            //            obj.setOpnum(98); // 4
            //            variant =  new Variant(dispatch,true);
            //            variant2 =  new Variant(true);
            //            obj.addInParamAsVariant(variant,Flags.FLAG_NULL);
            //            obj.addInParamAsInt(10,Flags.FLAG_NULL);
            //            obj.addInParamAsVariant(variant2,Flags.FLAG_NULL);
            //            // obj.addOutParamAsType(Variant.class,Flags.FLAG_NULL);
            //            result = handle.call(obj);

            //            obj.reInit();
            //            obj.setOpnum(46); // 4
            //            Variant variant = new Variant(new Variant(handle,true));
            //            Variant variant2 = new Variant(new Variant(new Variant(dispatch)));
            //
            //    //        variant = new Variant(Variant.EMPTY());
            //    //        variant2 =  new Variant(Variant.EMPTY());
            //
            //            obj.addInParamAsVariant(variant,Flags.FLAG_NULL);
            //            obj.addInParamAsShort((short)10,Flags.FLAG_NULL);
            //            // obj.addInParamAsPointer(new ComPointer(Short.ValueOf((short)10)),Flags.FLAG_NULL);
            //            obj.addInParamAsVariant(variant2,Flags.FLAG_NULL);
            //            obj.addOutParamAsType(Variant.class,Flags.FLAG_NULL);
            //            obj.addOutParamAsObject(new ComPointer(Short.class,true),Flags.FLAG_NULL);
            //            obj.addOutParamAsType(Variant.class,Flags.FLAG_NULL);
            //            result = handle.call(obj);

            obj.ReInit();
            obj.Opnum = 49;
            obj.AddInParamAsPointer(new ComPointer(new ComPointer(100)));
            handle.Call(obj);

            obj.ReInit();
            obj.Opnum = 53;
            obj.AddInParamAsPointer(new ComPointer(100));
            handle.Call(obj);


            obj.ReInit();
            obj.Opnum = 134;
            obj.AddInParamAsComObject(dispatch);
            handle.Call(obj);

            obj.ReInit();
            obj.Opnum = 135;
            obj.AddInParamAsComObject(dispatch);
            obj.AddOutParamAsType(typeof(IComObject));
            handle.Call(obj);

            obj.ReInit();
            obj.Opnum = 136;
            obj.AddInParamAsComObject(dispatch);
            handle.Call(obj);

            obj.ReInit();
            obj.Opnum = 137;
            obj.AddInParamAsString("Hello", InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
            obj.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
            handle.Call(obj);

            object[] ret = dispatch.CallMethodA("testBSTR01", new object[] { new Variant(new ComString("Hello"), true) });

            obj.ReInit();
            obj.Opnum = 138;
            obj.AddInParamAsString("Hello", InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
            handle.Call(obj);

            //             ret = dispatch.callMethodA("testBSTR02",new Object[]{new Variant(new ComString("Hello"),true)});

            obj.ReInit();
            obj.Opnum = 139;
            obj.AddInParamAsPointer(new ComPointer(new ComString("Hello")));
            handle.Call(obj);

            //        ret = dispatch.callMethodA("testBSTR03",new Object[]{new Variant(new ComString("Hello"),true)});

            // dispatch.callMethod("testIntPtr3D1",new Object[]{new Variant(100,true)});
            var array = new ComArray(new object[][]
            {
                new object[] {DateTime.Now},
                new object[] {handle},
                new object[] {handle},
                new object[] {handle}
            });

            var variant3 = dispatch.CallMethodA("testSA", new object[] { new Variant(array, true) });
            var array2 = variant3[1].ObjectAsArray;

            object[][] newValue = {
                 new object[] {new ComString("defe"), false, 98765.0 / 12345.0},
                 new object[] {DateTime.Now, 5454, 22.0 / 7.0},
                 new object[] { true, new ComString("dffe"), DateTime.Now}
             };
            // Variant variant2[] = dispatch.callMethodA(0x82,new Object[]{new Variant(new ComArray(new Integer[]{Integer.ValueOf(100),Integer.ValueOf(100),Integer.ValueOf(200)}),true)});
            // variant2[1].getObjectAsArray();
            var variant2 = dispatch.CallMethodA("testSA", new object[] { new Variant(new ComArray(newValue), true) });
            variant2 = dispatch.CallMethodA("testSAFEARRAY01", new object[] { new Variant(new ComArray(new int[] { 100, 100, 200 }, true), true) });
            // variant2[1].ObjectAsArray;

            dispatch.CallMethod(3, new object[] { new Variant(new Currency(-1, 0), true) });
            var variant = dispatch.CallMethodA("testSA", new object[] { new Variant(new Variant(dispatch, true)) });
            variant = dispatch.CallMethodA("testSA", new object[] { new Variant(Scode.Ok, true) });
            variant = dispatch.CallMethodA("testSA", new object[] { new Variant(Variant.CreateNULL()) });
            variant = dispatch.CallMethodA("testSA", new object[] { new Variant(dispatch, true) });
            variant = dispatch.CallMethodA("testSA", new object[] { new Variant(true, true) });
            // Variant[] variant = dispatch.callMethodA("test3variants",new Object[]{new Variant(100,true),new Variant(400,true),new Variant(300,true),new Integer(200)});
            // Variant[] variant  = dispatch.callMethodA("testSA",new Object[]{new Variant(new ComString("Qweqrt2e"),true)});
            var variant11 = dispatch.CallMethodA("testSA", new object[] { new Variant(new ComString("Qwertweer"), true) });
            var variant111 = dispatch.CallMethodA("testSA", new object[] { new Variant(new ComString("2qe4twreggwfgwdfgwdgfssdgwegwertgwertwweQA"), true) });
            var variant222 = dispatch.CallMethodA("testSA", new object[] { new Variant(new ComString("Q4624twegewgA"), true) });
            var variant333 = dispatch.CallMethodA("testSA", new object[] { new Variant(new ComString("ABdfgfdgdgdgfdgfC"), true) });
            var variant444 = dispatch.CallMethodA("testSA", new object[] { new Variant(new ComString("ABdfggdgdgfdgfgfdfgdfgdgfdgfC"), true) });
            var variant555 = dispatch.CallMethodA("testSA", new object[] { new Variant(new ComString("ABCDEFGH"), true) });
            var variant4 = dispatch.CallMethodA("testVariants678", new object[] { new Variant(100), new Variant(true), new Variant(100, true) });
            variant = dispatch.CallMethodA("testSA", new object[] { new Variant(100, true) });

            // this is failing as well...variant within a variant

            // ....

            var array3 = variant4[1].ObjectAsArray;

            variant = dispatch.CallMethodA("testSA", new object[] { new Variant(DateTime.UtcNow, true) });
            variant2 = dispatch.CallMethodA("testSAFEARRAY01", new object[] { new Variant(new ComArray(new int[] { 100, 100, 200 })) });
            // variant2[1].getObjectAsArray();
            // dispatch.callMethod(0x82,new Object[]{new Variant(new Boolean[]{Boolean.TRUE})});
            // dispatch.callMethod(3,new Object[]{new Variant(new Variant(new Variant(handle)))});
            // dispatch.callMethod(3,new Object[]{new Variant(new Date(System.currentTimeMillis()))});
            //    dispatch.callMethod(3,new Object[]{new Variant(new Currency(10,0))});
            dispatch.CallMethod(3, new object[] { new Variant(new Currency(-1, 0), true) });

            // dispatch.callMethod(3,new Object[]{new Variant(true,true)});

            // dispatch.callMethod(0x64,new Object[]{new Variant(dispatch),new Variant(true,true)});

            // Just testing
            obj.ReInit();
            obj.Opnum = 4;

            var interfaceDefinition = new LocalInterfaceDefinition("620012E2-69E3-4DC0-B553-AE252524D2F6");
            var component = new LocalCoClass(interfaceDefinition, typeof(Test));
            var runtimeObject = new LocalParamsDescriptor();
            var methodDescriptor = new LocalMethodDescriptor("test", 1, runtimeObject);
            interfaceDefinition.AddMethodDescriptor(methodDescriptor);

            var objMyCOM = ObjectFactory.BuildObject(_session, component);
            obj.AddInParamAsVariant(new Variant(objMyCOM));
            obj.AddOutParamAsType(typeof(Variant));
            result = handle.Call(obj);

            //            obj.reInit();
            //            obj.setOpnum(4);
            //            obj.addInParamAsVariant(new Variant(handle),Flags.FLAG_NULL);
            //            // obj.setUpParams(new Object[]{new Variant(new Object[]{new Character('S'),new Integer(12),handle,dispatch,new Double(12.23),new Float(101),new Float(101),new Double(12.23)})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
            //            result = handle.call(obj);

            //            obj.reInit();
            //            obj.setOpnum(95);
            //            obj.addInParamAsPointer(new ComPointer(new ComString("testuser",Flags.FLAG_REPRESENTATION_STRING_LPCTSTR)), Flags.FLAG_NULL);
            //            obj.addInParamAsPointer(new ComPointer(new ComString("AkshatShilpa",Flags.FLAG_REPRESENTATION_STRING_LPCTSTR)), Flags.FLAG_NULL);
            //            // obj.setUpParams(new Object[]{new Variant(new Object[]{new Character('S'),new Integer(12),handle,dispatch,new Double(12.23),new Float(101),new Float(101),new Double(12.23)})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
            //            result = handle.call(obj);

            //            obj.reInit();
            //            obj.setOpnum(3);
            //            obj.setUpParams(new Object[]{new Variant(new Object[]{new Character('S'),new Integer(12),handle,dispatch,new Double(12.23),new Float(101),new Float(101),new Double(12.23)})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
            //            result = handle.call(obj);
            //
            //            obj.reInit();
            //            obj.setOpnum(3);
            //            obj.addInParamAsVariant(new Variant(new ComString("123456789qwertyuiop")),Flags.FLAG_NULL);
            //
            //            result = handle.call(obj);

            //            obj.reInit();
            //            obj.setOpnum(94);
            //            out = new Object[]{new Pointer(new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPCTSTR)),new Pointer(new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPCTSTR))};
            //            obj.setUpParams(new Object[]{new Pointer(null),new Pointer(null)}, out,Flags.FLAG_REPRESENTATION_STRING_BSTR,Flags.FLAG_NULL);
            //            result = handle.call(obj);



            // obj.setUpParams(new Object[]{new Float[]{new Float(50),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60),new Float(60)}}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
            //            Float[] array = new Float[10];
            //            array[0] = new Float(10.00);
            //            array[1] = new Float(20.00);
            //            Double[] array = new Double[10];
            //            array[0] = new Double(10.3030303);
            //            array[1] = new Double(20.3030303);
            //            array[2] = new Double(30.3030303);
            //            Short[] array = new Short[10];
            //            array[0] = new Short((short)10);
            //            array[1] = new Short((short)20);
            //            array[2] = new Short((short)30);
            //            Boolean[] array = new Boolean[10];
            //            array[0] = Boolean.TRUE;
            //            array[9] = Boolean.TRUE;
            //            Integer[][] array = new Integer[2][2];
            //            obj.setOpnum(4);
            //            array[0][0] = new Integer(10);
            //            array[0][1] = new Integer(20);

            //            Double[][] array = new Double[3][5];
            //            array[0][0] = new Double(10.3030303);
            //            array[1][0] = new Double(20.3030303);
            //            array[2][0] = new Double(30.3030303);
            //            Float[][][] array = new Float[10][3][7];
            //            array[0][0][6] = new Float(10);
            //            array[1][0][6] = new Float(20);
            //            array[2][0][4] = new Float(30);

            /*        Boolean array[][][][] = new Boolean[1][2][1][2];
                    obj.setOpnum(7);
                    array[0][1][0][0] = Boolean.TRUE;
                    array[0][1][0][1] = Boolean.TRUE;
                    array[0][0][0][1] = Boolean.TRUE;
                    obj.setUpParams(new Object[]{new ComArray(array)}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        ////
        //
                    obj.reInit();
                    obj.setOpnum(11);
                    in = new Object[]{new ComArray(new Integer[10]),new ComArray(new Float[3]),new ComArray(new Double[5]),new ComArray(new Short[10][5])};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(9);
                    in = new Object[]{new Integer(10),new Variant(new Short((short)10))};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{new ComString(null)};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
        //
                    obj.reInit();
                    obj.setOpnum(19);
                     in = new Object[]{new Variant(new Integer(5)),new Variant(new ComString("wfwre")),new Variant(new ComString("wfwre")), new Integer(10)};// new Variant(new ComString("Mangoes"))};
                     out = null;// new Object[]{new ComString(null)};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(19);
                    in = new Object[]{new Variant(null),new Variant(null),new Variant(null), new Integer(0)};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{Variant.class,Variant.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(19);
                    in = new Object[]{new Variant(null),new Variant(null),new Variant(null), new Integer(1)};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{Variant.class,Variant.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(19);
                    in = new Object[]{new Variant(null),new Variant(null),new Variant(null), new Integer(2)};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{Variant.class,Variant.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(19);
                    in = new Object[]{new Variant(null),new Variant(null),new Variant(null), new Integer(3)};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{Variant.class,Variant.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(19);
                    in = new Object[]{new Variant(null),new Variant(null),new Variant(null), new Integer(4)};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{Variant.class,Variant.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(19);
                    in = new Object[]{new Variant(null),new Variant(null),new Variant(null), new Integer(5)};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{Variant.class,Variant.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(19);
                    in = new Object[]{new Variant(null),new Variant(null),new Variant(null), new Integer(6)};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{Variant.class,Variant.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(19);
                    in = new Object[]{new Variant(null),new Variant(null),new Variant(null), new Integer(7)};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{Variant.class,Variant.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(19);
                    in = new Object[]{new Variant(null),new Variant(null),new Variant(null), new Integer(8)};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{Variant.class,Variant.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(19);
                    in = new Object[]{new Variant(null),new Variant(null),new Variant(null), new Integer(9)};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{Variant.class,Variant.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(19);
                    in = new Object[]{new Variant(null),new Variant(null),new Variant(null), new Integer(10)};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{Variant.class,Variant.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(18);
                    in = new Object[]{new Variant(null),new Variant(null)};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{Variant.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
                    int k = 0;

                    obj.reInit();
                    obj.setOpnum(23);
                    obj.setUpParams(new Object[]{ Boolean.TRUE,new Integer(10)}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(21);
                    obj.setUpParams(new Object[]{Boolean.TRUE,new Variant(new ComString("12"))}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(28);
                    obj.setUpParams(new Object[]{new Integer("10"),new Double("10"),new Variant(null)}, new Object[]{Variant.class},Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(24);
                    obj.setUpParams(new Object[]{new Integer(10),new Variant(new Double(10)),new Variant(new Integer("123"))}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(22);
                    obj.setUpParams(new Object[]{new Short((short)123),new Variant(new Integer("12"))}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(20);
                    obj.setUpParams(new Object[]{new Variant(new ComString("12")),new Integer(10)}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(27);
                    obj.setUpParams(new Object[]{new Integer(10),Boolean.TRUE,new Variant(new ComString("12")),new Integer(1000),Boolean.FALSE,new Variant(new Integer("12")),new Variant(new Double("12")),new Variant(Boolean.TRUE),new Variant(Boolean.FALSE)}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(26);
                    obj.setUpParams(new Object[]{new Integer(10),Boolean.TRUE,new Variant(null),new Variant(new Double("12")),new Variant(Boolean.TRUE)},new Object[]{Variant.class,Variant.class,Variant.class},Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(3);  // TODO screwed
                    obj.setUpParams(new Object[]{new Variant(new ComString("12"))}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(12);
                     in = new Object[]{handle,handle.getDispatch()}; // TODO screwed
                     out = new Object[]{MInterfacePointer.class,MInterfacePointer.class};
                    obj.setUpParams(in, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(13);
                     in = new Object[]{new Integer(0),new Float(0),new Double(0),new ComString(null,Flags.FLAG_REPRESENTATION_STRING_BSTR),new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPCTSTR),new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPWSTR),new Short((short)0)};
                     out = new Object[]{Integer.class,Float.class,Double.class,new ComString(null,Flags.FLAG_REPRESENTATION_STRING_BSTR),new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPCTSTR),new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPWSTR),Short.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
                    int j = 0;

                    obj.reInit();
                    obj.setOpnum(14);
                     in = new Object[]{new ComString("qwe",Flags.FLAG_REPRESENTATION_STRING_BSTR)};
                     out = new Object[]{new ComString(null,Flags.FLAG_REPRESENTATION_STRING_BSTR)};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        ////
        //
                    obj.reInit();
                    obj.setOpnum(15);
                     in = new Object[]{new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPCTSTR)};
                     out = new Object[]{new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPCTSTR)};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(16);
                     in = new Object[]{new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPWSTR)};
                     out = new Object[]{new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPWSTR)};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //


                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new Object[]{new Character('S'),new Integer(12),handle,handle.getDispatch(),new Double(12.23),new Float(101),new Float(101),new Double(12.23)})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);


        //
        //            // GenericObject obj = new GenericObject(handle);
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(handle)}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(handle.getDispatch())}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new ComString("123456789qwertyuiop"))}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        ////
                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new Integer(100))}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new Float(100.07))}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new Double(100))}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(Boolean.TRUE)}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new Character('S'))}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

        //
                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new Short((short)100))}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new Short[]{new Short((short)100),new Short((short)100)})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new Character[]{new Character('s'),new Character('s'),new Character('s'),new Character('s'),new Character('s')})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new Boolean[]{Boolean.TRUE,Boolean.TRUE,Boolean.TRUE,Boolean.TRUE,Boolean.TRUE,Boolean.TRUE,Boolean.TRUE,Boolean.TRUE,Boolean.TRUE,Boolean.TRUE})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //// //
                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new Float[]{new Float(101),new Float(102),new Float(103),new Float(10),new Float(10),new Float(10),new Float(10),new Float(10),new Float(10),new Float(1032)})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new Double[]{new Double(10),new Double(10),new Double(10),new Double(10),new Double(10),new Double(10),new Double(103232),new Double(101123434)})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new Integer[]{new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10),new Integer(10)})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        ////
                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new ComString[]{new ComString("ABCDEF"),new ComString("ABCDEF"),new ComString("ABCDEF"),new ComString("ABCDEF"),new ComString("ABCDEF"),new ComString("ABCDEF"),new ComString("ABCDEF"),new ComString("ABCDEF"),new ComString("ABCDEF"),new ComString("ABCDEF"),new ComString("ABCDEF"),new ComString("ABCDEF"),new ComString("ABCDEF"),new ComString("ABCDEF"),new ComString("ABCDEF")})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);


                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new JMeowWrapperImpl[]{(JMeowWrapperImpl)handle,(JMeowWrapperImpl)handle,(JMeowWrapperImpl)handle,(JMeowWrapperImpl)handle,(JMeowWrapperImpl)handle,(JMeowWrapperImpl)handle,(JMeowWrapperImpl)handle,(JMeowWrapperImpl)handle,(JMeowWrapperImpl)handle,(JMeowWrapperImpl)handle,(JMeowWrapperImpl)handle})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(3);
                    obj.setUpParams(new Object[]{new Variant(new DispatchImpl[]{(DispatchImpl)handle.getDispatch(),(DispatchImpl)handle.getDispatch(),(DispatchImpl)handle.getDispatch(),(DispatchImpl)handle.getDispatch(),(DispatchImpl)handle.getDispatch(),(DispatchImpl)handle.getDispatch()})}, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //

                    obj.reInit();
                    obj.setOpnum(29);
                    in = new Object[]{Boolean.TRUE,new Double(0),new Variant(new ComString("He"))};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(28);
                    in = new Object[]{new Integer(0),new Double(0),new Variant(new ComString("He"))};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(28);
                    in = new Object[]{new Integer(0),new Double(0),new Variant(null)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);


                    obj.reInit();
                    obj.setOpnum(18);
                    in = new Object[]{new Variant(null),new Variant(null)};// new Variant(new ComString("Mangoes"))};
                    out = new Object[]{Variant.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(17);
                    in = new Object[]{new ComArray(new Integer[][]{{new Integer(1),new Integer(2)},{new Integer(3),new Integer(4)}})};
                    out = null;// new Object[]{new ComArray(Integer.class,new int[]{2},1)};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(44);
                    in = new Object[]{new ComArray(new Short[3][3])};
                    out = null;// new Object[]{new ComArray(Integer.class,new int[]{2},1)};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(43);
                    in = new Object[]{new ComArray(new Integer[2][2][3])};
                    out = null;// new Object[]{new ComArray(Integer.class,new int[]{2},1)};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);


                    obj.reInit();
                    obj.setOpnum(40);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(30),new Integer(40),new Integer(50)})};
                    out = null;// new Object[]{new ComArray(Integer.class,new int[]{2},1)};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(41);
                    in = new Object[]{new ComArray(new Integer[][]{{new Integer(1),new Integer(1),new Integer(1)},{new Integer(1),new Integer(70),new Integer(1)},{new Integer(90),new Integer(100),new Integer(110)}})};
                    out = null;// new Object[]{new ComArray(Integer.class,new int[]{2},1)};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(46);
                    in = new Object[]{new ComArray(new Integer[][]{{new Integer(1),new Integer(1),new Integer(1)},{new Integer(50),new Integer(1),new Integer(60)}})};
                    out = null;// new Object[]{new ComArray(Integer.class,new int[]{2},1)};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(47);
                    in = new Object[]{new ComArray(new Integer[][]{{new Integer(1),new Integer(1),new Integer(1)},{new Integer(1),new Integer(1),new Integer(50)},{new Integer(1),new Integer(60),new Integer(1)},{new Integer(70),new Integer(1),new Integer(80)}})};
                    out = null;// new Object[]{new ComArray(Integer.class,new int[]{2},1)};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(45);
                    in = new Object[]{new Variant(null),new Short((short)10),new Variant(null)};
                    out = new Object[]{Variant.class,Short.class,Variant.class};
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

            /*
                    obj.reInit();
                    obj.setOpnum(48);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(1),new Integer(1),new Integer(10)})};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(49);
                    in = new Object[]{new ComArray(new Integer[4])};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(50);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(6),new Integer(1),new Integer(1),new Integer(9),new Integer(1),new Integer(1),new Integer(12)})};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(51);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(1),new Integer(2),new Integer(3),new Integer(4),new Integer(5),new Integer(6),new Integer(7),new Integer(8),new Integer(9),new Integer(10),new Integer(11),new Integer(12),new Integer(13),new Integer(14),new Integer(15),new Integer(16)})};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(52);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(1),new Integer(2)})};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(53);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(1),new Integer(0),new Integer(10000),new Integer(10000)})};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(54);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(1),new Integer(1),new Integer(0),new Integer(0)})};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(55);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1)})};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(56);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1)})};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(57);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1)})};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(58);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1),new Integer(1)})};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);


                    obj.reInit();
                    obj.setOpnum(59);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(1),new Integer(1)})};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(65);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(0),new Integer(0),new Integer(0),new Integer(0)})};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(66);
                    in = new Object[]{new ComArray(new Double[]{new Double(10), new Double(20)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);


                    obj.reInit();
                    obj.setOpnum(67);
                    in = new Object[]{new ComArray(new Double[]{new Double(10), new Double(20), new Double(20)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(65);
                    in = new Object[]{new ComArray(new Double[]{new Double(1000)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(66);
                    in = new Object[]{new ComArray(new Double[]{new Double(1000),new Double(123)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(67);
                    in = new Object[]{new ComArray(new Double[]{new Double(1000),new Double(123),new Double(1235765)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(68);
                    in = new Object[]{new ComArray(new Double[]{new Double(1000),new Double(123),new Double(1235765),new Double(1235765)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(69);
                    in = new Object[]{new ComArray(new Double[]{new Double(1000),new Double(123),new Double(1235765),new Double(1235765),new Double(1235765)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(70);
                    in = new Object[]{new ComArray(new Double[]{new Double(1000)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(71);
                    in = new Object[]{new ComArray(new Double[]{new Double(1000),new Double(123)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(72);
                    in = new Object[]{new ComArray(new Double[]{new Double(1000),new Double(123),new Double(123)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(73);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(1000)}, true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);
        //
                    obj.reInit();
                    obj.setOpnum(74);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(10),new Integer(12)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(75);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(10),new Integer(12),new Integer(13)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(76);
                    in = new Object[]{new ComArray(new Integer[]{new Integer(10),new Integer(12),new Integer(12),new Integer(12)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(77);
                    in = new Object[]{new ComArray(new Short[]{new Short((short)10)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(78);
                    in = new Object[]{new ComArray(new Short[]{new Short((short)101),new Short((short)10)}, true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(79);
                    in = new Object[]{new ComArray(new Short[]{new Short((short)110),new Short((short)10),new Short((short)10)}, true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);

                    obj.reInit();
                    obj.setOpnum(80);
                    in = new Object[]{new ComArray(new Short[]{new Short((short)110),new Short((short)10),new Short((short)120),new Short((short)10)},true)};
                    out = null;
                    obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
                    result = handle.call(obj);



        //            obj.reInit();
        //            obj.setOpnum(11);
        //            in = new Object[]{new ComArray(new Integer[10]),new ComArray(new Float[3]),new ComArray(new Double[5]),new ComArray(new Short[10][5])};
        //            // out = new Object[]{new ComArray(Integer.class,new int[]{2},1)};
        //            obj.setUpParams(in, null,Flags.FLAG_NULL,Flags.FLAG_NULL);
        //            result = handle.call(obj);

                    */
            //            obj.reInit();
            //            obj.setOpnum(91);
            //            obj.setUpParams(new Object[]{new ComString("Hello",Flags.FLAG_REPRESENTATION_STRING_LPCTSTR),Boolean.TRUE,new ComString("Hi",Flags.FLAG_REPRESENTATION_STRING_LPCTSTR), new Short((short)100),new ComString("HPri",Flags.FLAG_REPRESENTATION_STRING_LPCTSTR)}, null,Flags.FLAG_REPRESENTATION_STRING_BSTR,Flags.FLAG_NULL);
            //            result = handle.call(obj);

            //            obj.reInit();
            //            obj.setOpnum(91);
            //            obj.setUpParams(new Object[]{new ComString("Hello121",Flags.FLAG_REPRESENTATION_STRING_LPCTSTR),Boolean.TRUE,new ComString("Hi1",Flags.FLAG_REPRESENTATION_STRING_LPCTSTR), new Short((short)100),new ComString("HPri1",Flags.FLAG_REPRESENTATION_STRING_LPCTSTR)}, null,Flags.FLAG_REPRESENTATION_STRING_BSTR,Flags.FLAG_NULL);
            //            result = handle.call(obj);


            //            obj.reInit();
            //            obj.setOpnum(92);
            //            obj.setUpParams(new Object[]{new ComString("Hello",Flags.FLAG_REPRESENTATION_STRING_LPWSTR),Boolean.TRUE,new ComString("QWERTY",Flags.FLAG_REPRESENTATION_STRING_LPWSTR), new Short((short)100),new ComString("123WE",Flags.FLAG_REPRESENTATION_STRING_LPWSTR)}, null,Flags.FLAG_REPRESENTATION_STRING_BSTR,Flags.FLAG_NULL);
            //            result = handle.call(obj);


            //            obj.reInit();
            //            obj.setOpnum(92);
            //            obj.setUpParams(new Object[]{new ComString("Hello121",Flags.FLAG_REPRESENTATION_STRING_LPWSTR),Boolean.TRUE,new ComString("QWERTY1",Flags.FLAG_REPRESENTATION_STRING_LPWSTR), new Short((short)100),new ComString("123WE1",Flags.FLAG_REPRESENTATION_STRING_LPWSTR)}, null,Flags.FLAG_REPRESENTATION_STRING_BSTR,Flags.FLAG_NULL);
            //            result = handle.call(obj);



            //            obj.reInit();
            //            obj.setOpnum(13);
            //            in = new Object[]{new Integer(0),new Float(0),new Double(0),new ComString(null,Flags.FLAG_REPRESENTATION_STRING_BSTR),new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPCTSTR),new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPWSTR),new Short((short)0)};
            //            out = new Object[]{Integer.class,Float.class,Double.class,new ComString(null,Flags.FLAG_REPRESENTATION_STRING_BSTR),new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPCTSTR | Flags.FLAG_REPRESENTATION_POINTER),new ComString(null,Flags.FLAG_REPRESENTATION_STRING_LPWSTR | Flags.FLAG_REPRESENTATION_POINTER),Short.class};
            //            obj.setUpParams(in, out,Flags.FLAG_NULL,Flags.FLAG_NULL);
            //            result = handle.call(obj);
            //            int j = 0;

            //            obj.reInit();
            //            obj.setOpnum(93);
            //            obj.setUpParams(new Object[]{new ComString("Hello121",Flags.FLAG_REPRESENTATION_STRING_LPCTSTR),new ComString("1",Flags.FLAG_REPRESENTATION_STRING_LPWSTR)}, null,Flags.FLAG_REPRESENTATION_STRING_BSTR,Flags.FLAG_NULL);
            //            result = handle.call(obj);



            /*

            IJMeowWrapper handle = queryInterface("5DD52389-B1A4-4fe7-B131-0F8EF73DD175",false);

            GenericObject obj = new GenericObject();
            obj.setOpnum(0);
            // obj.setUpParams(new Object[]{new Integer(35),new Integer(35)}, new Object[]{Integer.class});
            obj.setUpParams(new Object[]{new ComString("testname)")}, null,Flags.FLAG_REPRESENTATION_STRING_BSTR,Flags.FLAG_NULL);
            Object[] result = handle.call(obj);

//            obj.reInit();
//            obj.setOpnum(1);
//            obj.setUpParams(new Object[]{new Integer(90)},null);
//            handle.call(obj);


            // IStats of CAR : FE78387F-D150-4089-832C-BBF02402C872
            handle = queryInterface("FE78387F-D150-4089-832C-BBF02402C872",false);
            obj.reInit();

            obj.setOpnum(1);
            obj.setUpParams(null,new Object[]{ComString.class},Flags.FLAG_NULL,Flags.FLAG_REPRESENTATION_STRING_BSTR);
            handle.call(obj);

        /*    obj.reInit();
            obj.setOpnum(1);
            obj.setUpParams(null,new Object[]{ComString.class});
            result = handle.call(obj);

            // IEngine E27972D8-717F-4516-A82D-B688DC70170C
            handle = queryInterface("E27972D8-717F-4516-A82D-B688DC70170C",false);
            obj.reInit();

            obj.setOpnum(0); // speedup
            handle.call(obj);

            obj.setOpnum(0); // speedup
            handle.call(obj);

            obj.setOpnum(0); // speedup
            handle.call(obj);


            obj.reInit();
            obj.setOpnum(1);
            obj.setUpParams(null,new Object[]{Integer.class});
            result = handle.call(obj);

            obj.reInit();
            obj.setOpnum(2);
            obj.setUpParams(null,new Object[]{Integer.class});
            result = handle.call(obj);

            // System.out.println(result[0]);
//            handle.call(obj);
//            handle.call(obj);
//            handle.call(obj);
//
             */
            var i = 0;
            i++;


        }
        catch (Exception e)
        {
            // TODO Auto-generated catch block
            Console.WriteLine(e.ToString());
            Console.Write(e.StackTrace);
        }
        finally
        {
            try
            {
                Session.DestroySession(_session);
            }
            catch (InteropException e)
            {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }
    }

}
