// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Automation;
using System;
using System.Threading;

namespace Opc.Classic.Dcom.Test;

public class MSExcel2
{

    private readonly ComServer _comServer;
    private IDispatch _dispatch;
    private IComObject _unknown;
    private IDispatch _dispatchOfWorkSheets;
    private IDispatch _dispatchOfWorkBook;
    private IDispatch _dispatchOfWorkSheet;
    private readonly Session _session;

    public MSExcel2(string address, string[] args)
    {
        _session = Session.CreateSession(args[1], args[2], args[3]);
        //        session.useSessionSecurity(true);
        _comServer = new ComServer(ProgId.ValueOf("Excel.Application"), address, _session);
    }


    public void StartExcel()
    {
        _unknown = _comServer.CreateInstance();
        _dispatch = (IDispatch)ObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
    }


    public void ShowExcel()
    {
        var dispId = _dispatch.GetIDsOfNames("Visible");
        var variant = new Variant(true);
        _dispatch.Put(dispId, variant);
    }


    public void CreateWorkSheet()
    {
        var dispId = _dispatch.GetIDsOfNames("Workbooks");

        var outVal = _dispatch.Get(dispId);

        var dispatchOfWorkBooks = (IDispatch)ObjectFactory.NarrowObject(outVal.ObjectAsComObject);


        var outVal2 = dispatchOfWorkBooks.CallMethodA("Add", new object[] { Variant.CreateOPTIONAL_PARAM() });
        _dispatchOfWorkBook = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

        outVal = _dispatchOfWorkBook.Get("Worksheets");

        _dispatchOfWorkSheets = (IDispatch)ObjectFactory.NarrowObject(outVal.ObjectAsComObject);

        outVal2 = _dispatchOfWorkSheets.CallMethodA("Add", new object[] { Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });
        _dispatchOfWorkSheet = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
    }


    public void PasteArrayToWorkSheet()
    {
        var dispId = _dispatchOfWorkSheet.GetIDsOfNames("Range");
        var variant = new Variant(new ComString("A1:C3"));
        object[] @out = { typeof(Variant) };
        var outVal2 = _dispatchOfWorkSheet.Get(dispId, new object[] { variant });
        var dispRange = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);


        Variant[][] newValue = {
              new Variant[] {new Variant(new ComString("defe")), new Variant(false), new Variant(98765.0 / 12345.0) },
              new Variant[] {new Variant(DateTime.Now), new Variant(5454),new Variant((float)(22.0 / 7.0))},
              new Variant[] {new Variant(true), new Variant(new ComString("dffe")),new Variant(DateTime.Now)}
          };

        // implement safe array XxX dimension

        dispRange.Put("Value2", new Variant(new ComArray(newValue)));

        Thread.Sleep(10000);

        var variant2 = dispRange.Get("Value2");
        var newValue2 = variant2.ObjectAsArray;
        newValue = (Variant[][])newValue2.ArrayInstance;
        for (var i = 0; i < newValue.Length; i++)
        {
            for (var j = 0; j < newValue[i].Length; j++)
            {
                Console.Write(newValue[i][j] + "\t");
            }
            Console.WriteLine();
        }

        _dispatchOfWorkBook.CallMethod("close", new object[] { false, Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });
        _dispatch.CallMethod("Quit");
        Session.DestroySession(_session);
    }


    public static void RunTest(string[] args)
    {

        try
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Please provide address domain username password");
                return;
            }


            var test = new MSExcel2(args[0], args);
            test.StartExcel();
            test.ShowExcel();
            test.CreateWorkSheet();
            test.PasteArrayToWorkSheet();
        }
        catch (Exception e)
        {
            // TODO Auto-generated catch block
            Console.WriteLine(e.ToString());
            Console.Write(e.StackTrace);
        }
    }





}
