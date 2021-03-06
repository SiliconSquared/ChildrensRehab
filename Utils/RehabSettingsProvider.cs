using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Xml;

namespace RehabKiosk
{
    /// <summary>
    /// Provides portable, persistent application settings.
    /// </summary>
    public class PortableSettingsProvider : SettingsProvider, IApplicationSettingsProvider
    {

        private XDocument GetXmlDoc()
        {
            // to deal with multiple settings providers accessing the same file, reload on every set or get request.
            XDocument xmlDoc = null;
            bool initnew = false;
            if (File.Exists(this.ApplicationSettingsFile))
            {
                try
                {
                    xmlDoc = XDocument.Load(ApplicationSettingsFile);
                }
                catch { initnew = true; }
            }
            else
                initnew = true;
            if (initnew)
            {
                xmlDoc = new XDocument(new XElement("configuration",
                    new XElement("userSettings", new XElement("Roaming"))));
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            return xmlDoc;
        }

        private string ApplicationSettingsFolder
        {
            get
            {
                string applicationPrivateFolder = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "RehabKiosk");
                return System.IO.Path.Combine(applicationPrivateFolder, "Config");
            }
        }


        private string ApplicationSettingsFile
        {
            get
            {
                return Path.Combine(ApplicationSettingsFolder, "app.config");
            }
        }


        public override string ApplicationName { get { return Assembly.GetExecutingAssembly().GetName().Name; } set { } }

        public override string Name => "PortableSettingsProvider";

        public override void Initialize(string name, NameValueCollection config)
        {
            if (String.IsNullOrEmpty(name)) name = "PortableSettingsProvider";
            base.Initialize(name, config);
        }

        /// <summary>
        /// Applies this settings provider to each property of the given settings.
        /// </summary>
        /// <param name="settingsList">An array of settings.</param>
        public static void ApplyProvider(params ApplicationSettingsBase[] settingsList)
        {
            foreach (var settings in settingsList)
            {
                var provider = new PortableSettingsProvider();
                settings.Providers.Add(provider);
                foreach (SettingsProperty prop in settings.Properties)
                    prop.Provider = provider;
                settings.Reload();
            }
        }

        public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property)
        {
            SettingsPropertyValue value = new SettingsPropertyValue(property);
            value.IsDirty = false;
            return value;
        }



        public void Reset(SettingsContext context)
        {
            try
            {
                if (File.Exists(ApplicationSettingsFile))
                    File.Delete(ApplicationSettingsFile);
            }
            catch { }
        }

        public void Upgrade(SettingsContext context, SettingsPropertyCollection properties)
        { /* don't do anything here*/ }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            XDocument xmlDoc = GetXmlDoc();
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();

            try
            {
                // iterate through settings to be retrieved
                foreach (SettingsProperty setting in collection)
                {
                    SettingsPropertyValue value = new SettingsPropertyValue(setting);
                    value.IsDirty = false;
                    //Set serialized value to xml element from file. This will be deserialized by SettingsPropertyValue when needed.
                    var loadedValue = getXmlValue(xmlDoc, XmlConvert.EncodeLocalName((string)context["GroupName"]), setting);
                    if (loadedValue != null)
                        value.SerializedValue = loadedValue;
                    else value.PropertyValue = null;
                    values.Add(value);
                }
            }
            catch { }
            return values;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            XDocument xmlDoc = GetXmlDoc();
            foreach (SettingsPropertyValue value in collection)
            {
                setXmlValue(xmlDoc, XmlConvert.EncodeLocalName((string)context["GroupName"]), value);
            }
            try
            {
                if (!Directory.Exists(ApplicationSettingsFolder))
                    Directory.CreateDirectory(ApplicationSettingsFolder);

                // Make sure that special chars such as '\r\n' are preserved by replacing them with char entities.
                using (var writer = XmlWriter.Create(ApplicationSettingsFile,
                    new XmlWriterSettings() { NewLineHandling = NewLineHandling.Entitize, Indent = true }))
                {
                    xmlDoc.Save(writer);
                }
            }
            catch { }
        }

        private object getXmlValue(XDocument xmlDoc, string scope, SettingsProperty prop)
        {
            object result = null;
            if (!IsUserScoped(prop))
                return result;

            try
            {
                //determine the location of the settings property
                XElement xmlSettings = xmlDoc.Element("configuration").Element("userSettings");
                if (IsRoaming(prop))
                    xmlSettings = xmlSettings.Element("Roaming");
                else xmlSettings = xmlSettings.Element("PC_" + Environment.MachineName);
                // retrieve the value or set to default if available
                if (xmlSettings != null && xmlSettings.Element(scope) != null && xmlSettings.Element(scope).Element(prop.Name) != null)
                {
                    using (var reader = xmlSettings.Element(scope).Element(prop.Name).CreateReader())
                    {
                        reader.MoveToContent();
                        switch (prop.SerializeAs)
                        {
                            case SettingsSerializeAs.Xml:
                                result = reader.ReadInnerXml();
                                break;
                            case SettingsSerializeAs.Binary:
                                result = reader.ReadInnerXml();
                                result = Convert.FromBase64String(result as string);
                                break;
                            default:
                                result = reader.ReadElementContentAsString();
                                break;
                        }
                    }
                }
                else
                    result = prop.DefaultValue;
            }
            catch
            {
                result = null;
            }
            return result;
        }

        private void setXmlValue(XDocument xmlDoc, string scope, SettingsPropertyValue value)
        {
            try
            {
                if (!IsUserScoped(value.Property)) return;
                //determine the location of the settings property
                XElement xmlSettings = xmlDoc.Element("configuration").Element("userSettings");
                XElement xmlSettingsLoc;
                if (IsRoaming(value.Property))
                    xmlSettingsLoc = xmlSettings.Element("Roaming");
                else xmlSettingsLoc = xmlSettings.Element("PC_" + Environment.MachineName);
                // the serialized value to be saved
                XNode serialized;
                if (value.SerializedValue == null) serialized = new XText("");
                else if (value.Property.SerializeAs == SettingsSerializeAs.Xml)
                    serialized = XElement.Parse((string)value.SerializedValue);
                else if (value.Property.SerializeAs == SettingsSerializeAs.Binary)
                    serialized = new XText(Convert.ToBase64String((byte[])value.SerializedValue));
                else serialized = new XText((string)value.SerializedValue);
                // check if setting already exists, otherwise create new
                if (xmlSettingsLoc == null)
                {
                    if (IsRoaming(value.Property)) xmlSettingsLoc = new XElement("Roaming");
                    else xmlSettingsLoc = new XElement("PC_" + Environment.MachineName);
                    xmlSettingsLoc.Add(new XElement(scope,
                        new XElement(value.Name, serialized)));
                    xmlSettings.Add(xmlSettingsLoc);
                }
                else
                {
                    XElement xmlScope = xmlSettingsLoc.Element(scope);
                    if (xmlScope != null)
                    {
                        XElement xmlElem = xmlScope.Element(value.Name);
                        if (xmlElem == null) xmlScope.Add(new XElement(value.Name, serialized));
                        else xmlElem.ReplaceAll(serialized);
                    }
                    else
                    {
                        xmlSettingsLoc.Add(new XElement(scope, new XElement(value.Name, serialized)));
                    }
                }
            }
            catch { }
        }

        // Iterates through the properties' attributes to determine whether it's user-scoped or application-scoped.
        private bool IsUserScoped(SettingsProperty prop)
        {
            foreach (DictionaryEntry d in prop.Attributes)
            {
                Attribute a = (Attribute)d.Value;
                if (a.GetType() == typeof(UserScopedSettingAttribute))
                    return true;
            }
            return false;
        }

        // Iterates through the properties' attributes to determine whether it's set to roam.
        private bool IsRoaming(SettingsProperty prop)
        {
            foreach (DictionaryEntry d in prop.Attributes)
            {
                Attribute a = (Attribute)d.Value;
                if (a.GetType() == typeof(SettingsManageabilityAttribute))
                    return true;
            }
            return false;
        }
    }
}

