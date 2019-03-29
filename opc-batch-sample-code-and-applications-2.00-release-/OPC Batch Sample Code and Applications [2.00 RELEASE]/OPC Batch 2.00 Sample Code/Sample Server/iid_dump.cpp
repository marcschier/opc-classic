#define INITGUID

#include <windows.h>
#include <objidl.h>
#include <cguid.h>
#include <isguids.h>
//#include <rpcproxy.h>
#include <oaidl.h>
#include <oleidl.h>
#include <olectlid.h>

char * iid_dump(REFIID rid)
{
	if(rid == IID_IUnknown)
		return "IID_IUnknown";
	if(rid == IID_IClassFactory)
		return "IID_IClassFactory";
	if(rid == IID_IMarshal)
		return "IID_IMarshal";
	if(rid == IID_IMalloc)
		return "IID_IMalloc";
	if(rid == IID_IMallocSpy)
		return "IID_IMallocSpy";
	if(rid == IID_IStdMarshalInfo)
		return "IID_IStdMarshalInfo";
	if(rid == IID_IExternalConnection)
		return "IID_IExternalConnection";
	if(rid == IID_IEnumUnknown)
		return "IID_IEnumUnknown";
	if(rid == IID_IBindCtx)
		return "IID_IBindCtx";
	if(rid == IID_IEnumMoniker)
		return "IID_IEnumMoniker";
	if(rid == IID_IRunnableObject)
		return "IID_IRunnableObject";
	if(rid == IID_IRunningObjectTable)
		return "IID_IRunningObjectTable";
	if(rid == IID_IPersist)
		return "IID_IPersist";
	if(rid == IID_IPersistStream)
		return "IID_IPersistStream";
	if(rid == IID_IMoniker)
		return "IID_IMoniker";
	if(rid == IID_IROTData)
		return "IID_IROTData";
	if(rid == IID_IEnumString)
		return "IID_IEnumString";
	if(rid == IID_IStream)
		return "IID_IStream";
	if(rid == IID_IEnumSTATSTG)
		return "IID_IEnumSTATSTG";
	if(rid == IID_IStorage)
		return "IID_IStorage";
	if(rid == IID_IPersistFile)
		return "IID_IPersistFile";
	if(rid == IID_IPersistStorage)
		return "IID_IPersistStorage";
	if(rid == IID_ILockBytes)
		return "IID_ILockBytes";
	if(rid == IID_IEnumFORMATETC)
		return "IID_IEnumFORMATETC";
	if(rid == IID_IEnumSTATDATA)
		return "IID_IEnumSTATDATA";
	if(rid == IID_IRootStorage)
		return "IID_IRootStorage";
	if(rid == IID_IAdviseSink)
		return "IID_IAdviseSink";
	if(rid == IID_IAdviseSink2)
		return "IID_IAdviseSink2";
	if(rid == IID_IDataObject)
		return "IID_IDataObject";
	if(rid == IID_IDataAdviseHolder)
		return "IID_IDataAdviseHolder";
	if(rid == IID_IMessageFilter)
		return "IID_IMessageFilter";
	if(rid == IID_IRpcChannelBuffer)
		return "IID_IRpcChannelBuffer";
	if(rid == IID_IRpcProxyBuffer)
		return "IID_IRpcProxyBuffer";
	if(rid == IID_IRpcStubBuffer)
		return "IID_IRpcStubBuffer";
	if(rid == IID_IPSFactoryBuffer)
		return "IID_IPSFactoryBuffer";

	if(rid == IID_IRpcChannel)
		return "IID_IRpcChannel";
	if(rid == IID_IRpcStub)
		return "IID_IRpcStub";
	if(rid == IID_IStubManager)
		return "IID_IStubManager";
	if(rid == IID_IRpcProxy)
		return "IID_IRpcProxy";
	if(rid == IID_IProxyManager)
		return "IID_IProxyManager";
	if(rid == IID_IPSFactory)
		return "IID_IPSFactory";
	if(rid == IID_IInternalMoniker)
		return "IID_IInternalMoniker";
	if(rid == IID_IDfReserved1)
		return "IID_IDfReserved1";
	if(rid == IID_IDfReserved2)
		return "IID_IDfReserved2";
	if(rid == IID_IDfReserved3)
		return "IID_IDfReserved3";
	if(rid == IID_IStub)
		return "IID_IStub";
	if(rid == IID_IProxy)
		return "IID_IProxy";
	if(rid == IID_IEnumGeneric)
		return "IID_IEnumGeneric";
	if(rid == IID_IEnumHolder)
		return "IID_IEnumHolder";
	if(rid == IID_IEnumCallback)
		return "IID_IEnumCallback";
	if(rid == IID_IOleManager)
		return "IID_IOleManager";
	if(rid == IID_IOlePresObj)
		return "IID_IOlePresObj";
	if(rid == IID_IDebug)
		return "IID_IDebug";
	if(rid == IID_IDebugStream)
		return "IID_IDebugStream";

	if(rid == IID_IUniformResourceLocator)
		return "IID_IUniformResourceLocator";

/*	if(rid == IID_GENERIC_CHECK_IID)
*		return "IID_GENERIC_CHECK_IID";
*	if(rid == IID_BS_LOOKUP_SETUP)
*		return "IID_BS_LOOKUP_SETUP";
*	if(rid == IID_BS_LOOKUP_INITIAL_TEST)
*		return "IID_BS_LOOKUP_INITIAL_TEST";
*	if(rid == IID_BS_LOOKUP_NEXT_TEST)
*		return "IID_BS_LOOKUP_NEXT_TEST";
*	if(rid == IID_BS_LOOKUP_RETURN_RESULT)
*		return "IID_BS_LOOKUP_RETURN_RESULT";
*/

	if(rid == IID_ICreateTypeInfo)
		return "IID_ICreateTypeInfo";
	if(rid == IID_ICreateTypeLib)
		return "IID_ICreateTypeLib";
	if(rid == IID_IDispatch)
		return "IID_IDispatch";
	if(rid == IID_IEnumVARIANT)
		return "IID_IEnumVARIANT";
	if(rid == IID_ITypeComp)
		return "IID_ITypeComp";
	if(rid == IID_ITypeInfo)
		return "IID_ITypeInfo";
	if(rid == IID_ITypeLib)
		return "IID_ITypeLib";
	if(rid == IID_IErrorInfo)
		return "IID_IErrorInfo";
	if(rid == IID_ICreateErrorInfo)
		return "IID_ICreateErrorInfo";
	if(rid == IID_ISupportErrorInfo)
		return "IID_ISupportErrorInfo";

	if(rid == IID_IOleAdviseHolder)
		return "IID_IOleAdviseHolder";
	if(rid == IID_IOleCache)
		return "IID_IOleCache";
	if(rid == IID_IOleCache2)
		return "IID_IOleCache2";
	if(rid == IID_IOleCacheControl)
		return "IID_IOleCacheControl";
	if(rid == IID_IParseDisplayName)
		return "IID_IParseDisplayName";
	if(rid == IID_IOleContainer)
		return "IID_IOleContainer";
	if(rid == IID_IOleClientSite)
		return "IID_IOleClientSite";
	if(rid == IID_IOleObject)
		return "IID_IOleObject";
	if(rid == IID_IOleWindow)
		return "IID_IOleWindow";
	if(rid == IID_IOleLink)
		return "IID_IOleLink";
	if(rid == IID_IOleItemContainer)
		return "IID_IOleItemContainer";
	if(rid == IID_IOleInPlaceUIWindow)
		return "IID_IOleInPlaceUIWindow";
	if(rid == IID_IOleInPlaceActiveObject)
		return "IID_IOleInPlaceActiveObject";
	if(rid == IID_IOleInPlaceFrame)
		return "IID_IOleInPlaceFrame";
	if(rid == IID_IOleInPlaceObject)
		return "IID_IOleInPlaceObject";
	if(rid == IID_IOleInPlaceSite)
		return "IID_IOleInPlaceSite";
	if(rid == IID_IViewObject)
		return "IID_IViewObject";
	if(rid == IID_IViewObject2)
		return "IID_IViewObject2";
	if(rid == IID_IDropSource)
		return "IID_IDropSource";
	if(rid == IID_IDropTarget)
		return "IID_IDropTarget";
	if(rid == IID_IEnumOLEVERB)
		return "IID_IEnumOLEVERB";

	if(rid == IID_IPropertyNotifySink)
		return "IID_IPropertyNotifySink";
	if(rid == IID_IClassFactory2)
		return "IID_IClassFactory2";
	if(rid == IID_IProvideClassInfo)
		return "IID_IProvideClassInfo";
	if(rid == IID_IProvideClassInfo2)
		return "IID_IProvideClassInfo2";
	if(rid == IID_IConnectionPointContainer)
		return "IID_IConnectionPointContainer";
	if(rid == IID_IEnumConnectionPoints)
		return "IID_IEnumConnectionPoints";
	if(rid == IID_IConnectionPoint)
		return "IID_IConnectionPoint";
	if(rid == IID_IEnumConnections)
		return "IID_IEnumConnections";
	if(rid == IID_IOleControl)
		return "IID_IOleControl";
	if(rid == IID_IOleControlSite)
		return "IID_IOleControlSite";
	if(rid == IID_ISimpleFrameSite)
		return "IID_ISimpleFrameSite";
	if(rid == IID_IPersistStreamInit)
		return "IID_IPersistStreamInit";
	if(rid == IID_IPersistMemory)
		return "IID_IPersistMemory";
	if(rid == IID_IPersistPropertyBag)
		return "IID_IPersistPropertyBag";
	if(rid == IID_IPropertyBag)
		return "IID_IPropertyBag";
	if(rid == IID_IErrorLog)
		return "IID_IErrorLog";
	if(rid == IID_IPropertyFrame)
		return "IID_IPropertyFrame";
	if(rid == IID_ISpecifyPropertyPages)
		return "IID_ISpecifyPropertyPages";
	if(rid == IID_IPerPropertyBrowsing)
		return "IID_IPerPropertyBrowsing";
	if(rid == IID_IPropertyPageSite)
		return "IID_IPropertyPageSite";
	if(rid == IID_IPropertyPage)
		return "IID_IPropertyPage";
	if(rid == IID_IPropertyPage2)
		return "IID_IPropertyPage2";
	if(rid == IID_IFont)
		return "IID_IFont";
	if(rid == IID_IFontDisp)
		return "IID_IFontDisp";
	if(rid == IID_IPicture)
		return "IID_IPicture";
	if(rid == IID_IPictureDisp)
		return "IID_IPictureDisp";

static	char	buf[80];
	wsprintf(buf, "0x%-08x 0x%-04x 0x%-04x", 
		rid.Data1, rid.Data2, rid.Data3);
	return buf;

}
