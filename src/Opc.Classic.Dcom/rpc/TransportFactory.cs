// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom.Internal;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Transport factory
/// </summary>
public abstract class TransportFactory
{
    /// <summary>
    /// Create transport
    /// </summary>
    /// <param name="address">Network address or binding address for the remote endpoint.</param>
    /// <param name="properties">Property values used to initialize the COM descriptor.</param>
    /// <exception cref="ProviderException">Thrown when the provider cannot complete the requested RPC transport operation.</exception>
    /// <returns>A new <see cref="ITransport"/> instance built from <paramref name="address"/>.</returns>
    public abstract ITransport CreateTransport(string address, PropertyBag properties);

    /// <summary>
    /// Default properties
    /// </summary>
    public static PropertyBag DefaultProperties { get; } = new PropertyBag();

    //    private static class MetaTransportFactory extends TransportFactory {
    //
    //        public Transport createTransport(String address, PropertyBag properties)
    //                throws ProviderException {
    //            if (address == null) {
    //                throw new ProviderException("No address specified.");
    //            }
    //            if (properties == null) {
    //                properties = TransportFactory.getDefaultProperties();
    //            }
    //            Iterator factories = FACTORIES.iterator();
    //            while (factories.hasNext()) {
    //                try {
    //                    return ((TransportFactory)
    //                            factories.next()).createTransport(address,
    //                                    properties);
    //                } catch (ProviderException ex) { }
    //            }
    //            throw new ProviderException(
    //                    "Unable to find suitable provider for \"" + address +
    //                            "\".");
    //        }
    //
    //    }

    //    private static final TransportFactory META_FACTORY;

    //    private static final List FACTORIES;

    // private static PropertyBag defaultProperties = new PropertyBag();

    //    static {
    //        META_FACTORY = new MetaTransportFactory();
    //        FACTORIES = new ArrayList();
    //        String service = "META-INF/services/" +
    //                TransportFactory.class.getName();
    //        Set locations = new HashSet();
    //        ClassLoader loader = TransportFactory.class.getClassLoader();
    //        if (loader != null) {
    //            try {
    //                Enumeration resources = loader.getResources(service);
    //                while (resources.hasMoreElements()) {
    //                    locations.add(resources.nextElement());
    //                }
    //            } catch (IOException ex) { }
    //        }
    //        try {
    //            Enumeration resources = ClassLoader.getSystemResources(service);
    //            while (resources.hasMoreElements()) {
    //                locations.add(resources.nextElement());
    //            }
    //        } catch (IOException ex) { }
    //        Iterator iterator = locations.iterator();
    //        while (iterator.hasNext()) {
    //            try {
    //                PropertyBag properties = new PropertyBag();
    //                properties.load(((URL) iterator.next()).openStream());
    //                Enumeration classNames = properties.propertyNames();
    //                while (classNames.hasMoreElements()) {
    //                    Class factoryClass =
    //                            Class.forName((String) classNames.nextElement());
    //                    TransportFactory factory = (TransportFactory)
    //                            factoryClass.newInstance();
    //                    FACTORIES.add(factory);
    //                }
    //            } catch (Exception ex) { }
    //        }
    //    }
    //
    //    public static TransportFactory getInstance() {
    //        return META_FACTORY;
    //    }

    // /// <summary>
    // /// Default properties
    // /// </summary>
    // public static PropertyBag DefaultProperties {
    //     get {
    //
    //   // TODO
    //   lock (typeof(TransportFactory)) {
    //       if (defaultProperties == null) {
    //           var properties = new PropertyBag();
    //           string defaults = null;
    //           try {
    //               defaults = System.getProperty("rpc.properties");
    //           }
    //           catch (Exception) {
    //           }
    //           if (defaults != null) {
    //               URL url = null;
    //               try {
    //                   url = new URL(new File(".").toURL(), defaults);
    //                   properties.load(url.openStream());
    //               }
    //               catch (MalformedURLException ex) {
    //                   throw new ArgumentException("Bad location " + defaults + ": " + ex.Message);
    //               }
    //               catch (Exception ex) {
    //                   throw new ArgumentException("Unable to load " + " RPC properties from " + url + ": " + ex.Message);
    //               }
    //           }
    //           else {
    //               try {
    //                   properties.load(typeof(TransportFactory).getResourceAsStream("/rpc.properties"));
    //               }
    //               catch (Exception) {
    //                   try {
    //                       properties.load(ClassLoader.getSystemResourceAsStream("/rpc.properties"));
    //                   }
    //                   catch (Exception) {
    //                   }
    //               }
    //           }
    //           defaultProperties = properties;
    //       }
    //   }
    //    var properties = new PropertyBag(defaultProperties);
    //    try {
    //        properties.PutAll(System.Properties);
    //    }
    //    catch (Exception) {
    //    }
    //    return properties;
    //         return defaultProperties;
    //     }
    // }
    //

}
