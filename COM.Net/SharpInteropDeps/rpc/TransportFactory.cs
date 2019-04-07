// 
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
// 
// j-Interop (Pure Java implementation of DCOM protocol)
// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace rpc {
    using SharpCifs.Util.Sharpen;
    using System;

    /// <summary>
    /// Transport factory
    /// </summary>
    public abstract class TransportFactory {

        /// <summary>
        /// Create transport
        /// </summary>
        /// <param name="address"></param>
        /// <param name="properties"></param>
        /// <exception cref="ProviderException"></exception>
        /// <returns></returns>
        public abstract ITransport CreateTransport(string address, Properties properties);

        /// <summary>
        /// Default properties
        /// </summary>
        public static Properties DefaultProperties { get; } = new Properties();

        // TODO!!!

        //    private static class MetaTransportFactory extends TransportFactory {
        //
        //        public Transport createTransport(String address, SharpCifs.Util.Sharpen.Properties properties)
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

        // private static Properties defaultProperties = new Properties();

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
        //                SharpCifs.Util.Sharpen.Properties properties = new SharpCifs.Util.Sharpen.Properties();
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
      // public static Properties DefaultProperties {
      //     get {
      //
                //   // TODO
                //   lock (typeof(TransportFactory)) {
                //       if (defaultProperties == null) {
                //           var properties = new Properties();
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
                //    var properties = new Properties(defaultProperties);
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

}