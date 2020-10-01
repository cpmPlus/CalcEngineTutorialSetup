using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ABB.Vtrin;
using ABB.Vtrin.Drivers;

namespace CalcEngineTutorialSetup
{
    class EquipmentModelFixture : IFixture
    {
        public void Setup()
        {
            // Create or update equipment type and properties
            CreateOrUpdateEquipmentTypes();

            // Create or update equipment instances
            if (Context.NumberOfSites > 1)
            {
                for (var site = 1; site <= Context.NumberOfSites; site++)
                {
                    createSiteFacade(site);

                    if (site != Context.NumberOfSites)
                    {
                        // There's some concurrency issue after creating instances for one site
                        Thread.Sleep(300);
                    }
                }
            }
            else
            {
                createSiteFacade(null);
            }
        }

        public void Cleanup()
        {
            removeAclEntries();
            deleteEquipmentInstances();
            deleteEquipmentTypes();
        }

        private void CreateOrUpdateEquipmentTypes()
        {

            // CREATE EQUIPMENT TYPES
            // ======================

            // Abstract base types
            // ===================

            var baseEquipmentType = CreateOrUpdateEquipmentType(
                equipmentTypeName: "Device",
                isAbstract: true);

            var mechanicalEquipmentType = CreateOrUpdateEquipmentType(
                equipmentTypeName: "Mechanical device",
                baseEquipmentType: baseEquipmentType,
                isAbstract: true);

            var electricalEquipmentType = CreateOrUpdateEquipmentType(
                equipmentTypeName: "Electrical device",
                baseEquipmentType: baseEquipmentType,
                isAbstract: true);

            // Equipment types
            // ===============

            var tankType = CreateOrUpdateEquipmentType(
                equipmentTypeName: "Tank",
                baseEquipmentType: mechanicalEquipmentType);

            var pipeType = CreateOrUpdateEquipmentType(
                equipmentTypeName: "Pipe",
                baseEquipmentType: mechanicalEquipmentType);

            var pumpType = CreateOrUpdateEquipmentType(
                equipmentTypeName: "Pump",
                baseEquipmentType: electricalEquipmentType);


            // EQUIPMENT TYPE PROPERTIES
            // =========================

            // Common properties
            // =================

            CreateOrUpdateEquipmentProperty(
                propertyName: "Manufacturer",
                propertyType: ABB.Vtrin.cTypeCode.String,
                propertyUnit: "",
                propertyDescription: "The manufacturer of the device",
                isHistorized: false,
                equipmentType: baseEquipmentType);

            // Tank properties
            // ===============

            CreateOrUpdateEquipmentProperty(
                propertyName: "Level",
                propertyType: ABB.Vtrin.cTypeCode.Double,
                propertyUnit: "mm",
                propertyDescription: "The water level inside the tank",
                isHistorized: true,
                equipmentType: tankType);

            CreateOrUpdateEquipmentProperty(
                propertyName: "Volume",
                propertyType: ABB.Vtrin.cTypeCode.Double,
                propertyUnit: "l",
                propertyDescription: "The total capacity of the tank",
                isHistorized: false,
                equipmentType: tankType);

            CreateOrUpdateEquipmentProperty(
                propertyName: "ContentsVolume",
                propertyType: ABB.Vtrin.cTypeCode.Double,
                propertyUnit: "l",
                propertyDescription: "The volume of water in the tank. This is calculated from Level and Diameter",
                isHistorized: true,
                equipmentType: tankType);

            CreateOrUpdateEquipmentProperty(
                propertyName: "Diameter",
                propertyType: ABB.Vtrin.cTypeCode.Double,
                propertyUnit: "mm",
                propertyDescription: "The diameter of the tank. The tank is a cylinder.",
                isHistorized: false,
                equipmentType: tankType);

            // Pipe properties
            // ===============

            CreateOrUpdateEquipmentProperty(
                propertyName: "Diameter",
                propertyType: ABB.Vtrin.cTypeCode.Double,
                propertyUnit: "cm",
                propertyDescription: "The diameter of the pipe",
                isHistorized: false,
                equipmentType: pipeType);

            CreateOrUpdateEquipmentProperty(
                propertyName: "Flow",
                propertyType: ABB.Vtrin.cTypeCode.Double,
                propertyUnit: "l/min",
                propertyDescription: "The current water flow through the pipe",
                isHistorized: true,
                equipmentType: pipeType);

            // Pump properties
            // ===============

            CreateOrUpdateEquipmentProperty(
                propertyName: "Power",
                propertyType: ABB.Vtrin.cTypeCode.Double,
                propertyUnit: "W",
                propertyDescription: "The current power of the pump",
                isHistorized: true,
                equipmentType: pumpType);

            CreateOrUpdateEquipmentProperty(
                propertyName: "Nominal power",
                propertyType: ABB.Vtrin.cTypeCode.Double,
                propertyUnit: "W",
                propertyDescription: "The nominal power of the pump",
                isHistorized: false,
                equipmentType: pumpType);

            CreateOrUpdateEquipmentProperty(
                propertyName: "Source tank",
                propertyType: ABB.Vtrin.cTypeCode.GUID,
                propertyUnit: null,
                propertyDescription: "The tank that pump is pumping water from",
                isHistorized: false,
                equipmentType: pumpType,
                referenceTarget: "Class:" + tankType.ClassName);

            CreateOrUpdateEquipmentProperty(
                propertyName: "Target tank",
                propertyType: ABB.Vtrin.cTypeCode.GUID,
                propertyUnit: null,
                propertyDescription: "The tank that pump is pumping water into",
                isHistorized: false,
                equipmentType: pumpType,
                referenceTarget: "Class:" + tankType.ClassName);

            CreateOrUpdateEquipmentProperty(
                propertyName: "Operational state",
                propertyType: ABB.Vtrin.cTypeCode.Int16,
                propertyUnit: null,
                propertyDescription: "Writing to this property turns the pump on or off",
                isHistorized: true,
                equipmentType: pumpType,
                referenceTarget: "Enumeration:Binary Text(1)");

            CreateOrUpdateEquipmentProperty(
                propertyName: "Power state",
                propertyType: ABB.Vtrin.cTypeCode.Int16,
                propertyUnit: null,
                propertyDescription: "Tells whether the pump is powered or not",
                isHistorized: true,
                equipmentType: pumpType,
                referenceTarget: "Enumeration:Binary Text(6)");
        }

        private ABB.Vtrin.Interfaces.IEquipment CreateOrUpdateEquipmentType(
            string equipmentTypeName,
            bool isAbstract = false,
            ABB.Vtrin.Interfaces.IEquipment baseEquipmentType = null)
        {
            var equipmentCache = Context.Driver.Classes["Equipment"].Instances;

            // Try to find existing equipment type with the given name
            var equipmentType =
                (ABB.Vtrin.Interfaces.IEquipment)equipmentCache[equipmentTypeName]?.BeginUpdate();

            // Case: No existing equipment type found
            // > Create a new equipment type
            if (equipmentType == null)
                equipmentType = (ABB.Vtrin.Interfaces.IEquipment)equipmentCache.Add();

            // Update attributes and commit changes
            equipmentType.Name = equipmentTypeName;
            equipmentType.Base = baseEquipmentType;
            equipmentType.Abstract = isAbstract;
            equipmentType.CommitChanges();

            return equipmentType;
        }

        private ABB.Vtrin.Interfaces.IPropertyDefinition CreateOrUpdateEquipmentProperty(
            string propertyName,
            ABB.Vtrin.cTypeCode propertyType,
            string propertyUnit,
            bool isHistorized,
            ABB.Vtrin.Interfaces.IEquipment equipmentType,
            string propertyDescription = null,
            string referenceTarget = null)
        {
            ABB.Vtrin.Interfaces.IPropertyDefinition property;
            var equipmentPropertyInstances = Context.Driver.Classes["EquipmentPropertyInfo"].Instances;

            // Query existing property infos using property name and equipment type
            var properties = equipmentPropertyInstances.GetInstanceSet("Equipment=? AND DisplayName=?", equipmentType, propertyName);

            // Case: No existing property found
            // > Create a new property
            if (properties.Length == 0)
                property = (ABB.Vtrin.Interfaces.IPropertyDefinition)equipmentPropertyInstances.Add();

            // Case: Existing property found
            // > Select that and begin to update
            else
                property = (ABB.Vtrin.Interfaces.IPropertyDefinition)properties[0].BeginUpdate();

            // Update property info
            property.DisplayName = propertyName;
            property.Type = (int)propertyType;
            property.Unit = propertyUnit;
            property.Description = propertyDescription;
            property.Historized = isHistorized;
            property.Equipment = equipmentType;
            property.ReferenceTarget = referenceTarget;

            // Save or update property
            property.CommitChanges();

            return property;
        }

        private void CreateOrUpdateEquipmentInstances(int? site)
        {
            string topLevelHierarchy = getTopLevelHierarchyPrefix(site);

            // Define tank instances
            // =====================

            var sourceTank = GetOrCreateEquipmentInstance(
                instanceName: $"{topLevelHierarchy}.Tank area.Source tank",
                equipmentType: Context.Driver.Classes["Path_Tank"]);

            var targetTank = GetOrCreateEquipmentInstance(
                instanceName: $"{topLevelHierarchy}.Tank area.Target tank",
                equipmentType: Context.Driver.Classes["Path_Tank"]);


            // Define pipe instances
            // =====================

            var mainPipe = GetOrCreateEquipmentInstance(
                instanceName: $"{topLevelHierarchy}.Pipe",
                equipmentType: Context.Driver.Classes["Path_Pipe"]);

            var flowbackPipe = GetOrCreateEquipmentInstance(
                instanceName: $"{topLevelHierarchy}.Flowback pipe",
                equipmentType: Context.Driver.Classes["Path_Pipe"]);

            // Define pump instance
            // ====================

            var pump = GetOrCreateEquipmentInstance(
                instanceName: $"{topLevelHierarchy}.Pump section.Pump",
                equipmentType: Context.Driver.Classes["Path_Pump"]);


            // Defining instance properties
            // ============================

            pump = pump.BeginUpdate();
            pump["Source tank"] = sourceTank;
            pump["Target tank"] = targetTank;
            pump["Nominal power"] = 1000;
            pump["Manufacturer"] = "Pumps & Pipes Inc.";
            pump.CommitChanges();

            targetTank = targetTank.BeginUpdate();
            targetTank["Volume"] = 1000;
            targetTank["Diameter"] = 1128;
            targetTank["Level"] = 500;
            targetTank["ContentsVolume"] = 0;
            targetTank["Manufacturer"] = "Tank Company";
            targetTank.CommitChanges();

            sourceTank = sourceTank.BeginUpdate();
            sourceTank["Volume"] = 1000;
            sourceTank["Diameter"] = 1128;
            sourceTank["Level"] = 500;
            sourceTank["ContentsVolume"] = 0;
            sourceTank["Manufacturer"] = "Tank Company";
            sourceTank.CommitChanges();

            mainPipe = mainPipe.BeginUpdate();
            mainPipe["Diameter"] = 20;
            mainPipe["Manufacturer"] = "Pumps & Pipes Inc.";
            mainPipe.CommitChanges();

            flowbackPipe = flowbackPipe.BeginUpdate();
            flowbackPipe["Diameter"] = 10;
            flowbackPipe["Manufacturer"] = "Pumps & Pipes Inc.";
            flowbackPipe.CommitChanges();
        }

        private ABB.Vtrin.cDbClassInstance GetOrCreateEquipmentInstance(
            string instanceName,
            ABB.Vtrin.cDbClass equipmentType)
        {

            // Try to find existing instance by the given name
            var instance = equipmentType.Instances.GetInstanceByName(instanceName);

            // Case: No existing instance found
            // > Create a new one and set info
            if (instance == null)
            {
                instance = equipmentType.Instances.Add();
                instance.Name = instanceName;
                instance.CommitChanges();
            }

            return instance;
        }

        private void createSiteFacade(int? site)
        {
            CreateOrUpdateEquipmentInstances(site);
            if (Context.Group != null)
            {
                addAclEntryForSite(site);
            }
        }

        private void addAclEntryForSite(int? site)
        {
            string topLevelHierarchy = getTopLevelHierarchyPrefix(site);

            var pathCache = Context.Driver.Classes["Path"].Instances;
            var siteRootPath = pathCache.GetInstanceByName(topLevelHierarchy);

            var aclCache = Context.Driver.Classes["UIAccessControlListEntry"].Instances;

            var aclEntrySet = Context.Driver.Classes["UIAccessControlListEntry"].Instances.GetInstanceSet($"Object LIKE '{topLevelHierarchy}'");

            var aclEntry = aclEntrySet.Length > 0 ? aclEntrySet[0]?.BeginUpdate() : null;
            if (aclEntry == null)
            {
                aclEntry = aclCache.Add();
            }

            aclEntry.SetRawPropertyValue("ObjectRef", $"/Path/{siteRootPath.Id}");
            aclEntry["GroupOrUserName"] = Context.Group;
            aclEntry.SetRawPropertyValue("Allow", cDbPermissions.Write | cDbPermissions.Execute);

            aclEntry.CommitChanges();
        }

        private void deleteEquipmentInstances()
        {
            var rootPaths = Context.Driver.Classes["Path"].Instances.GetInstanceSet("Name LIKE 'Example site*' AND Parent = NULL");

            foreach (var path in rootPaths)
            {
                path.Remove();
            }
        }

        private void deleteEquipmentTypes()
        {
            Context.Driver.Classes["Equipment"].Instances.GetInstanceByName("Pipe")?.Remove();
            Context.Driver.Classes["Equipment"].Instances.GetInstanceByName("Tank")?.Remove();
            Context.Driver.Classes["Equipment"].Instances.GetInstanceByName("Pump")?.Remove();
            Context.Driver.Classes["Equipment"].Instances.GetInstanceByName("Mechanical device")?.Remove();
            Context.Driver.Classes["Equipment"].Instances.GetInstanceByName("Electrical device")?.Remove();
            Context.Driver.Classes["Equipment"].Instances.GetInstanceByName("Device")?.Remove();
        }

        private void removeAclEntries()
        {
            var aclEntries = Context.Driver.Classes["UIAccessControlListEntry"].Instances.GetInstanceSet("Object LIKE 'Example site*'");

            foreach (var aclEntry in aclEntries)
            {
                aclEntry.Remove();
            }
        }

        private string getTopLevelHierarchyPrefix(int? site)
        {
            return $"Example site{(site != null ? " " + site : "")}";
        }
    }
}
